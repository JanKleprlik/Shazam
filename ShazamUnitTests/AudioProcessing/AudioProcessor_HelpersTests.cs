using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shazam.AudioProcessing;
using Shazam.AudioProcessing.Server;
using Shazam.Extensions;

namespace ShazamUnitTests.AudioProcessing
{
	[TestClass]
	public class AudioProcessor_HelpersTests
	{
		[TestMethod]
		public void SwapExtension_Double()
		{
			double first = 100;
			double second = 123.123;

			first = first.Swap(ref second);

			Assert.AreEqual(123.123, first);
			Assert.AreEqual(100, second);
		}
	}
}
