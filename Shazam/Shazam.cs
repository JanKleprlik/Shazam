using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using SFML.Audio;
using SFML.System;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;

[assembly: InternalsVisibleTo("ShazamUnitTests")]
namespace Shazam
{
	public class Shazam
	{
		private const int windowSize = 4096;
		private const int downSampleCoef = 4;
		private const int targetZoneSize = 5;
		private const int anchorOffset = 2;


		private Dictionary<Address, List<TargetZone>> database = new Dictionary<Address, List<TargetZone>>();
		/// <summary>
		/// Bits are from left to right
		/// <para>Key:</para>
		/// <para> 9 bits =  frequency of anchor</para>
		/// <para> 9 bits =  frequency of point</para>
		/// <para> 14 bits =  delta</para>
		/// <para>Value:</para>
		/// <para>32 bits absolute time of anchor</para>
		/// <para>32 bits id of a song</para>
		/// </summary>
		private Dictionary<uint, List<ulong>> databaseSmall = new Dictionary<uint, List<ulong>>();


		/// <summary>
		/// Add new song to the database
		/// </summary>
		/// <param name="path"></param>
		public void AddNewSong(string path, uint songID)
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

			double[] data = ShortArrayToDoubleArray(audio.Data);
			
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

			int bufferSize = windowSize / downSampleCoef; //default 4096/4 = 1024
			var TimeFrequencyPoitns = CreateTimeFrequencyPoints(bufferSize, downsampledData, coefficient:1);
			/*/
			for (int i = 0; i < TimeFrequencyPoitns.Count; i++)
			{
				Console.WriteLine($"index: {i}  --- time: {TimeFrequencyPoitns[i].Item1} --- frequency: {TimeFrequencyPoitns[i].Item2}");
			}
			/**/
			AddTFpointsToDatabase(TimeFrequencyPoitns, songID);



			#endregion
		}



