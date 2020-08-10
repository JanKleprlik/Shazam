using System;
using System.Collections.Generic;
using System.Text;

namespace Shazam.Audio
{
	public interface IAudioFormat
	{
		public short Channels { get; set; }
		public int SampleRate { get; set; }
		public int ByteRate { get; set; }
		public short BlockAlign { get; set; }
		public short BitsPerSample { get; set; }
		public int NumOfDataBytes { get; set; }
		public byte[] Data { get; set; }

		public bool IsCorrectFormat(byte[] data);
	}
}
