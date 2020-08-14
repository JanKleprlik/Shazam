using SFML.Graphics;

namespace Shazam.Visualiser.MusicModes
{
	interface IVisualizerMode
	{
		void Draw(RenderWindow window);
		void Update();
		void Quit();
	}
}
