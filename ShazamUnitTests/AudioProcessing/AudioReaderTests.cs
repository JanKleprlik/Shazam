using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shazam;
using Shazam.AudioFormats;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;
namespace ShazamUnitTests
{
	[TestClass]
	public class AudioReaderTests
	{
		[TestMethod]
		public void Read_Metadata_Hertz()
		{
			const string path = "Songs/Hertz.wav";

			
			IAudioFormat Sound = AudioReader.GetSound(path);


			Assert.AreEqual((uint)1, Sound.Channels);
			Assert.AreEqual((uint)48000, Sound.SampleRate);
			Assert.AreEqual(96000, Sound.ByteRate);
			Assert.AreEqual(2, Sound.BlockAlign);
			Assert.AreEqual(16,Sound.BitsPerSample);
			Assert.AreEqual(6_909_144, Sound.NumOfDataSamples);
			Assert.AreEqual(Sound.NumOfDataSamples, Sound.Data.Length);
		}

		[TestMethod]
		public void Read_Metadata_World()
		{
			const string path = "Songs/World.wav";


			IAudioFormat Sound = AudioReader.GetSound(path);


			Assert.AreEqual((uint)2, Sound.Channels);
			Assert.AreEqual((uint)48000, Sound.SampleRate);
			Assert.AreEqual(192_000, Sound.ByteRate);
			Assert.AreEqual(4, Sound.BlockAlign);
			Assert.AreEqual(16, Sound.BitsPerSample);
			Assert.AreEqual(15_314_944, Sound.NumOfDataSamples);
			Assert.AreEqual(Sound.NumOfDataSamples, Sound.Data.Length);

		}


	}
}
