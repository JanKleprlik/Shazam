using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shazam;
namespace ShazamUnitTests.Shazam
{
	[TestClass]
	public class ShazamTests
	{
		[TestMethod]
		public void BuildAddress_Ones()
		{
			uint anchorFreq = 1;
			uint pointFreq = 1;
			uint delta = 1;
			uint res = global::Shazam.Shazam.BuildAddress(anchorFreq,pointFreq, delta);

			uint correctRes = 0b000000001___000000001___00000000000001;
			Assert.AreEqual(correctRes, res);

		}		[TestMethod]
		public void BuildAddress_MaxValues()
		{
			uint anchorFreq = 511;
			uint pointFreq = 511;
			uint delta = 12345;

			uint res = global::Shazam.Shazam.BuildAddress(anchorFreq,pointFreq, delta);

			uint correctRes = 0b111111111___111111111___11000000111001;
			Assert.AreEqual(correctRes, res);

		}
	}
}
