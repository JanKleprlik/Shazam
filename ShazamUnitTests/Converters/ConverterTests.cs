using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Shazam.Tools;

namespace ShazamUnitTests
{
	[TestClass]
	public class ConverterTests
	{
		#region BytesToShort
		[TestMethod]
		public void BytesToShort_0()
		{
			byte[] data = { 0x00, 0x00 };
			
			Assert.AreEqual(0,Converter.BytesToShort(data));
		}

		[TestMethod]
		public void BytesToShort_13200()
		{
			byte[] data = { 0x90, 0x33 };

			Assert.AreEqual(13200, Converter.BytesToShort(data));
		}

		[TestMethod]
		public void BytesToShort_ShortMaxValue()
		{
			byte[] data = { 0xff, 0x7f };

			Assert.AreEqual(short.MaxValue, Converter.BytesToShort(data));
		}

		[TestMethod]
		public void BytesToShort_TooManyBytes()
		{
			byte[] data = { 0xff, 0x7f, 0xff };
			try
			{
				Assert.AreEqual(32767, Converter.BytesToShort(data));
				Assert.Fail();
			}
			catch (ArgumentException)
			{}
		}
		#endregion

		#region BytesToInt
		[TestMethod]
		public void BytesToInt_0()
		{
			byte[] data = { 0x00, 0x00, 0x00, 0x00 };

			Assert.AreEqual(0, Converter.BytesToInt(data));
		}

		[TestMethod]
		public void BytesToInt_13200123()
		{
			byte[] data = { 0xFB, 0x6A, 0xC9, 0x00 };

			Assert.AreEqual(13200123, Converter.BytesToInt(data));
		}

		[TestMethod]
		public void BytesToInt_IntMaxValue()
		{
			byte[] data = { 0xff, 0xff, 0xff, 0x7f };

			Assert.AreEqual(int.MaxValue, Converter.BytesToInt(data));
		}

		[TestMethod]
		public void BytesToInt_TooFewBytes()
		{
			byte[] data = { 0xff, 0x7f};
			try
			{
				Assert.AreEqual(32767, Converter.BytesToInt(data));
				Assert.Fail();
			}
			catch (ArgumentException)
			{ }
		}
		#endregion

	}
}
