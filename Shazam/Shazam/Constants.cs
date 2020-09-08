using System;
using System.Collections.Generic;
using System.Text;

namespace Shazam
{
	public partial class Shazam
	{
		static class Constants
		{
			/// <summary>
			/// Default size of FFT window
			/// </summary>
			public const int WindowSize = 4096;
			/// <summary>
			/// Default downsample coeficient
			/// </summary>
			public const int DownSampleCoef = 4;
			/// <summary>
			/// Default size of target zone
			/// </summary>
			public const int TargetZoneSize = 5;
			/// <summary>
			/// Default offset of anchor from first actual point
			/// </summary>
			public const int AnchorOffset = 2;
			/// <summary>
			/// Obligated portion of samples in TGZ
			/// </summary>
			public const double SamplesInTgzCoef = 0.5;
			/// <summary>
			/// Obligated portion of time coherent notes
			/// </summary>
			public const double CoherentNotesCoef = 0.4;
			/// <summary>
			/// Default path to fingerprints
			/// </summary>
			public const string FingerprintPath = @"Resources/Fingerprints";
			/// <summary>
			/// Default path to metadata
			/// </summary>
			public const string MetadataPath = @"Resources/Metadata.csv";

		}
	}

}
