using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shazam.AudioFormats;
using Shazam.AudioProcessing;

namespace ShazamUnitTests.AudioProcessing
{
	[TestClass]
	public class AudioProcessorTests
	{

		[TestMethod]
		public void StereoToMono_OddValules()
		{
			short[] sampleShorts = { 1657, 11 };
			IAudioFormat sound = new WavFormat();
			sound.Data = sampleShorts;
			sound.NumOfDataSamples = 2;
			sound.SampleRate = 48000;
			sound.BlockAlign = 4;
			sound.BitsPerSample = 16;
			sound.Channels = 2;


			AudioProcessor.StereoToMono(sound);

			short[] correctShorts = { 834 };

			Assert.AreEqual(correctShorts.Length, sound.Data.Length);
			for (int i = 0; i < correctShorts.Length; i++)
			{
				Assert.AreEqual(correctShorts[i], sound.Data[i]);
			}
		}

		[TestMethod]
		public void StereoToMono_EvenValules()
		{
			short[] sampleShorts = { 12100, 23500 };
			IAudioFormat sound = new WavFormat();
			sound.Data = sampleShorts;
			sound.NumOfDataSamples = 2;
			sound.SampleRate = 48000;
			sound.BlockAlign = 4;
			sound.BitsPerSample = 16;
			sound.Channels = 2;


			AudioProcessor.StereoToMono(sound);


			short[] correctShorts = { 17800 };

			Assert.AreEqual(correctShorts.Length, sound.Data.Length);
			for (int i = 0; i < correctShorts.Length; i++)
			{
				Assert.AreEqual(correctShorts[i], sound.Data[i]);
			}
		}

		[TestMethod]
		public void StereoToMono_LongData()
		{
			//each row corresponds to 1 sample
			short[] sampleShorts = {
				119, 66 ,
				37 , 131,
				35 , 164,
				159, 141,
				59 , 118,
				74 , 189,
				97 , 52 ,
				122, 25

			};

			IAudioFormat sound = new WavFormat();
			sound.Data = sampleShorts;
			sound.NumOfDataSamples = 16;
			sound.SampleRate = 48000;
			sound.BlockAlign = 4;
			sound.BitsPerSample = 16;
			sound.Channels = 2;



			AudioProcessor.StereoToMono(sound);


			short[] correctShorts =
			{
				92 ,
				84 ,
				99 ,
				150,
				88 ,
				131,
				74,
				73

			};

			Assert.AreEqual(correctShorts.Length, sound.Data.Length);
			for (int i = 0; i < correctShorts.Length; i++)
			{
				Assert.AreEqual(correctShorts[i], sound.Data[i]);
			}
		}

		[TestMethod]
		public void BitReverse_SmallSample()
		{
			double[] data = new double[]
			{
				33.25, 1010.5,	//0th
				1.25, -1010.5,	//1st
				1.825, 100,		//2nd
				12123.25, -33	//3rd
			};

			AudioProcessor.BitReverse(data);

			//positions 1 and 2 (01 && 10) swapped
			double[] reversedCorrect = new double[]
			{
				33.25, 1010.5,	//0th
				1.825, 100,		//1st 
				1.25, -1010.5,	//2nd 
				12123.25, -33	//3rd
			};
			
			for (int i = 0; i < data.Length; i++)
			{
				Assert.AreEqual(reversedCorrect[i], data[i]);
			}

		}

		[TestMethod]
		public void FFT_SmallSample()
		{
			double[] input = new double[]
			{ 
			  //Re Im
				1d, 0d,
				0d, 0d,
				1d, 0d,
				0d, 0d
			};

			AudioProcessor.FFT(input);

			double[] res = new double[]
			{
				2d, 0d,
				0d, 0d,
				2d, 0d,
				0d, 0d

			};

			for (int i = 0; i < res.Length; i++)
			{
				Assert.AreEqual(res[i], input[i]);
			}
		}

	}
}
