using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SFML.Audio;
using Shazam.AudioProcessing;
[assembly: InternalsVisibleTo("ShazamUnitTests")]
namespace Shazam
{
	public partial class Shazam
	{

		/// <summary>
		/// <para>Records audio and returns data as array of doubles</para>
		/// <para>WARNING: device must be able to records mono at 12kHz sample rate.</para>
		/// </summary>
		/// <param name="length">Length of the recording in milliseconds.</param>
		/// <returns></returns>
		private double[] RecordAudio(int length)
		{
			double[] data;
			using (var recorder = new SoundBufferRecorder())
			{
				recorder.ChannelCount = 1;
				recorder.GetDevice();
				recorder.Start(sampleRate: 12000);

				Task waiter = new Task(() => Thread.Sleep(length)); //record for 10 secs
				waiter.Start();

				while (!waiter.IsCompleted)
				{
					Console.WriteLine("{0,2} {1}", "", "Recording audio...");
					Thread.Sleep(2000);
				}

				recorder.Stop();

				//check number of channels and sampling rate
				if (recorder.SoundBuffer.ChannelCount != 1)
				{
					throw new ConstraintException($"Channel count for recording is invalid.\n Expected: 1\n Actual: {recorder.SoundBuffer.ChannelCount}");
				}

				if (recorder.SoundBuffer.SampleRate != 12000)
				{
					throw new ConstraintException($"Sampling rate for recording is invalid.\n Expected: 12000\n Actual: {recorder.SoundBuffer.SampleRate}");
				}

				data = ShortArrayToDoubleArray(recorder.SoundBuffer.Samples);
				recorder.SoundBuffer.Dispose(); //dispose buffer manually
			}

			return data;
		}

		/// <summary>
		/// Applies Hamming window and then FFT at every <c>bufferSize</c> number of samples.
		/// Filters out strongest bins and creates Time-frequency points that are ordered. Primarly by time, secondary by frequency.
		/// Both in ascending manner.
		/// </summary>
		/// <param name="bufferSize">Size of a window FFT will be applied to.</param>
		/// <param name="data">Data FFT will be applied to.</param>
		/// <returns></returns>
		private List<Tuple<uint, uint>> CreateTimeFrequencyPoints(int bufferSize, double[] data, double sensitivity = 0.9)
		{
            Contract.Requires(bufferSize > 0);
            Contract.Requires(data != null);
            Contract.Requires(sensitivity > 0);
            Contract.Requires(sensitivity <= 1);

            List<Tuple<uint, uint>> TimeFrequencyPoitns = new List<Tuple<uint, uint>>();
			double[] HammingWindow = AudioProcessor.GenerateHammingWindow(bufferSize);
			double Avg = 0d;// = GetBinAverage(data, HammingWindow);

			int offset = 0;
			var sampleData = new double[bufferSize * 2]; //*2  because of Re + Im
			uint AbsTime = 0;
			while (offset < data.Length)
			{
				if (offset + bufferSize < data.Length)
				{
					for (int i = 0; i < bufferSize; i++) //setup for FFT
					{
						sampleData[i * 2] = data[i + offset] * HammingWindow[i];
						sampleData[i * 2 + 1] = 0d;
					}

					FastFourierTransformation.FFT(sampleData);
					double[] maxs =
					{
						GetStrongestBin(data, 0, 10),
						GetStrongestBin(data, 10, 20),
						GetStrongestBin(data, 20, 40),
						GetStrongestBin(data, 40, 80),
						GetStrongestBin(data, 80, 160),
						GetStrongestBin(data, 160, 512),
					};


					for (int i = 0; i < maxs.Length; i++)
					{
						Avg += maxs[i];
					}

					Avg /= maxs.Length;
					//get doubles of frequency and time 
					RegisterTFPoints(sampleData, Avg, AbsTime, ref TimeFrequencyPoitns, sensitivity);

				}

				offset += bufferSize;
				AbsTime++;
			}

			return TimeFrequencyPoitns;
		}
		
