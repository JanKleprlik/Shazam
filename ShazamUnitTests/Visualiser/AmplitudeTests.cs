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
	public class AmplitudeTests
	{
		[TestMethod]
		public void AmplitudeVisualiser_Hertz()
		{
			var audio = AudioReader.GetSound("Songs/Hertz.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Amplitude);
			window.Run();
		}


		[TestMethod]
		public void AmplitudeVisualiser_World()
		{
			var audio = AudioReader.GetSound("Songs/World.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Amplitude);
			window.Run();
		}

		[TestMethod]
		public void AmplitudeVisualiser_Avicii()
		{
			var audio = AudioReader.GetSound("Songs/Avicii.wav");
			if (audio.Channels == 2)
				AudioProcessor.StereoToMono(audio);
			var window = new Visualizer(audio.Data, audio.Channels, audio.SampleRate, VisualisationModes.Amplitude);
			window.Run();
		}

	}
}
