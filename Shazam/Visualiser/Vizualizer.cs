using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using Shazam.Extensions;
using Shazam.Visualiser.MusicModes;
using Shazam.Visualiser.Spectograms;

namespace Shazam.Visualiser
{
	public enum VisualisationModes
	{
		Amplitude,
		Frequencies,
		CutOffFrequencies,
		Spectogram
	}

	public class Visualizer
	{
		#region Constructors
		public Visualizer(short[] data, uint channelCount, uint sampleRate, VisualisationModes vm, int downSampleCoef= 1)
		{
			soundBuffer = new SoundBuffer(data, channelCount, sampleRate);
			visualisation = CreateMode(soundBuffer, vm, downSampleCoef);
		}
		#endregion

		private static IVisualizerMode CreateMode(SoundBuffer sb, VisualisationModes vm, int downSampleCoef)
		{
			switch (vm)
			{
				case VisualisationModes.Amplitude:
					return new AmplitudeMode(sb);
				case VisualisationModes.Frequencies:
					return new FrequenciesMode(sb, downSampleCoef);
				case VisualisationModes.CutOffFrequencies:
					return new CutOffFrequenciesMode(sb);
				case VisualisationModes.Spectogram:
					return new Spectogram(sb);
				default:
					throw new ArgumentException($"Mode {vm.ToString()} is not supported");
			}
		}

		internal readonly IVisualizerMode visualisation;
		private readonly SoundBuffer soundBuffer;

		public void Run()
		{
			var mode = new SFML.Window.VideoMode(1224, 400);
			var window = new SFML.Graphics.RenderWindow(mode, "Shazam");
			window.KeyPressed += Window_KeyPressed;
			window.SetFramerateLimit(30);

			// Start the game loop
			while (window.IsOpen)
			{
				visualisation.Update();
				window.Clear();

				// Process events
				//window.DispatchEvents();
				window.DispatchEvents();
				visualisation.Draw(window);
				// Finally, display the rendered frame on screen
				window.Display();
			}

			visualisation.Quit();
			soundBuffer.Dispose();
			window.Close();
		}

		private void Window_KeyPressed(object sender, SFML.Window.KeyEventArgs e)
		{
			var window = (SFML.Window.Window)sender;
			if (e.Code == SFML.Window.Keyboard.Key.Escape)
			{
				window.Close();
			}
		}



	}


}
