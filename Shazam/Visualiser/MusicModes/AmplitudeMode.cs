using SFML.Audio;
using SFML.Graphics;
using SFML.System;

namespace Shazam.Visualiser.MusicModes
{
	class AmplitudeMode : AbstractMode
	{
		#region Constructors
		public AmplitudeMode(string path) :base(new SoundBuffer(path))
		{
			VA = new VertexArray(PrimitiveType.LineStrip, BufferSize);
			Song.Loop = true;
			Song.Play();
		}
		public AmplitudeMode(byte[] data) : base(new SoundBuffer(data))
		{
			VA = new VertexArray(PrimitiveType.LineStrip, BufferSize);
			Song.Loop = true;
			Song.Play();
		}
		public AmplitudeMode(short[] data, uint channelCount, uint sampleRate) : base(new SoundBuffer(data, channelCount, sampleRate))
		{
			VA = new VertexArray(PrimitiveType.LineStrip, BufferSize);
			Song.Loop = true;
			Song.Play();
		}

		public AmplitudeMode(SoundBuffer sb) : base(sb)
		{
			VA = new VertexArray(PrimitiveType.LineStrip, BufferSize);
			Song.Loop = true;
			Song.Play();
		}

		#endregion


		private VertexArray VA;


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
					//Takes every second sample (only the Left ones)
					for (uint i = 0; i < BufferSize; i++)
					{
						VA[i] = new Vertex(new Vector2f(i/4 + 100, (float)(200 + Samples[(i + offset) * 2] * 0.008)));
					}
				}
				else 
				{
					for (uint i = 0; i < BufferSize; i++)
					{
						VA[i] = new Vertex(new Vector2f(i/4 + 100, (float)(200 + Samples[(i + offset)] * 0.008)));
					}
				}

			}
		}

	}
}
