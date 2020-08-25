using System;
using System.Collections.Generic;
using System.Text;

namespace Shazam.AudioFormats
{
	public class Song
	{
		/// <summary>
		/// Instance of a song
		/// </summary>
		/// <param name="id">song ID</param>
		/// <param name="name">Name of the song</param>
		/// <param name="author">Creator of the song</param>
		public Song(uint id, string name, string author)
		{
			ID = id;
			Name = name;
			Author = author;
		}

		public uint ID { get; }
		public string Name { get; }
		public string Author { get; }

	}
}
