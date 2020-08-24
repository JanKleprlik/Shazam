﻿using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
			Shazam s = new Shazam();
			//s.AddNewSong("Songs/Avicii.wav", 1);
			//s.AddNewSong("Songs/Coldplay.wav", 2);
			//s.AddNewSong("Songs/400Hz.wav", 3);
			//s.AddNewSong("Songs/Hertz.wav", 4);
			//s.AddNewSong("Songs/Havana.wav", 5);
			//s.AddNewSong("Songs/SameOldLove.wav", 6);	

			for (uint i = 1; i <= 6; i++)
			{
				s.LoadSong(i);
			}

			Console.WriteLine($"Songs ID is : {s.RecognizeSong()}");

		}
	}	
}
