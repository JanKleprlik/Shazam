using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ShazamUnitTests.Shazam
{
	[TestClass]
	public class ShazamTests
	{
		[TestMethod]
		public void BuildAddress_Ones()
		{
			global::Shazam.Shazam sh = new global::Shazam.Shazam();
			uint anchorFreq = 1;
			uint pointFreq = 1;
			uint delta = 1;

			uint res = sh.BuildAddress(anchorFreq,pointFreq, delta);

			uint correctRes = 0b0000_0000_1___000_0000_01___00_0000_0000_0001;
			Assert.AreEqual(correctRes, res);

		}		[TestMethod]
		public void BuildAddress_MaxValues()
		{
			global::Shazam.Shazam sh = new global::Shazam.Shazam();
			uint anchorFreq = 511;
			uint pointFreq = 511;
			uint delta = 12345;

			uint res = sh.BuildAddress(anchorFreq,pointFreq, delta);

			uint correctRes = 0b1111_1111_1___111_1111_11___11_0000_0011_1001;
			Assert.AreEqual(correctRes, res);

		}
	}
}
