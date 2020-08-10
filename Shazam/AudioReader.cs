using NAudio.Wave;
using Shazam.Audio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Shazam.Converters;

namespace Shazam
{
	public class AudioReader
	{
		public IAudioFormat GetSound(string path)
		{
			byte[] data = File.ReadAllBytes(path);
			IAudioFormat Sound;

			if (path.EndsWith(".wav"))
			{
				Sound = new WavFormat();
				//Check if the beginning starts with RIFF (currently only supported format of wav files)
				if (!Sound.IsCorrectFormat(new []{ data[0], data[1], data[2], data[3] }))
				{
					throw new ArgumentException($"File {path} formatted wrongly, not a 'wav' format.");
				}

				Sound.Channels = Converter.BytesToShort(new byte[] {data[22], data[23] });
				Sound.SampleRate = Converter.BytesToInt(new byte[] {data[24], data[25], data[26] , data[27] });
				Sound.ByteRate = Converter.BytesToInt(new byte[] {data[28], data[29], data[30] , data[31] });
				Sound.BlockAlign = Converter.BytesToShort(new byte[] {data[32], data[33] });
				Sound.BitsPerSample = Converter.BytesToShort(new byte[] {data[34], data[35] });
				int dataOffset = findDataOffset(data);
				Sound.NumOfDataBytes = Converter.BytesToInt(new byte[]
					{data[dataOffset - 4], data[dataOffset - 3], data[dataOffset - 2], data[dataOffset - 1]});
				Sound.Data = data.Skip(dataOffset).ToArray();
			}
			else
			{
				throw new NotImplementedException($"Format of file {path} is not implemented.");
			}

			return Sound;
		}


		private int findDataOffset(byte[] data)
		{
			for (int i = 0; i < data.Length - 3; i++)
			{
				if (data[i] == 0x64 && //d
				    data[i + 1] == 0x61 && //a
				    data[i + 2] == 0x74 && //t
				    data[i + 3] == 0x61)//a
				{
					return i + 8; //+4 is for 'data' bytes, +4 is for integer representing number of data bytes
				}
			}

			throw new ArgumentException("Part with data not found.");
		}
	}
}