		/// <summary>
		/// Filter outs the strongest bins of logarithmically scaled parts of bins. Chooses the strongest and remembers it if its value is above average. Those points are
		/// chornologically added to the <c>timeFrequencyPoints</c> List.
		/// </summary>
		/// <param name="data">bins to choose from, alternating real and complex values as doubles. Must contain 512 complex values</param>
		/// <param name="average">Limit that separates weak spots from important ones.</param>
		/// <param name="absTime">Absolute time in the song.</param>
		/// <param name="timeFrequencyPoitns">List to add points to.</param>
		private void RegisterTFPoints(double[] data, in double average, in uint absTime, ref List<Tuple<uint, uint>> timeFrequencyPoitns, double coefficient = 0.9)
		{
			int[] BinBoundries =
			{
				//low   high
				0 , 10,
				10, 20,
				20, 40,
				40, 80,
				80, 160,
				160,512
			};

			//loop through logarithmically scalled sections of bins
			for (int i = 0; i < BinBoundries.Length / 2; i++)
			{
				//get strongest bin from a section if its above average
				var idx = GetStrongestBinIndex(data, BinBoundries[i * 2], BinBoundries[i * 2 + 1], average, coefficient);
				if (idx != null)
				{
					//idx is divided by 2 because of (Re + Im)
					timeFrequencyPoitns.Add(new Tuple<uint, uint>(absTime, (uint)idx / 2));
				}
			}
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

			const int bufferSize = Constants.WindowSize / Constants.DownSampleCoef;
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

					FastFourierTransformation.FFT(data);

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

		/// <summary>
		/// Returns normalized value of the strongest bin in given bounds
		/// </summary>
		/// <param name="bins">Complex values alternating Real and Imaginary values</param>
		/// <param name="from">lower bound</param>
		/// <param name="to">upper bound</param>
		/// <returns>Normalized value of the strongest bin</returns>
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

		/// <summary>
		/// Finds the strongest bin above limit in given segment.
		/// </summary>
		/// <param name="bins">Complex values alternating Real and Imaginary values</param>
		/// <param name="from">lower bound</param>
		/// <param name="to">upper bound</param>
		/// <param name="limit">limit indicating weak bin</param>
		/// <param name="sensitivity">sensitivity of the limit (the higher the lower sensitivity)</param>
		/// <returns>index of strongest bin or null if none of the bins is strong enought</returns>
		private int? GetStrongestBinIndex(double[] bins, int from, int to, double limit, double sensitivity = 0.9d)
		{
			var max = double.MinValue;
			int? index = null;
			for (int i = from; i < to; i++)
			{
				var normalized = 2 * Math.Sqrt((bins[i * 2] * bins[i * 2] + bins[i * 2 + 1] * bins[i * 2 + 1]) / 2048);
				var decibel = 20 * Math.Log10(normalized);

				if (decibel > max && decibel * sensitivity > limit)
				{
					max = decibel;
					index = i * 2;
				}

			}

			return index;
		}


		#region simple helpers

		/// <summary>
		/// Converts array of shorts to array of doubles
		/// </summary>
		/// <param name="audioData"></param>
		/// <returns></returns>
		private double[] ShortArrayToDoubleArray(short[] audioData)
		{
			double[] res = new double[audioData.Length]; //allocate new memory
			for (int i = 0; i < audioData.Length; i++) //copy shorts to doubles
			{
				res[i] = audioData[i];
			}
			audioData = null; //free up memory
			return res;
		}

		/// <summary>
		/// Builds address from parts
		/// </summary>
		/// <param name="anchorFreq">Frequency of anchor point</param>
		/// <param name="pointFreq">Frequency of Self point</param>
		/// <param name="delta">Time delta between Anchor and Self point</param>
		/// <returns>Left to right: 9bits Anchor frequency, 9bits Self point frequency, 14 bits delta</returns>
		internal static uint BuildAddress(in uint anchorFreq, in uint pointFreq, uint delta)
		{
			uint res = anchorFreq;
			res <<= 9; //move 9 bits 
			res += pointFreq;
			res <<= 14; //move 14 bits 
			res += delta;
			return res;
		}

		/// <summary>
		/// Builds song value out of parts
		/// </summary>
		/// <param name="absAnchorTime">Absolute time of anchor</param>
		/// <param name="id">ID of a song</param>
		/// <returns>Left to right: 32bits AbsAnchTime, 32 bits songID</returns>
		internal static ulong BuildSongValue(in uint absAnchorTime, uint id)
		{
			ulong res = absAnchorTime;
			res <<= 32;
			res += id;
			return res;
		}
		
		#endregion
	}
}
