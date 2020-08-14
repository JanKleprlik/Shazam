using System;
using System.Runtime.CompilerServices;

namespace Shazam.Extensions
{
	public static class ValueTypeExtensions
	{
		public static T Swap<T>(this T first, ref T second)
		{
			T tmp = second;
			second = first;
			return tmp;
		}

		public static int ToPowOf(this int _base, int exp) 
		{
			int res = _base;
			for (int i = 0; i < exp; i++)
			{
				res *= _base;
			}

			return res;
		}

		public static double Pow2(this double _base)
		{
			return _base * _base;
		}

	}
}