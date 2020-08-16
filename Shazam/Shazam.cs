using System;
using System.Collections.Generic;
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
			var window = AudioProcessor.GenerateHammingWindow(1024);
			for (int offset = 0; offset < downsampledData.Length;) //HAMMING
			{
				if (offset + 1024 < downsampledData.Length)	//for every 1024 apply window function
				{
					for (int j = 0; j < 1024; j++)
					{
						downsampledData[offset + j] = window[j]; 
					}
				}
				else	//apply Hamming window for the rest of the song that is smaller than 1024 samples
				{
					int restSize = downsampledData.Length - offset;
					var restWindow = AudioProcessor.GenerateHammingWindow(restSize);
					for (int j = 0; j < restSize; j++)
					{
						downsampledData[offset + j] = restWindow[j];
					}
				}
				offset += 1024;
			}

			#endregion


			#region FFT
			//apply FFT at every 1024 samples
			//get 512 bins 
			//of frequencies 0 - 6 kHZ
			//bin size of ~ 11,7 Hz

			{//ew
				int offset = 0;
				var dataFFT = new double[1024 * 2];
				while (offset < downsampledData.Length)
				{

					for (int i = 0; i < 1024; i++) //prepare data - insert 0s
					{
						dataFFT[i * 2] = downsampledData[offset + i];
						dataFFT[i * 2 + 1] = 0d;
					}
					offset += 1024;

					AudioProcessor.FFT(dataFFT); //i need only first 512 bins (other half is symettrical) (acutally its 1024 (Re + Im)


				}
			}




			#endregion

		}

		public string RecognizeSong()
		{
			throw new NotImplementedException();
		}
	}
}
