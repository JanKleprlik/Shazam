using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Shazam.Extensions;

namespace Shazam.AudioProcessing
{
	static class FastFourierTransformation
	{
		/// <summary>
		/// Recursive DFT
		/// Currently not working
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static Complex[] DFT_Recurr(Complex[] data)
		{
			int n = data.Length;

			if (n == 1)
				return new Complex[] { 1, data[0] };

			Complex[] even = new Complex[n / 2];
			Complex[] odd = new Complex[n / 2];

			for (int i = 0; i < n / 2; i++)
			{
				even[i] = data[i * 2];
				odd[i] = data[i * 2 + 1];
			}

			even = DFT_Recurr(even);
			odd = DFT_Recurr(odd);

			Complex[] bin = new Complex[n];

			Complex[] w = new Complex[n];
			for (int i = 0; i < n; i++)
			{
				double alpha = 2.0 * Math.PI * (double)i / (double)n;
				w[i] = new Complex(Math.Cos(alpha), Math.Sin(alpha));
			}

			for (int i = 0; i < n / 2; i++)
			{
				bin[i] = even[i] + w[i] * odd[i];
				bin[i + n / 2] = even[i] - w[i] * odd[i];
			}

			return bin;
		}

		/// <summary>
		/// forloop DFT
		/// Currently too slow
		/// </summary>
		/// <param name="data"></param>
		public static void DFT(Complex[] data)
		{
			int n = data.Length;
			double alpha;
			Complex[] bin = new Complex[n];

			for (int i = 0; i < n / 2; i++) //for audio, only one half is needed
			{
				alpha = 2.0 * Math.PI * i / (double)n;

				for (int j = 0; j < n; j++)
				{
					bin[i] += new Complex(data[j].Real * Math.Cos(j * alpha) - data[j].Imaginary * Math.Sin(j * alpha),
										data[j].Real * Math.Cos(j * alpha) + data[j].Imaginary * Math.Sin(j * alpha));
				}
			}

			for (int i = 0; i < n / 2; i++)
			{
				data[i] = new Complex(bin[i].Real / n, bin[i].Imaginary / n); //divide by n to scale 
			}
		}

		/// <summary>
		/// FFT specialized for audio
		/// Inspired by LomontFFT <see cref="https://www.lomont.org/software/misc/fft/LomontFFT.cs"/> and classic wikipedia implementation <see cref="https://en.wikipedia.org/wiki/Cooley%E2%80%93Tukey_FFT_algorithm"/>.
		/// Data are stored as real and imaginary doubles alternating.
		/// Data length must be a power of two.
		/// </summary>
		/// <param name="data">Complex valued data stored as doubles.
		/// Alternating between real and imaginary parts.</param>
		/// <exception cref="ArgumentException">data length is not power of two</exception>
		public static void FFT(double[] data, bool normalize = false)
		{
			int n = data.Length;
			if (!Tools.Tools.IsPowOfTwo(n))
				throw new ArgumentException($"Data length: {n} is not power of two.");

			n /= 2; //data are represented as 1 double for Real part && 1 double for Imaginary part

			BitReverse(data);

			int max = 1;
			while (n > max) // while loop represents logarithm for loop implementation of https://en.wikipedia.org/wiki/Cooley%E2%80%93Tukey_FFT_algorithm
			{
				int step = 2 * max; // 2^s form wiki
									//helper variables for Real and Img separate computations
				double omegaReal = 1;
				double omegaImg = 0;
				double omegaCoefReal = Math.Cos(Math.PI / max);
				double omegaCoefImg = Math.Sin(Math.PI / max);
				for (int m = 0; m < step; m += 2) //2 because of Real + Img double
				{
					//2*n because we have double the amount of data (Re+Img)
					for (int k = m; k < 2 * n; k += 2 * step)
					{
						double tmpReal = omegaReal * data[k + step] - omegaImg * data[k + step + 1]; //t real part from wiki
						double tmpImg = omegaImg * data[k + step] + omegaReal * data[k + step + 1]; //t img part from wiki
																									//A[k+j+m/2] from wiki
						data[k + step] = data[k] - tmpReal;
						data[k + step + 1] = data[k + 1] - tmpImg;
						//A[k+j] from wiki
						data[k] = data[k] + tmpReal;
						data[k + 1] = data[k + 1] + tmpImg;
					}
					//compute new omega
					double tmp = omegaReal;
					omegaReal = omegaReal * omegaCoefReal - omegaImg * omegaCoefImg;
					omegaImg = omegaImg * omegaCoefReal + tmp * omegaCoefImg;
				}

				max = step; //move logarithm loop
			}

			if (normalize)
				Normalize(data);
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
					data[first + 1] = data[first + 1].Swap(ref data[second + 1]);

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

		/// <summary>
		/// Normalize data after fft - classical
		/// </summary>
		/// <param name="data"></param>
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
