using SFML.Graphics;

namespace Shazam.Visualiser.MusicModes
{
	interface IVisualiserMode
	{
		void Draw(RenderWindow window);
		void Update();
		void Quit();
	}
}
