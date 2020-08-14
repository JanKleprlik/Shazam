using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using Shazam.AudioProcessing;
using tools = Shazam.Tools.Tools;

namespace Shazam.Visualiser.MusicModes
{
	class FrequenciesMode : AbstractMode
	{
		public FrequenciesMode(SoundBuffer sb) : base(sb)
		{
			VA = new VertexArray(PrimitiveType.LineStrip, BufferSize/2);
			window = AudioProcessor.GenerateHammingWindow(BufferSize);
			bin = new double[BufferSize*2];
			Song.Loop = true;
			Song.Play();
		}

		private VertexArray VA { get; set; }
		private double[] bin { get; set; }
		private double[] window { get; set; }
		public override void Draw(RenderWindow window)
		{
			window.Draw(VA);
		}


		public override void Update()
		{
			int offset = (int) (Song.PlayingOffset.AsSeconds() * SampleRate);
			if (offset + BufferSize < SampleCount)
			{
				if (ChannelCount == 2)
				{
					for (uint i = 0; i < BufferSize; i++)
					{
						bin[i * 2] = Samples[(i + offset) * 2] * window[i];
						bin[i * 2 + 1] = 0;
					}
				}
				else
				{
					for (uint i = 0; i < BufferSize; i++)
					{
						bin[i * 2] = Samples[(i + offset)] * window[i];
						bin[i * 2 + 1] = 0;

					}
				}
			}


			AudioProcessor.FFT(bin);

			for (uint i = 0; i < BufferSize / 2; i++)
			{
				VA[i] = new Vertex(new Vector2f(i / 2 + 100,
					(float) (200 - tools.GetComplexAbs(bin[2 * i], bin[2 * i + 1]) / 100000)));
			}

		}
	}
}
