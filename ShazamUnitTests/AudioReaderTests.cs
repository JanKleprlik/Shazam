using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shazam;
using Shazam.Audio;

namespace ShazamUnitTests
{
	[TestClass]
	public class AudioReaderTests
	{
		[TestMethod]
		public void Read_Metadata_Sifflet()
		{
			const string path = "Songs/sifflet.wav";

			AudioReader ar = new AudioReader();
			
			IAudioFormat Sound = ar.GetSound(path);


			Assert.AreEqual(1, Sound.Channels);
			Assert.AreEqual(44100, Sound.SampleRate);
			Assert.AreEqual(88200, Sound.ByteRate);
			Assert.AreEqual(2, Sound.BlockAlign);
			Assert.AreEqual(16,Sound.BitsPerSample);
			Assert.AreEqual(882_000, Sound.NumOfDataBytes);
		}

		[TestMethod]
		public void Read_Metadata_World()
		{
			const string path = "Songs/World.wav";

			AudioReader ar = new AudioReader();

			IAudioFormat Sound = ar.GetSound(path);


			Assert.AreEqual(2, Sound.Channels);
			Assert.AreEqual(48000, Sound.SampleRate);
			Assert.AreEqual(192_000, Sound.ByteRate);
			Assert.AreEqual(4, Sound.BlockAlign);
			Assert.AreEqual(16, Sound.BitsPerSample);
			Assert.AreEqual(30_629_888, Sound.NumOfDataBytes);
		}
	}
}
