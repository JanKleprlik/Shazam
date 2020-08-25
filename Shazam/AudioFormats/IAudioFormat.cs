using System;
using System.Collections.Generic;
using System.Text;

namespace Shazam.AudioFormats
{
	public interface IAudioFormat
	{
		public uint Channels { get; set; }
		public uint SampleRate { get; set; }
		public int ByteRate { get; set; }
		public short BlockAlign { get; set; }
		public short BitsPerSample { get; set; }
		public int NumOfDataSamples { get; set; }
		//Song PlayData
		public short[] Data { get; set; }
		public bool IsCorrectFormat(byte[] data);
	}
}
