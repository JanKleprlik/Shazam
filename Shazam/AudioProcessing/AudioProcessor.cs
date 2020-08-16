using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Shazam.AudioFormats;
using Shazam.AudioProcessing.Server;
using Shazam.Extensions;
using Shazam.Visualiser;

namespace Shazam.AudioProcessing
{
	static partial class AudioProcessor
	{
		/// <summary>
		/// Resamples data from two channels to one
		/// </summary>
		/// <param name="audio"></param>
		public static void StereoToMono(IAudioFormat audio)
		{
			IsSupportedFormat(audio);

			if (audio.Channels != 2)
				throw new ArgumentException($"Audio is not stereo.\n Actual number of channels: {audio.Channels}");

			short[] mono = new short[audio.NumOfDataSamples / 2];

			for (int i = 0; i < audio.NumOfDataSamples; i += 2) //4 bytes per loop are processed (2 left + 2 right samples)
			{
				mono[i / 2] = Average(audio.Data[i], audio.Data[i + 1]);
			}

			// number of bytes is halved (Left and Right bytes are merget into one)
			audio.Data = mono; //set new SampleData
			audio.Channels = 1; //lower number of channels
			audio.NumOfDataSamples /= 2;  //lower number of data samples

		}

		public static void DownSample(IAudioFormat audio, int downFactor)
		{
			audio.DataDouble = new double[audio.Data.Length/downFactor];
			
			//filter out frequencies larger than the one that will be available
			//after downsampling by downFactor. To aviod audio aliasing.
			double cutOff = audio.SampleRate / downFactor;
			double[] dataDouble = new double[audio.Data.Length];
			for (int i = 0; i < audio.Data.Length; i++)
			{
				dataDouble[i] = audio.Data[i];
			}
			audio.Data = null; //free up memory

			dataDouble = ButterworthFilter.Butterworth(dataDouble,audio.SampleRate, cutOff);

			//make average of every downFactor number of samples
			for (int i = 0; i < dataDouble.Length / downFactor; i++)
			{
				double sum = 0;
				for (int j = 0; j < downFactor; j++)
				{
					sum += dataDouble[i*downFactor + j];
				}
				audio.DataDouble[i] = sum / downFactor;
			}
		}

		public static double[] DownSample(double[] data, int downFactor, double sampleRate)
		{
			if (downFactor == 0 || downFactor == 1)
				return data;
			var res = new double[data.Length / downFactor];

			//filter out frequencies larger than the one that will be available
			//after downsampling by downFactor. To aviod audio aliasing.
			double cutOff = sampleRate / downFactor;
			double[] dataDouble = new double[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				dataDouble[i] = data[i];
			}

			
			var dataDoubleDownsampled = ButterworthFilter.Butterworth(dataDouble, sampleRate, cutOff); //4k samples

			//make average of every downFactor number of samples
			for (int i = 0; i < dataDoubleDownsampled.Length / downFactor; i++) //1k samples
			{
				double sum = 0;
				for (int j = 0; j < downFactor; j++)
				{
					sum += dataDouble[i * downFactor + j];
				}
				res[i] = sum / downFactor;
			}

			return res;
		}

		#region  FFT
		/// <summary>
		/// Recursive DFT
		/// Currently not working
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Complex[] DFT_Recurr(Complex[] data)
		{
			int n = data.Length;

			if (n == 1)
				return new Complex[] { 1, data[0] };

			Complex[] even = new Complex[n / 2];
			Complex[] odd = new Complex[n / 2];

			for (int i = 0; i < n / 2; i++)
			{
				even[i] = data[i * 2];
				odd[i] = data[i * 2 + 1];
			}

			even = DFT_Recurr(even);
			odd = DFT_Recurr(odd);

			Complex[] bin = new Complex[n];

			Complex[] w = new Complex[n];
			for (int i = 0; i < n; i++)
			{
				double alpha = 2.0 * Math.PI * (double)i / (double)n;
				w[i] = new Complex(Math.Cos(alpha), Math.Sin(alpha));
			}

			for (int i = 0; i < n / 2; i++)
			{
				bin[i] = even[i] + w[i] * odd[i];
				bin[i + n / 2] = even[i] - w[i] * odd[i];
			}

			return bin;
		}

