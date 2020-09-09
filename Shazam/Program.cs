using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
			TextWriter output = Console.Out;
			bool isDebug = false;

			//CreateFingerprints(s);


			Thread command = null;

			output.WriteLine("{1,2} {0}", "Enter 'h' or 'help' for help.","");
			while (true)
			{
				string argument = Console.ReadLine();
				switch (argument.ToLower())
				{
					case "a":
					case "add":
						output.WriteLine("{1,2} {0}", "Enter name of the song", "");
						string name = Console.ReadLine();
						output.WriteLine("{1,2} {0}", "Enter name of the author","");
						string author = Console.ReadLine();
						output.WriteLine("{1,2} {0}", "Enter name of the audio file","");
						string path = Console.ReadLine();
						if (command != null && command.IsAlive)
							command.Join();
						command = new Thread(() =>
						{
							AddSong(path, name, author, output, s);
						});
						command.Start();

						break;
					case "c":
					case "clear":
						if(output == Console.Out)
							Console.Clear();
						break;
					case "d":
					case "debug":
						if (!isDebug)
						{
							isDebug = true;
							RedirectDebugOutput(output, isDebug);
							output.WriteLine("{1,2} {0}", "Debug mode activated", "");
						}
						else
						{
							isDebug = false;
							RedirectDebugOutput(output, isDebug);
							output.WriteLine("{1,2} {0}", "Debug mode cancelled", "");
						}
						break;

					case "h":
					case "help":
						WriteHelp(output);
						break;

					case "l":
					case "list":
						s.ListSongs(output);
						break;

					case "q":
					case "quit":
						return;

					case "r":
					case "record":
						if (command != null && command.IsAlive)
							command.Join();
						command = new Thread(() =>
						{
							output.WriteLine("{0,2} {1}", "", s.RecognizeSong());
						});
						command.Start();
						break;

					default:
						continue;
						
				}
				
			}
		}

        private static void CreateFingerprints(Shazam shazam)
        {
	        const string folderPath = "Resources/Dataset";
			foreach (string file in Directory.EnumerateFiles(folderPath, "*.wav"))
			{
				Regex rx = new Regex(@"\\(?<songID>\d+).wav"); //regex for matching songID

				if (uint.TryParse(rx.Match(file).Groups["songID"].Value, out uint songID))
				{
					try
					{
						shazam.AddNewSong("../../"+file, songID.ToString(), songID.ToString());
					}
					catch (Exception e)
					{
						Console.WriteLine($"Song ID: {songID}; file: {file}; {e.Message}");
					}
				}
			}
		}


        private static void RedirectDebugOutput(TextWriter output, bool add)
        {
	        if (add)
	        {
		        var tl = new TextWriterTraceListener(output);
		        tl.Name = "customOutput";
		        Trace.Listeners.Add(tl);
	        }
	        else
	        {
				Trace.Listeners.Remove("customOutput");
	        }
		}

        private static void AddSong(string path, string name, string author, TextWriter o, Shazam s)
        {
			try
			{
				s.AddNewSong(path,name,author);
				o.WriteLine("{0,2} {1}", "", $"Song '{name}' added.");
			}
			catch (Exception e)
			{
				if (e is FileNotFoundException)
				{
					o.WriteLine(e.Message);
					o.WriteLine("File not found.");
					o.WriteLine("{0,2} {1}", "", $"Adding '{name}' was unsuccessful.");
				}
				else if (e is ArgumentException)
				{
					o.WriteLine(e.Message);
					o.WriteLine("{0,2} {1}", "", $"Adding '{name}' was unsuccessful.");
				}
				else throw e;

			}
		}

        private static void WriteHelp(TextWriter o)
        {
	        Tuple<string, string>[] commandList = 
	        {
				new Tuple<string, string>("a | add", "add new song to the database"),
				new Tuple<string, string>("d | debug", "write debug information"),
				new Tuple<string, string>("c | clear", "clear the console"),
				new Tuple<string, string>("h | help", "list all commands"),
				new Tuple<string, string>("l | list", "list all songs in the database"),
				new Tuple<string, string>("q | quit", "quit the application"),
				new Tuple<string, string>("r | record", "record and audio and recognize the song")

			};
	        o.WriteLine("{2,2} {0,-10} {1,-20}\n", "COMMAND", "DESCRIPTION","");

			foreach (var pair in commandList)
	        {
		        o.WriteLine("{2,2} {0,-10} {1,-20}", pair.Item1, pair.Item2, "");
	        }
        }
	}	
}
