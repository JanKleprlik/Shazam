using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Xml;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using Shazam.Visualiser.MusicModes;

namespace Shazam.Visualiser.Spectograms
{
	abstract class AbstractSpectogram : AbstractMode
	{
		protected AbstractSpectogram(SoundBuffer sb) : base(sb) 
		{
			VA = new VertexArray(PrimitiveType.Points, 512);
		}

		protected VertexArray VA { get; set; }

		public abstract void Render(double[] data, Vector2f basePosition, int binsToRender);

		/// <summary>
		/// <para>Mapping of intesity of data.</para>
		/// <para>Low: Black</para>
		/// <para>High: White</para>
		/// </summary>
		/// <param name="n">Size of the fft window.</param>
		/// <returns></returns>
		protected static Color IntensityToColor(double real, double imaginary, int n)
		{
			//Black : 0,0,0
			//White: 255,255,255
			var normalized = 2 * Math.Sqrt((real * real + imaginary * imaginary) / n);
			var decibel = 20 * Math.Log10(normalized);
			byte colorIntensity;
			if (decibel < 0)
			{
				colorIntensity = 0;
			}
			else if (decibel > 255)
			{
				colorIntensity = 255;
				//Console.WriteLine($"ABOVE: {decibel} : 255");
			}
			else
			{
				colorIntensity = (byte)(int)decibel;
				//Console.WriteLine($"{decibel} : {colorIntensity}");
			}
			
			return new Color(colorIntensity,colorIntensity,colorIntensity);
		}
	}
}
