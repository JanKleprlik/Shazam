using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;
using Shazam.Visualiser;

namespace ShazamUnitTests.Visualiser
{
	[TestClass]
	public class FrequenciesTests
	{
		[TestMethod]
		public void FrequenciesVisualizes_Hertz()
		{
			var audio = AudioReader.GetSound("Songs/Hertz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies);
			window.Run();
		}
		[TestMethod]
		public void FrequenciesVisualizes_Piano()
		{
			var audio = AudioReader.GetSound("Songs/Piano.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies);
			window.Run();
		}
		[TestMethod]
		public void Frequency_10500Hz()
		{
			var audio = AudioReader.GetSound("Songs/10500Hz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies);
			window.Run();
		}

		[TestMethod]
		public void Frequency_10500Hz_DownsampledFourTimes()
		{
			var audio = AudioReader.GetSound("Songs/10500Hz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies, downSampleCoef: 4);
			window.Run();
		}

		[TestMethod]
		public void Frequency_Hertz_DownsampledFourTimes()
		{
			var audio = AudioReader.GetSound("Songs/Hertz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies, downSampleCoef: 4);
			window.Run();
		}

		[TestMethod]
		public void Frequency_400Hz()
		{
			var audio = AudioReader.GetSound("Songs/400Hz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies);
			window.Run();
		}

		[TestMethod]
		public void Frequency_Avicii()
		{
			var audio = AudioReader.GetSound("Songs/Avicii.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Frequencies);
			window.Run();
		}
	}
}
