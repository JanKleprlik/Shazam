using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using Shazam.AudioFormats;
using Shazam.Extensions;
[assembly: InternalsVisibleTo("ShazamUnitTests")]
namespace Shazam.AudioProcessing
{
	
	public partial class AudioProcessor
	{
		private static short Average(short left, short right)
		{
			return (short) ((left + right) / 2);
		}

		private static void IsSupportedFormat(IAudioFormat sound)
		{

			//wav
			if (sound is WavFormat)
			{
				//currently only supported wav format for creating fingreprints into the database
				if (sound.BitsPerSample != 16)
					throw new ArgumentException($"Given audio is not in supported format. \nExpected BitsPerSample: 16 \n Actual BitsPerSample: {sound.BitsPerSample}");
				if (sound.SampleRate != 48000)
					throw new ArgumentException($"Given audio is not in supported format. \nExpected SampleRate: 48000 \n Actual SampleRate: {sound.SampleRate}");
				if (sound.BlockAlign != 4)
					throw new ArgumentException($"Given audio is not in supported format. \nExpected BlockAlign: 4 \n Actual BlockAlign: {sound.BlockAlign}");

			}

		}


		/// <summary>
		/// BitReverse for array of doubles valued as complex number alternating real and imaginary part.
		/// Swaps data for every two indexes that are bit-reverses to each other
		/// Taken from Lomont implementation.
		/// Implementation from Knuth's The Art Of Computer Programming.
		/// </summary>
		/// <param name="data"></param>
		internal static void BitReverse(double[] data)
		{
			int n = data.Length / 2;
			int first = 0, second = 0;
			
			int top = n / 2;

			while (true)
			{
				//swapping real parts
				data[first + 2] = data[first + 2].Swap(ref data[second + n]);
				//swapping imaginary parts
				data[first + 3] = data[first + 3].Swap(ref data[second + n + 1]);

				if (first > second) //first and second met -> swap two more
				{
					//first
					//swapping real parts
					data[first] = data[first].Swap(ref data[second]);
					//swapping imaginary parts
					data[first+1] = data[first + 1].Swap(ref data[second + 1]);

					//second
					//swapping real parts
					data[first + n + 2] = data[first + n + 2].Swap(ref data[second + n + 2]);
					//swapping imaginary parts
					data[first + n + 3] = data[first + n + 3].Swap(ref data[second + n + 3]);
				}

				//moving counters to next bit-reversed indexes
				second += 4;
				if (second >= n)
					break;
				int finder = top;
				while (first >= finder)
				{
					first -= finder;
					finder /= 2;
				}
				first += finder;
			}
		}


		private static void Normalize(double[] data)
		{
			int n = data.Length / 2; //div 2 because of Re+Img
			for (int i = 0; i < data.Length; i++)
			{
				data[i] *= Math.Pow(n, -1 / 2);
			}
		}
	}
}
