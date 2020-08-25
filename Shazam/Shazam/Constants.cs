using System;
using System.Collections.Generic;
using System.Text;

namespace Shazam
{
	public partial class Shazam
	{
		static class Constants
		{
			public const int WindowSize = 4096;
			public const int DownSampleCoef = 4;
			public const int TargetZoneSize = 5;
			public const int AnchorOffset = 2;
			public const double SamplesInTgzCoef = 0.5;
			public const double CoherentNotesCoef = 0.4;

			public const string FingerprintPath = @"Resources/Fingerprints";
			public const string MetadataPath = @"Resources/Metadata.csv";

		}
	}

}