		/// <summary>
		/// forloop DFT
		/// Currently too slow
		/// </summary>
		/// <param name="data"></param>
		public static void DFT(Complex[] data)
		{
			int n = data.Length;
			double alpha;
			Complex[] bin = new Complex[n];

			for (int i = 0; i < n / 2; i++) //for audio, only one half is needed
			{
				alpha = 2.0 * Math.PI * i / (double)n;

				for (int j = 0; j < n; j++)
				{
					bin[i] += new Complex(data[j].Real * Math.Cos(j * alpha) - data[j].Imaginary * Math.Sin(j * alpha),
										data[j].Real * Math.Cos(j * alpha) + data[j].Imaginary * Math.Sin(j * alpha));
				}
			}

			for (int i = 0; i < n / 2; i++)
			{
				data[i] = new Complex(bin[i].Real / n, bin[i].Imaginary / n); //divide by n to scale 
			}
		}

		/// <summary>
		/// FFT specialized for audio
		/// Inspired by LomontFFT <see cref="https://www.lomont.org/software/misc/fft/LomontFFT.cs"/> and classic wikipedia implementation <see cref="https://en.wikipedia.org/wiki/Cooley%E2%80%93Tukey_FFT_algorithm"/>.
		/// Data are stored as real and imaginary doubles alternating.
		/// Data length must be a power of two.
		/// </summary>
		/// <param name="data">Complex valued data stored as doubles.
		/// Alternating between real and imaginary parts.</param>
		/// <exception cref="ArgumentException">data length is not power of two</exception>
		public static void FFT(double[] data)
		{
			int n = data.Length;
			if (!Tools.Tools.IsPowOfTwo(n))
				throw new ArgumentException($"Data length: {n} is not power of two.");

			n /= 2; //data are represented as 1 double for Real part && 1 double for Imaginary part

			BitReverse(data);

			int max = 1;
			while (n > max) // while loop represents logarithm for loop implementation of https://en.wikipedia.org/wiki/Cooley%E2%80%93Tukey_FFT_algorithm
			{
				int step = 2 * max; // 2^s form wiki
									//helper variables for Real and Img separate computations
				double omegaReal = 1;
				double omegaImg = 0;
				double omegaCoefReal = Math.Cos(Math.PI / max);
				double omegaCoefImg = Math.Sin(Math.PI / max);
				for (int m = 0; m < step; m += 2) //2 because of Real + Img double
				{
					//2*n because we have double the amount of data (Re+Img)
					for (int k = m; k < 2 * n; k += 2 * step)
					{
						double tmpReal = omegaReal * data[k + step] - omegaImg * data[k + step + 1]; //t real part from wiki
						double tmpImg = omegaImg * data[k + step] + omegaReal * data[k + step + 1]; //t img part from wiki
																									//A[k+j+m/2] from wiki
						data[k + step] = data[k] - tmpReal;
						data[k + step + 1] = data[k + 1] - tmpImg;
						//A[k+j] from wiki
						data[k] = data[k] + tmpReal;
						data[k + 1] = data[k + 1] + tmpImg;
					}
					//compute new omega
					double tmp = omegaReal;
					omegaReal = omegaReal * omegaCoefReal - omegaImg * omegaCoefImg;
					omegaImg = omegaImg * omegaCoefReal + tmp * omegaCoefImg;
				}

				max = step; //move logarithm loop
			}

			//Normalize(data);
		}
		#endregion
		
		#region  WINDOWS
		public static double[] GenerateHammingWindow(int windowSize)
		{
			var Window = new double[windowSize];

			for (uint i = 0; i < windowSize; i++)
			{
				Window[i] = 0.54 - 0.46 * Math.Cos((2 * Math.PI * i) / windowSize);
			}

			return Window;
		}

		public static double[] GenerateBlackmannHarrisWindow(uint windowSize)
		{
			var Window = new double[windowSize];

			for (uint i = 0; i < windowSize; i++)
			{
				Window[i] = 0.35875 - (0.48829 * Math.Cos((2 * Math.PI * i) / windowSize)) + (0.14128 * Math.Cos((4 * Math.PI * i) / windowSize)) - (0.01168 * Math.Cos((6 * Math.PI * i) / windowSize));
			}

			return Window;
		}

		public static double[] GenerateHannWindow(uint windowSize)
		{
			var Window = new double[windowSize];

			for (uint i = 0; i < windowSize; i++)
			{
				Window[i] = 0.5 * (1 - Math.Cos((2 * Math.PI * i) / windowSize));
			}

			return Window;
		}
		#endregion


	}
}
