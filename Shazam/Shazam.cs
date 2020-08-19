using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;

namespace Shazam
{
	public class Shazam
	{
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

			double[] data = new double[audio.Data.Length]; //transform shorst to doubles
			for (int i = 0; i < audio.Data.Length; i++) //copy shorts to doubles
			{
				data[i] = audio.Data[i];
			}
			audio.Data = null;//free up memory

			#region LOW PASS & DOWNSAMPLE

			var downsampledData = AudioProcessor.DownSample(data, 4, audio.SampleRate); //LOWPASS + DOWNSAMPLE
			data = null; //release memory
			#endregion

			#region HAMMING
			//applyWindow(downsampledData, 1024);
			#endregion

			#region FFT
			//apply FFT at every 1024 samples
			//get 512 bins 
			//of frequencies 0 - 6 kHZ
			//bin size of ~ 11,7 Hz
			double[] HammingWindow = AudioProcessor.GenerateHammingWindow(1024);
			int offset = 0;
			data = new double[2048];
			while (offset < downsampledData.Length)
			{
				if (offset + 1024 < downsampledData.Length)
				{
					for (int i = 0; i < 1024; i++) //setup for FFT
					{
						data[i * 2] = downsampledData[i + offset] * HammingWindow[i];
						data[i * 2 + 1] = 0d;
					}

					AudioProcessor.FFT(data);

					double sum = 0;
					sum += getStrongestBin(data, 3, 10);
					sum += getStrongestBin(data, 10, 20);
					sum += getStrongestBin(data, 20, 40);
					sum += getStrongestBin(data, 40, 80);
					sum += getStrongestBin(data, 80, 160);
					sum += getStrongestBin(data, 160, 512);
					sum /= 5;
					Console.WriteLine($"-----------{offset}-----------");
					var idx = getStrongestBinIndex(data,3, 10, sum);
					Console.WriteLine($"Highest frequency 0 : {idx / 2 * 11.71875}");
					idx = getStrongestBinIndex(data, 10, 20, sum);
					Console.WriteLine($"Highest frequency 1 : {idx/2*11.71875}");
					idx = getStrongestBinIndex(data, 20, 40, sum);
					Console.WriteLine($"Highest frequency 2 : {idx/2* 11.71875}");
					idx = getStrongestBinIndex(data, 40, 80, sum);
					Console.WriteLine($"Highest frequency 3 : {idx/2* 11.71875}");
					idx = getStrongestBinIndex(data, 80, 160, sum);
					Console.WriteLine($"Highest frequency 4 : {idx/2* 11.71875}");
					idx = getStrongestBinIndex(data, 160, 512, sum);
					Console.WriteLine($"Highest frequency 5 : {idx/2* 11.71875}");

				}

				offset += 1024;
			}










			//sets strongest bins from the whole song multiplied by coeficient
			//strongestBins = setStrongestBins(downsampledData, 1d );

			//compute avg of strongest bins
			//var avg = getAvg(strongestBins);

			//filter out only the strongest bins to make fingerprint

			//var filteredData = filterBins(downsampledData, avg);





			#endregion

		}

		public string RecognizeSong()
		{
			throw new NotImplementedException();
		}


		private void applyWindow(double[] data, int windowSize)
		{
			var window = AudioProcessor.GenerateHammingWindow(windowSize);
			for (int offset = 0; offset < data.Length;) //HAMMING
			{
				if (offset + windowSize < data.Length) //for every 1024 apply window function
				{
					for (int j = 0; j < windowSize; j++)
					{
						data[offset + j] = window[j];
					}
				}
				else    //apply Hamming window for the rest of the song that is smaller than 1024 samples
				{
					int restSize = data.Length - offset;
					var restWindow = AudioProcessor.GenerateHammingWindow(restSize);
					for (int j = 0; j < restSize; j++)
					{
						data[offset + j] = restWindow[j];
					}
				}
				offset += windowSize;
			}
		}




		private double[] strongestBins = new double[6];

