using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using System.Text;

namespace Shazam.Tools
{
	/// <summary>
	/// Converter from Little-endian byte arrays to other data types using byte shifts
	/// </summary>
	public static class Converter
	{
		public static short BytesToShort(byte[] bytes)
		{
            Contract.Requires(bytes != null);
            Contract.Requires(bytes.Length == 2);

            if (bytes.Length != 2)
				throw new ArgumentException($"Exactly 2 bytes requiered, got {bytes.Length} bytes.");
			short res = bytes[1];
			res = (short)(res << 8);
			res += bytes[0];
			return res;
		}

		public static int BytesToInt(byte[] bytes)
		{
			Contract.Requires(bytes != null);
			Contract.Requires(bytes.Length == 4);
            
			if (bytes.Length != 4)
				throw new ArgumentException($"Exactly 4 bytes requiered, got {bytes.Length} bytes.");
			int res = bytes[3];
			for(int i = 2; i >= 0; i--)
			{
				res = res << 8;	
				res += bytes[i];
			}

			return res;
		}

		public static uint BytesToUInt(byte[] bytes)
		{
			uint res = bytes[bytes.Length-1];
			for (int i = bytes.Length-2; i >= 0; i--)
			{
				res = res << 8;
				res += bytes[i];
			}

			return res;
		}
	}
}
