using System;
using System.Collections.Generic;
using System.Text;

namespace Shazam.AudioFormats
{
	public class WavFormat : IAudioFormat
	{
		public uint Channels { get; set; }
		public uint SampleRate { get; set; }
		public int ByteRate { get; set; }
		public short BlockAlign { get; set; }
		public short BitsPerSample { get; set; }
		public int NumOfDataSamples { get; set; }
		public short[] Data { get; set; }
		public double[] DataDouble { get; set; }

		public bool IsCorrectFormat(byte[] data)
		{
			//RIFF in ASCII
			if (data[0] == 0x52 && //R
			    data[1] == 0x49 && //I
			    data[2] == 0x46 && //F
			    data[3] == 0x46)   //F
				return true;
			return false;
		}

	}
}