		/// <summary>
		/// Sets strongest bins for each logarithmic band.
		/// </summary>
		/// <param name="data"></param>
		private double[] setStrongestBins(double[] data, double coeficient) 
		{
			/*/
			 * 6 sectors 
			 * BINS:	0 - 10
			 *			10 - 20
			 *			20 - 40
			 *			40 - 80
			 *			80 - 160
			 *			160 - 512
			/**/

			//initialize 
			var strongestBins = new double[6];
			for (int i = 0; i < 6; i++)
			{
				strongestBins[i] = double.MinValue;
			}

			int offset = 0;
			var dataFFT = new double[1024 * 2];
			while (offset + 1024 < data.Length) //do FFT
			{

				for (int i = 0; i < 1024; i++) //prepare data - insert 0s
				{
					dataFFT[i * 2] = data[offset + i];
					dataFFT[i * 2 + 1] = 0d;
				}
				offset += 1024;

				AudioProcessor.FFT(dataFFT); //i need only first 512 bins (other half is symettrical) (acutally its 1024 (Re + Im))


				var max = getStrongestBin(dataFFT, 3, 10);
				strongestBins[0] = strongestBins[0] < max ? max : strongestBins[0];

				max = getStrongestBin(data, 10, 20);
				strongestBins[1] = strongestBins[1] < max ? max : strongestBins[1];

				max = getStrongestBin(data, 20, 40);
				strongestBins[2] = strongestBins[2] < max ? max : strongestBins[2];

				max = getStrongestBin(data, 40, 80);
				strongestBins[3] = strongestBins[3] < max ? max : strongestBins[3];

				max = getStrongestBin(data, 80, 160);
				strongestBins[4] = strongestBins[4] < max ? max : strongestBins[4];

				max = getStrongestBin(data, 160, 512);
				strongestBins[5] = strongestBins[5] < max ? max : strongestBins[5];
				
			}

			//multiply by coeficient
			for (int i = 0; i < strongestBins.Length; i++)
			{
				strongestBins[i] *= coeficient;
			}


			return strongestBins;

		}

		private double getStrongestBin(double[] bins, int from, int to)
		{
			var max = double.MinValue;
			for (int i = from; i < to; i++)
			{
				var normalized = 2 * Math.Sqrt((bins[i*2] * bins[i * 2] + bins[i * 2+1] * bins[i * 2+1]) / 2048);
				var decibel = 20 * Math.Log10(normalized);

				var binMax = Tools.Tools.GetComplexAbs(bins[i * 2], bins[i * 2 + 1]);
				if (decibel > max)
				{
					max = decibel;
				}

			}

			return max;
		}
		private int getStrongestBinIndex(double[] bins, int from, int to, double limit)
		{
			var max = double.MinValue;
			int index = 0;
			for (int i = from; i < to; i++)
			{
				var normalized = 2 * Math.Sqrt((bins[i * 2] * bins[i * 2] + bins[i * 2 + 1] * bins[i * 2 + 1]) / 2048);
				var decibel = 20 * Math.Log10(normalized);

				var binMax = Tools.Tools.GetComplexAbs(bins[i * 2], bins[i * 2 + 1]);
				if (decibel > max && decibel > limit)
				{
					max = decibel;
					index = i*2;
				}

			}

			return index;
		}
		private double getAvg(double[] data)
		{
			double sum = 0;
			for (int i = 0; i < data.Length; i++)
			{
				sum += data[i];
			}

			return sum / data.Length;
		}

		private List<double> filterBins(double[] data, double limit)
		{
			List<double> filteredData = new List<double>();
			int offset = 0;
			var dataFFT = new double[1024 * 2];

			while (offset + 1024 < data.Length) {

				for (int i = 0; i < 1024; i++) //prepare data - insert 0s
				{
					dataFFT[i * 2] = data[offset + i];
					dataFFT[i * 2 + 1] = 0d;
				}
				offset += 1024;

				AudioProcessor.FFT(dataFFT); //i need only first 512 bins (other half is symettrical) (acutally its 1024 (Re + Im))


				var strongestBins = new double[6];
				var max = getStrongestBin(dataFFT, 3, 10);
				strongestBins[0] = strongestBins[0] < max ? max : strongestBins[0];

				max = getStrongestBin(data, 10, 20);
				strongestBins[1] = strongestBins[1] < max ? max : strongestBins[1];

				max = getStrongestBin(data, 20, 40);
				strongestBins[2] = strongestBins[2] < max ? max : strongestBins[2];

				max = getStrongestBin(data, 40, 80);
				strongestBins[3] = strongestBins[3] < max ? max : strongestBins[3];

				max = getStrongestBin(data, 80, 160);
				strongestBins[4] = strongestBins[4] < max ? max : strongestBins[4];

				max = getStrongestBin(data, 160, 512);
				strongestBins[5] = strongestBins[5] < max ? max : strongestBins[5];


				//save only data above avg
				for (int i = 0; i < strongestBins.Length; i++)
				{
					if (strongestBins[i] >= limit)
						filteredData.Add(strongestBins[i]);
				}
			}

			return filteredData;

		}
	}
}
