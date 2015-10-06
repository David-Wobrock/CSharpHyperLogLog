using CSharpHyperLogLog.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class UtilsTests
    {
    // TODO Tests for ints (and not ulong)

        [TestMethod]
        public void ExtractBitsTest()
        {
            // 1001 1110 1101 0110 1110 1011 0101 0110 0101 0111 1000 1000 1110 0011 1001 0111
            ulong initial = 11445594259076998039UL;
            ulong expected;

            // 1001 1110 (left bits)
            expected = 158;
            ulong first8 = initial.ExtractBits(64 - 8, 64);
            Assert.AreEqual(expected, first8);

            first8 = initial.ExtractBits(56, 250);
            Assert.AreEqual(expected, first8);

            // x001 1110 (left bits)
            expected = 30;
            ulong first7Except1 = initial.ExtractBits(56, 63);
            Assert.AreEqual(expected, first7Except1);

            // 1110 0011 1001 0111 (right bits)
            expected = 58263;
            ulong last16 = initial.ExtractBits(0, 16);
            Assert.AreEqual(expected, last16);

            // 19th bit
            expected = 1;
            ulong nineteenthBit = initial.ExtractBits(19, 20);
            Assert.AreEqual(expected, nineteenthBit);

            // 0101 0110 0101 0111 1000 (20 bits in the middle)
            expected = 353656;
            ulong middleBits = initial.ExtractBits(20, 40);
            Assert.AreEqual(expected, middleBits);

            // Errors
            try
            {
                initial.ExtractBits(-1, 64);
                Assert.Fail("Should throw exception. Negative beginning index.");
            }
            catch (ArgumentException) { }
            try
            {
                initial.ExtractBits(0, -7);
                Assert.Fail("Should throw exception. Ending index negative and lower than beginning");
            }
            catch (ArgumentException) { }
            try
            {
                initial.ExtractBits(50, 20);
                Assert.Fail("Should throw exception. Ending index is lower than beginning index");
            }
            catch (ArgumentException) { }
            try
            {
                initial.ExtractBits(15, 15);
                Assert.Fail("Should throw exception. Ending index is equal to the beginning index");
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void NumberOfLeadingZerosTest()
        {
            // 0000 0000 1101 0000 1110 1011 0101 0110 0101 0111 1000 1000 1110 0011 1001 0111
            ulong initial = 58805551224120215UL;
            ulong expected;

            // TODO +1 or not
            expected = 9; // 8 without in reality
            byte nbLeadingZeros_All = initial.NumberOfLeadingZeros();
            Assert.AreEqual(expected, nbLeadingZeros_All);

            expected = 5;
            byte nbLeadingZeros_Less4 = initial.NumberOfLeadingZeros(60);
            Assert.AreEqual(expected, nbLeadingZeros_Less4);

            try
            {
                initial.NumberOfLeadingZeros(90);
                Assert.Fail("Should throw exception. Given size is greater than 64");
            }
            catch (ArgumentException) { }
        }
    }
}