		public string RecognizeSong()
		{
			double[] data;

			#region Sound recording

			using (var recorder = new SoundBufferRecorder())
			{
				recorder.ChannelCount = 1;
				recorder.GetDevice();
				recorder.Start(sampleRate: 12000);

				Task waiter = new Task(() => Thread.Sleep(10000)); //record for 10 secs
				waiter.Start();
				waiter.Wait();

				recorder.Stop();

				if (recorder.SoundBuffer.ChannelCount != 1)
				{
					throw new ConstraintException($"Channel count for recording is invalid.\n Expected: 1\n Actual: {recorder.SoundBuffer.ChannelCount}");
				}

				if (recorder.SoundBuffer.SampleRate != 12000)
				{
					throw new ConstraintException($"Sampling rate for recording is invalid.\n Expected: 12000\n Actual: {recorder.SoundBuffer.SampleRate}");
				}

				data = ShortArrayToDoubleArray(recorder.SoundBuffer.Samples);
			}

			#endregion

			List<Tuple<uint, uint>> timeFrequencyPoints;

			#region Creating Time-frequency points
			int bufferSize = windowSize / downSampleCoef;
			timeFrequencyPoints= CreateTimeFrequencyPoints(bufferSize, data, coefficient:1.1); //set higher coefficient because microphone has lower sensitivity
			#endregion

			/*/
			for (int i = 0; i < timeFrequencyPoints.Count; i++)
			{
				Console.WriteLine($"index: {i}  --- time: {timeFrequencyPoints[i].Item1} --- frequency: {timeFrequencyPoints[i].Item2}");
			}
			/**/

			//[address;(AbsAnchorTimes)]
			Dictionary<uint, List<uint>> recordAddresses = CreateRecordAddresses(timeFrequencyPoints);

			var PotentialDoubles = GetPotentialSongs(recordAddresses);


			//check time coherency




			return "Idk really";
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="recordAddresses">Key: Address, Value: AbsRecordAnchorTime</param>
		/// <returns>[SongID, [SongAddress, &lt;AbsSongAnchorTime;Number of records&gt;]]</returns>
		private Dictionary<uint, Dictionary<uint, int>> GetPotentialSongs(Dictionary<uint, List<uint>> recordAddresses)
		{
			// [SongID, [SongAddress, <AbsSongAnchorTime;Number of records>]]
			Dictionary<uint, Dictionary<uint, int>> songTargetZones = new Dictionary<uint, Dictionary<uint, int>>();

			// [SongAddress, num. of occurence]
			Dictionary<uint, int> AddressQts = new Dictionary<uint, int>();
			foreach (var KeyVal in recordAddresses)
			{
				if (databaseSmall.ContainsKey(KeyVal.Key))
				{
					foreach (var Val in databaseSmall[KeyVal.Key])
					{
						
						uint SongId = (uint)Val;
						uint AbsAnchorTime = (uint) (Val >> 32);

						if (!songTargetZones.ContainsKey(SongId)) //if such song doesnt have any record yet, initialize it
						{
							songTargetZones.Add(SongId, new Dictionary<uint, int>());
						}
						if (!songTargetZones[SongId].ContainsKey(AbsAnchorTime)) //if anch. time doesnt exist yet, initialize it
						{
							songTargetZones[SongId].Add(AbsAnchorTime, 0);
						}
						songTargetZones[SongId][AbsAnchorTime]++; //update number of occurences
					}
				}
			}
#if DEBUG
			foreach (var song in songTargetZones)
			{
				var a = from ocr in song.Value where ocr.Value >= 5 select ocr;
				Debug.WriteLine($"Song: {song.Key} has {song.Value.Count} occurences with {a.Count()} target zones.");
			}
#endif

			foreach (var songID_Dict in songTargetZones)
			{
				int counter = 0; //counter for number of samples in 
				foreach (var AbsAnchTime_Occurece in songID_Dict.Value)
				{
					counter++;
					if (AbsAnchTime_Occurece.Value < 5) //remove if occurence < 5
					{
						songTargetZones[songID_Dict.Key].Remove(AbsAnchTime_Occurece.Key);
						counter--;
						continue;
					}
				}

				//maybe change to have over half of samples in TGZ
				if (counter < 250) //remove song if only few target zones are found 
				{
					//280 = 300 * noiseCoefficient
					//300 because: approx 3 samples (from parts of freq. spectrum) per ~0,1 song for 10 seconds
					songTargetZones.Remove(songID_Dict.Key);
				}
			}

			return songTargetZones;
		}

		private Dictionary<uint, List<uint>> CreateRecordAddresses(List<Tuple<uint, uint>> timeFrequencyPoints)
		{
			Dictionary<uint, List<uint>> res = new Dictionary<uint, List<uint>>();


			// -targetZoneSize: because of end limit 
			// -1: because of anchor point at -2 position target zone
			int stopIdx = timeFrequencyPoints.Count - targetZoneSize - anchorOffset;
			for (int i = 0; i < stopIdx; i++)
			{
				//anchor is at idx i
				//1st in TZ is at idx i+3
				//5th in TZ is at idx i+7

				uint anchorFreq = timeFrequencyPoints[i].Item2;
				uint anchorTime = timeFrequencyPoints[i].Item1;
				for (int pointNum = 3; pointNum < targetZoneSize + 3; pointNum++)
				{
					uint pointFreq = timeFrequencyPoints[i + pointNum].Item2;
					uint pointTime = timeFrequencyPoints[i + pointNum].Item1;

					uint address = BuildAddress(anchorFreq, pointFreq, pointTime - anchorTime);

					if (!res.ContainsKey(address)) //create new instance if it doesnt exist
					{
						res.Add(address, new List<uint>() { anchorTime});
					}
					else //add Anchor time to the list
					{
						res[address].Add(anchorTime);
					}
				}

			}

			return res;
		}


		/// <summary>
		/// Applies Hamming window and then FFT at every <c>bufferSize</c> number of samples.
		/// Filters out strongest bins and creates Time-frequency points that are ordered. Primarly by time, secondary by frequency.
		/// Both in ascending manner.
		/// </summary>
		/// <param name="bufferSize">Size of a window FFT will be applied to.</param>
		/// <param name="data">Data FFT will be applied to.</param>
		/// <returns></returns>
		private List<Tuple<uint, uint>> CreateTimeFrequencyPoints(int bufferSize, double[] data, double coefficient = 0.9)
		{
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

					AudioProcessor.FFT(sampleData);
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
					RegisterTFPoints(sampleData, Avg, AbsTime,ref TimeFrequencyPoitns, coefficient);

				}

				offset += bufferSize;
				AbsTime++;
			}

