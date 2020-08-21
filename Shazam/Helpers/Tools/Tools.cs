using System;
using System.Collections.Generic;
using System.Text;
using Shazam.Extensions;

namespace Shazam.Tools
{
	public static class Tools
	{
		public static double GetComplexAbs(double real, double img)
		{
			return Math.Sqrt(real.Pow2() + img.Pow2());
		}

		public static bool IsPowOfTwo(int n)
		{
			if ((n & (n - 1)) != 0)
				return false;
			return true;
		}
		/// <summary>
		/// Takes complex number (as two doubles: real and imaginary part) from FFT and converts it into an amplitude value in dB.
		/// </summary>
		/// <param name="real">Real part of complex number.</param>
		/// <param name="imaginary">Imaginary part fo complex number.</param>
		/// <param name="windowSize">Size of a window applied at FFT. Default is 2048.</param>
		/// <returns></returns>
		public static double NormalizeAmplitude(double real, double imaginary, int windowSize = 2048)
		{
			var normalized = 2 * Math.Sqrt((real  * real + imaginary * imaginary) / windowSize);
			return 20 * Math.Log10(normalized);
		}
	}
}
