using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using SFML.System;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;


namespace Shazam
{
	public class Shazam
	{
		private const int windowSize = 4096;
		private const int downSampleCoef = 4;
		private StreamWriter sw = new StreamWriter("output.txt");

		/// <summary>
		/// 
		/// </summary>
		/// <param name="path"></param>
		public void AddNewSong(string path)
		{
			//Server side
			//Plan of audio processing
			//STEREO -> MONO -> LOW PASS -> DOWNSAMPLE -> HAMMING -> FFT

			#region STEREO

			var audio = AudioReader.GetSound(path);

			#endregion

			#region MONO

			if (audio.Channels == 2)  //MONO
				AudioProcessor.StereoToMono(audio);

			#endregion

			#region Short to Double

			double[] data = new double[audio.Data.Length]; //transform shorst to doubles
			for (int i = 0; i < audio.Data.Length; i++) //copy shorts to doubles
			{
				data[i] = audio.Data[i];
			}
			audio.Data = null;//free up memory

			#endregion

			#region LOW PASS & DOWNSAMPLE

			var downsampledData = AudioProcessor.DownSample(data, 4, audio.SampleRate); //LOWPASS + DOWNSAMPLE
			data = null; //release memory
			#endregion

			#region HAMMING & FFT
			//apply FFT at every 1024 samples
			//get 512 bins 
			//of frequencies 0 - 6 kHZ
			//bin size of ~ 11,7 Hz

			double[] HammingWindow = AudioProcessor.GenerateHammingWindow(1024);
			double Avg = GetBinAverage(downsampledData, HammingWindow);

			int offset = 0;
			int bufferSize = windowSize / downSampleCoef;
			data = new double[bufferSize * 2]; //*2  because of Re + Im
			int time = 0;
			while (offset < downsampledData.Length)
			{
				if (offset + bufferSize < downsampledData.Length)
				{
					for (int i = 0; i < bufferSize; i++) //setup for FFT
					{
						data[i * 2] = downsampledData[i + offset] * HammingWindow[i];
						data[i * 2 + 1] = 0d;
					}

					AudioProcessor.FFT(data);

					//get doubles of frequency and time 

					sw.WriteLine($"-----------offset: {offset}-----------seconds: {offset / 12000d}-----------time: {time}");
					var idx = GetStrongestBinIndex(data, 0, 10, Avg);
					sw.WriteLine($"Highest frequency 0 : {idx / 2 * 11.71875}");
					idx = GetStrongestBinIndex(data, 10, 20, Avg);
					sw.WriteLine($"Highest frequency 1 : {idx / 2 * 11.71875}");
					idx = GetStrongestBinIndex(data, 20, 40, Avg);
					sw.WriteLine($"Highest frequency 2 : {idx / 2 * 11.71875}");
					idx = GetStrongestBinIndex(data, 40, 80, Avg);
					sw.WriteLine($"Highest frequency 3 : {idx / 2 * 11.71875}");
					idx = GetStrongestBinIndex(data, 80, 160, Avg);
					sw.WriteLine($"Highest frequency 4 : {idx / 2 * 11.71875}");
					idx = GetStrongestBinIndex(data, 160, 512, Avg);
					sw.WriteLine($"Highest frequency 5 : {idx / 2 * 11.71875}");

				}

				offset += bufferSize;
				time++;
			}

			#endregion
		}

		public string RecognizeSong()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Computes average of strongest bins throughout the whole song for 6 logarithmically scaled sectors of bins. Bins:
		/// <para>0-10</para>
		/// <para>10-20</para>
		/// <para>20-40</para>
		/// <para>40-80</para>
		/// <para>80-160</para>
		/// <para>160-512</para>
		/// </summary>
		/// <param name="data"></param>
		/// <param name="window"></param>
		/// <returns></returns>
		private double GetBinAverage(double[] inputData, double[] window)
		{
			double[] strongestBins = new double[6];
			for (int i = 0; i < strongestBins.Length; i++)
			{
				strongestBins[i] = double.MinValue;
			}

			const int bufferSize = windowSize / downSampleCoef;
			int offset = 0;
			var data = new double[bufferSize * 2]; // * 2 because of Re + Im
			while (offset < inputData.Length)
			{
				if (offset + bufferSize < inputData.Length)
				{
					for (int i = 0; i < bufferSize; i++) //setup for FFT
					{
						data[i * 2] = inputData[i + offset] * window[i]; //re
						data[i * 2 + 1] = 0d; //im
					}

					AudioProcessor.FFT(data);

					//get max of every bin sector
					double[] maxs =
						{
							GetStrongestBin(data, 0, 10),
							GetStrongestBin(data, 10, 20),
							GetStrongestBin(data, 20, 40),
							GetStrongestBin(data, 40, 80),
							GetStrongestBin(data, 80, 160),
							GetStrongestBin(data, 160, 512),
						};

					//set maximum of every bin sector
					for (int i = 0; i < maxs.Length; i++)
					{
						strongestBins[i] = strongestBins[i] < maxs[i] ? maxs[i] : strongestBins[i];
					}
				}
				offset += bufferSize;
			}

			//compute the average of strongest bins
			double sum = 0;
			for (int i = 0; i < strongestBins.Length; i++)
			{
				sum += strongestBins[i];
			}

			return sum / strongestBins.Length;
		}

		private double GetStrongestBin(double[] bins, int from, int to)
		{
			var max = double.MinValue;
			for (int i = from; i < to; i++)
			{
				var normalized = 2 * Math.Sqrt((bins[i * 2] * bins[i * 2] + bins[i * 2 + 1] * bins[i * 2 + 1]) / 2048);
				var decibel = 20 * Math.Log10(normalized);

				if (decibel > max)
				{
					max = decibel;
				}

			}

			return max;
		}

		private int GetStrongestBinIndex(double[] bins, int from, int to, double limit)
		{
			const double coeficient = 1.15;
			var max = double.MinValue;
			int index = 0;
			for (int i = from; i < to; i++)
			{
				var normalized = 2 * Math.Sqrt((bins[i * 2] * bins[i * 2] + bins[i * 2 + 1] * bins[i * 2 + 1]) / 2048);
				var decibel = 20 * Math.Log10(normalized);

				var binMax = Tools.Tools.GetComplexAbs(bins[i * 2], bins[i * 2 + 1]);
				if (decibel > max && decibel * coeficient > limit)
				{
					max = decibel;
					index = i * 2;
				}

			}

			return index;
		}

	}
}
