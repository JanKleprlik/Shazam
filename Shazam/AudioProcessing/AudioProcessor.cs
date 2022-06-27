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
using System.Diagnostics.Contracts;

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
            Contract.Requires(audio.Channels == 2);
            Contract.Requires(audio.Data.Length % 2 == 0);
            Contract.Requires(audio.Data.Length == audio.NumOfDataSamples);
            Contract.Ensures(audio.Channels == 1);
            Contract.Ensures(audio.Data.Length % 2 == 0);
            Contract.Ensures(audio.Data.Length * 2 == Contract.OldValue(audio.Data.Length));
            Contract.Ensures(audio.NumOfDataSamples * 2== Contract.OldValue(audio.Data.Length));
            Contract.Ensures(audio.NumOfDataSamples == audio.Data.Length);

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

		/// <summary>
		/// Downsamples data by a <c>downFactor</c>
		/// </summary>
		/// <param name="data">data to downsample</param>
		/// <param name="downFactor">factor of downsampling</param>
		/// <param name="sampleRate">original sample rate</param>
		/// <returns>Downsampled data</returns>
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
