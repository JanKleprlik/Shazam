using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SFML.Audio;
using Shazam.AudioFormats;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;

[assembly: InternalsVisibleTo("ShazamUnitTests")]
namespace Shazam
{
	public partial class Shazam
	{
		public Shazam()
		{
			LoadFingerprints(Constants.FingerprintPath);
			
			LoadMetadata(Constants.MetadataPath);
		}


		/// <summary>
		/// Bits are stored as BE
		/// <para>Key:</para>
		/// <para> 9 bits =  frequency of anchor</para>
		/// <para> 9 bits =  frequency of point</para>
		/// <para> 14 bits =  delta</para>
		/// <para>Value (List of):</para>
		/// <para>32 bits absolute time of anchor</para>
		/// <para>32 bits id of a song</para>
		/// </summary>
		private Dictionary<uint, List<ulong>> database;
		/// <summary>
		/// Currently used highest songID
		/// </summary>
		private uint maxSongID;
		/// <summary>
		/// <para>Key: songID</para>
		/// <para>Value: Song with metadata</para>
		/// </summary>
		private Dictionary<uint, Song> metadata;


		/// <summary>
		/// <para>Add a new song to the database.</para>
		/// <para>WARNING: Song must be sampled at 48000Hz!</para>
		/// </summary>
		/// <param name="path">Location of .wav audio file</param>
		public void AddNewSong(string path, string name, string author)
		{
			//Plan of audio processing
			//STEREO -> MONO -> LOW PASS -> DOWNSAMPLE -> HAMMING -> FFT

			#region STEREO

			var audio = AudioReader.GetSound("Resources/Songs/"+path);

			#endregion

			#region MONO

			if (audio.Channels == 2)  //MONO
				AudioProcessor.StereoToMono(audio);

			#endregion

			#region Short to Double

			double[] data = ShortArrayToDoubleArray(audio.Data);
			#endregion

			#region LOW PASS & DOWNSAMPLE

			var downsampledData = AudioProcessor.DownSample(data, Constants.DownSampleCoef, audio.SampleRate); //LOWPASS + DOWNSAMPLE
			data = null; //release memory
			#endregion

			#region HAMMING & FFT
			//apply FFT at every 1024 samples
			//get 512 bins 
			//of frequencies 0 - 6 kHZ
			//bin size of ~ 11,7 Hz

			int bufferSize = Constants.WindowSize / Constants.DownSampleCoef; //default: 4096/4 = 1024
			var TimeFrequencyPoitns = CreateTimeFrequencyPoints(bufferSize, downsampledData, sensitivity:1);

			// DEBUG: printing time-frequency points
			/*/
			for (int i = 0; i < TimeFrequencyPoitns.Count; i++)
			{
				Debug.WriteLine($"index: {i}  --- time: {TimeFrequencyPoitns[i].Item1} --- frequency: {TimeFrequencyPoitns[i].Item2}");
			}
			/**/
			#endregion

			//Add TFPs to database
			AddTFPToDatabase(TimeFrequencyPoitns, ++maxSongID);
			//Add songs metadata to database
			metadata.Add(maxSongID, new Song(maxSongID, name, author));
			
			//Create file with TFPs
			SaveTFPs(TimeFrequencyPoitns, maxSongID);
			//Save metadata
			SaveMetadata(maxSongID, name, author);
		}

		/// <summary>
		/// Records 10 sec of audio through microphone and finds best match in song database
		/// </summary>
		/// <returns></returns>
		public string RecognizeSong()
		{
			//recording of the song
			double[] data = RecordAudio(10000);
			
			//measure time of song searching
			Stopwatch stopwatch = Stopwatch.StartNew();
			stopwatch.Start();

			List<Tuple<uint, uint>> timeFrequencyPoints;
			#region Creating Time-frequency points
			int bufferSize = Constants.WindowSize / Constants.DownSampleCoef;
			timeFrequencyPoints= CreateTimeFrequencyPoints(bufferSize, data, sensitivity:0.9); //set higher sensitivity because microphone has lower sensitivity

			//DEBUG: printing time-frequency points
			/*/
			foreach (var TFpoint in timeFrequencyPoints)
			{
				Console.WriteLine($"Time: {TFpoint.Item1} Freq: {TFpoint.Item2}");
			}
			/**/
			#endregion


			//find the best song in database
			uint? finalSongID =  Recogniser.FindBestMatch(database, timeFrequencyPoints);

			stopwatch.Stop();
					
			Trace.WriteLine($"   Song recognized in: {stopwatch.ElapsedMilliseconds} milliseconds");

			if (finalSongID == null)
				return "Recording was unrecognizable.";
			Song resultSong = metadata[(uint) finalSongID];

			return $"{resultSong.Name} by {resultSong.Author}";
		}

		/// <summary>
		/// Lists all songs in the database
		/// </summary>
		/// <param name="output">TextWriter to write the songs into</param>
		public void ListSongs(TextWriter output)
		{
			output.WriteLine("{3,2} {0,-30} {1,-30} {2} \n", "NAME", "AUTHOR", "ID", "");

			foreach (var song in metadata.Values)
			{
				output.WriteLine("{3,2} {0,-30} {1,-30} {2}", song.Name, song.Author,song.ID, "");
			}
		}
	}
}
