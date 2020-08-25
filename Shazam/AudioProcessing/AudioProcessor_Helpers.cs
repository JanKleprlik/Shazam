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

	}
}
