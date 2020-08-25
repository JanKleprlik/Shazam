using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Shazam
{
	public partial class Shazam
	{
		/// <summary>
		/// Saves TFPs as as .txt file
		/// </summary>
		/// <param name="timeFrequencyPoitns">TFPs</param>
		/// <param name="songID">ID to associate TFPs with</param>
		private void SaveTFPs(List<Tuple<uint, uint>> timeFrequencyPoitns, in uint songID)
		{
			using (StreamWriter sw = new StreamWriter($"Resources/Fingerprints/{songID}.txt"))
			{
				foreach (var TFpoint in timeFrequencyPoitns)
				{
					sw.WriteLine(TFpoint.Item1 + ";" + TFpoint.Item2);
				}
			}
		}
		/// <summary>
		/// Saves song metadata to CSV file.
		/// </summary>
		/// <param name="songID">songID</param>
		/// <param name="name">name of the song</param>
		/// <param name="author">author of the song</param>
		/// <param name="metadataPath"></param>
		private void SaveMetadata(uint songID, string name, string author, string metadataPath = Constants.MetadataPath)
		{
			using (StreamWriter sr = new StreamWriter(metadataPath, true))
			{
				//ID;Name;Author
				sr.WriteLine($"{songID};{name};{author}");
			}
		}

	}
}
