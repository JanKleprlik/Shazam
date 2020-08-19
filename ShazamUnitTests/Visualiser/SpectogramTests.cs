using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SFML.Graphics;
using SFML.System;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;
using Shazam.Visualiser;
using Shazam.Visualiser.Spectograms;

namespace ShazamUnitTests.Visualiser
{
	[TestClass]
	public class SpectogramTests
	{
		[TestMethod]
		public void Spectogram_Hertz()
		{
			var audio = AudioReader.GetSound("Songs/Hertz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);

			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Spectogram);
			window.Run();
		}

		[TestMethod]
		public void Spectogram_Hertz_Downsampled4()
		{
			var audio = AudioReader.GetSound("Songs/Hertz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);

			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Spectogram, downSampleCoef:4);
			window.Run();
		}


		[TestMethod]
		public void Spectogram_Avicii()
		{
			var audio = AudioReader.GetSound("Songs/Avicii.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);

			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Spectogram);
			window.Run();
		}

		[TestMethod]
		public void Spectogram_Avicii_Downsampled4()
		{
			var audio = AudioReader.GetSound("Songs/Avicii.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);

			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Spectogram, downSampleCoef: 4);
			window.Run();
		}

		[TestMethod]
		public void Spectogram_Coldplay()
		{
			var audio = AudioReader.GetSound("Songs/Coldplay.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);

			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Spectogram);
			window.Run();
		}

		/*/
		[TestMethod]
		public void Spectogram_400Hz()
		{
			const int secInFrame = 60;
			const int windowSize = 4096;

			var hammingWindow = AudioProcessor.GenerateHammingWindow(windowSize);
			var audio = AudioReader.GetSound("Songs/400Hz.wav");
			//var sp = new Spectogram();
			//data for FFT
			double[] data = new double[windowSize * 2]; //*2 because Re + Img
			//samples of audio
			double[] samples = new double[windowSize]; //sampels to take
			//number of seconds rendered in 1 spectogram picture
			int secondsStop = (int)(secInFrame / ((double)windowSize /audio.SampleRate));

			#region render window setup

			var mode = new SFML.Window.VideoMode(1224, 800);
			var window = new SFML.Graphics.RenderWindow(mode, "Spectogram - Hertz test");
			window.SetFramerateLimit(30);

			#endregion

			#region Stereo To Mono
			if (audio.Channels == 2)
			{
				AudioProcessor.StereoToMono(audio);
			}
			#endregion

			int numOfSamples = audio.Data.Length; //set after converting audio to mono
			uint offset = 0;
			while (window.IsOpen)
			{
				for (int i = 0; i < secondsStop; i++)
				{
					if (offset + windowSize < numOfSamples)
					{
						for (int counter = 0; counter < windowSize; counter++) //one line of spectogram
						{
							samples[counter] = audio.Data[offset + counter]; //4k samples
						}
						offset += windowSize;

						//var cutOffData = AudioProcessor.DownSample(samples, 4, audio.SampleRate); //1k samples
						for (int index = 0; index < windowSize; index++)
						{
							//data[index * 2] = cutOffData[index] * hammingWindow[index]; //apply hamming window
							data[index * 2] = samples[index] * hammingWindow[index]; //apply hamming window
							data[index * 2 + 1] = 0d; //set 0s for Img complex values
						}

						AudioProcessor.FFT(data); //apply fft

						//sp.Render(data, new Vector2f(100 + i, 700), 2048);
						//sp.Draw(window);
					}
					else
					{
						break;
					}
				}
				window.Display();
				Thread.Sleep(2000);
				window.Clear();

				// if end of song is reached: quit
				if (offset + windowSize >= audio.Data.Length)
					break;
			}
			window.Close();

		}
		/**/
	}

}
