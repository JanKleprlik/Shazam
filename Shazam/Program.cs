using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using SFML.Audio;
using SFML.System;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;
using Shazam.Visualiser;
using Shazam.Visualiser.Spectograms;

namespace Shazam
{
	class Program
	{
        public static void Main()
        {

			//TODO: tweak constants and coefitients
			Shazam s = new Shazam();
			
			s.AddNewSong("Songs/Avicii.wav");

        }
	}	
}
