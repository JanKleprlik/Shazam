using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Shazam.AudioFormats;

namespace Shazam
{
	public partial class Shazam
	{
		/// <summary>
		/// Loads all fingerprints stored at <c>folderPath</c>
		/// </summary>
		/// <param name="folderPath">Folder with fingerprints</param>
		private void LoadFingerprints(string folderPath)
		{
			database = new Dictionary<uint, List<ulong>>();
			foreach (string file in Directory.EnumerateFiles(folderPath, "*.txt"))
			{
				Regex rx = new Regex(@"\\(?<songID>\d+).txt"); //regex for matching songID

				if (uint.TryParse(rx.Match(file).Groups["songID"].Value, out uint songID))
				{
					LoadSongFingerprint(file, songID);
					Trace.WriteLine($"   Song ID: {songID} was loaded.");
					maxSongID = Math.Max(maxSongID, songID);
				}
			}
		}
		/// <summary>
		/// Loads fingerprint of song at <c>fingerprintPath</c> as song with <c>songID</c> ID.
		/// </summary>
		/// <param name="fingerprintPath">Location of the fingerprint.</param>
		/// <param name="songID">Song ID</param>
		private void LoadSongFingerprint(string fingerprintPath, uint songID)
		{
			List<Tuple<uint, uint>> timeFrequencyPoints = LoadTFP(fingerprintPath);

			AddTFPToDatabase(timeFrequencyPoints, songID);
		}
		/// <summary>
		/// Loads Time-Frequency Points of a fingerprint at <c>fingerprintPath</c>
		/// </summary>
		/// <param name="fingerprintPath"></param>
		/// <returns>List of tuples of time, frequency</returns>
		private List<Tuple<uint, uint>> LoadTFP(string fingerprintPath = Constants.FingerprintPath)
		{
			//TFPs = Time-Frequency Points
			List<Tuple<uint, uint>> TFPs = new List<Tuple<uint, uint>>();
			using (StreamReader sr = new StreamReader(fingerprintPath))
			{
				while (true)
				{
					string line = sr.ReadLine();
					if (line == null)
						break;
					var tokens = line.Split(';');

					if (!uint.TryParse(tokens[0], out uint time))
						continue; //skip this TFP
					if (!uint.TryParse(tokens[1], out uint frequency))
						continue; //skip this TFP

					TFPs.Add(new Tuple<uint, uint>(time, frequency));
				}
			}

			return TFPs;
		}
		/// <summary>
		/// Loads metadata to of songs from <c>metadataPath</c>
		/// </summary>
		/// <param name="metadataPath">	<para>XSV file with metadata</para>
		///								<para>format: ID;NAME;ARTIST</para></param>
		private void LoadMetadata(string metadataPath = Constants.MetadataPath)
		{
			metadata = new Dictionary<uint, Song>();
			using (StreamReader sr = new StreamReader(metadataPath))
			{
				sr.ReadLine(); //0th line
				while (true)
				{
					string line = sr.ReadLine();
					if (line == null)
						break;
					if (line.StartsWith('#'))
						continue; //comments start with '#'

					var tokens = line.Split(';');

					if (!uint.TryParse(tokens[0], out uint songID))
						continue; //skip this song
					if (metadata.ContainsKey(songID))
						continue; //songID is already in database

					metadata.Add(songID, new Song(songID, tokens[1], tokens[2]));
				}
			}
		}
		/// <summary>
		/// Populates local database with TFPs
		/// </summary>
		/// <param name="timeFrequencyPoints">Time-frequency points of the song</param>
		/// <param name="songId">songID</param>
		private void AddTFPToDatabase(List<Tuple<uint, uint>> timeFrequencyPoints, in uint songId)
		{
			/* spectogram:
			 *
			 * |
			 * |       X X
			 * |         X
			 * |     X     X
			 * |   X         X
			 * | X X X     X
			 * x----------------
			 */


			// -targetZoneSize: because of end limit 
			// -1: because of anchor point at -2 position target zone
			int stopIdx = timeFrequencyPoints.Count - Constants.TargetZoneSize - Constants.AnchorOffset;
			for (int i = 0; i < stopIdx; i++)
			{
				//anchor is at idx i
				//1st in TZ is at idx i+3
				//5th in TZ is at idx i+7

				uint anchorFreq = timeFrequencyPoints[i].Item2;
				uint anchorTime = timeFrequencyPoints[i].Item1;
				ulong SongValue = BuildSongValue(anchorTime, songId);
				for (int pointNum = 3; pointNum < Constants.TargetZoneSize + 3; pointNum++)
				{
					uint pointFreq = timeFrequencyPoints[i + pointNum].Item2;
					uint pointTime = timeFrequencyPoints[i + pointNum].Item1;

					uint address = BuildAddress(anchorFreq, pointFreq, pointTime - anchorTime);

					if (!database.ContainsKey(address)) //create new instance if it doesnt exist
					{
						database.Add(address, new List<ulong>() { SongValue });
					}
					else //add SongValue to the list of
					{
						database[address].Add(SongValue);
					}
				}

			}
		}

	}
}