			return TimeFrequencyPoitns;
		}

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

		private void AddTFpointsToDatabase(List<Tuple<uint, uint>> timeFrequencyPoints, in uint songId)
		{
			/* spectogram:
			 *
			 * |
			 * |       X X
			 * |         X
			 * |     X     X
			 * |   X         X
			 * | X X X     X
			 * x----------------
			 */


			// -targetZoneSize: because of end limit 
			// -1: because of anchor point at -2 position target zone
			int stopIdx = timeFrequencyPoints.Count - targetZoneSize - anchorOffset; 
			for (int i = 0; i < stopIdx; i++)
			{
				//anchor is at idx i
				//1st in TZ is at idx i+3
				//5th in TZ is at idx i+7

				uint anchorFreq = timeFrequencyPoints[i].Item2;
				uint anchorTime = timeFrequencyPoints[i].Item1;
				ulong SongValue = BuildSongValue(anchorTime, songId);
				for (int pointNum = 3; pointNum < targetZoneSize + 3; pointNum++)
				{
					uint pointFreq = timeFrequencyPoints[i + pointNum].Item2;
					uint pointTime = timeFrequencyPoints[i + pointNum].Item1;

					uint address = BuildAddress(anchorFreq, pointFreq, pointTime - anchorTime);

					if (!databaseSmall.ContainsKey(address)) //create new instance if it doesnt exist
					{
						databaseSmall.Add(address, new List<ulong>(){SongValue});
					}
					else //add SongValue to the list of
					{
						databaseSmall[address].Add(SongValue);
					}
				}

			}
		}

		internal uint BuildAddress(in uint anchorFreq, in uint pointFreq, uint delta)
		{
			uint res = anchorFreq;
			res = res << 9; //move 9 bits 
			res += pointFreq;
			res = res << 14; //move 14 bits 
			res += delta;
			return res;
		}

		internal ulong BuildSongValue(in uint absAnchorTime, uint id)
		{
			ulong res = absAnchorTime;
			res = res << 32;
			res += id;
			return res;
		}

		/// <summary>
		/// Filter outs the strongest bins of logarithmically scaled parts of bins. Chooses the strongest and remembers it if its value is above average. Those points are
		/// chornologically added to the <c>timeFrequencyPoints</c> List.
		/// </summary>
		/// <param name="data">bins to choose from, alternating real and complex values as doubles. Must contain 512 complex values</param>
		/// <param name="average">Limit that separates weak spots from important ones.</param>
		/// <param name="absTime">Absolute time in the song.</param>
		/// <param name="timeFrequencyPoitns">List to add points to.</param>
		private void RegisterTFPoints(double[] data, in double average, in uint absTime,ref List<Tuple<uint, uint>> timeFrequencyPoitns, double coefficient = 0.9)
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
					timeFrequencyPoitns.Add(new Tuple<uint, uint>(absTime, (uint) idx/2));
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

		private int? GetStrongestBinIndex(double[] bins, int from, int to, double limit, double coeficient = 0.9d)
		{
			var max = double.MinValue;
			int? index = null;
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
