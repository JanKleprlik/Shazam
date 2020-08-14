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
	}
}
