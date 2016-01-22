using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog.Utils;

namespace CSharpHyperLogLog_Tests.UtilsTests
{
    [TestClass]
    public class IntExtensionsTests
    {
        [TestMethod]
        public void ExtractBitsTest()
        {
            // 1001 1110 1101 0110 1110 1011 0101 0110 0101 0111 1000 1000 1110 0011 1001 0111
            const ulong initial = 11445594259076998039UL;
            ulong expected;

            // 1001 1110 (left bits)
            expected = 158;
            ExtractBitsAndAssert(initial, expected, 64 - 8, 64);
            ExtractBitsAndAssert(initial, expected, 56, 250);

            // x001 1110 (left bits)
            expected = 30;
            ExtractBitsAndAssert(initial, expected, 56, 63);

            // 1110 0011 1001 0111 (right bits)
            expected = 58263;
            ExtractBitsAndAssert(initial, expected, 0, 16);

            // 19th bit
            expected = 1;
            ExtractBitsAndAssert(initial, expected, 19, 20);

            // 0101 0110 0101 0111 1000 (20 bits in the middle)
            expected = 353656;
            ExtractBitsAndAssert(initial, expected, 20, 40);
        }

        private void ExtractBitsAndAssert(ulong value, ulong expectedExtractedValue, int from, int to)
        {
            ulong extractedValue = value.ExtractBits(from, to);
            Assert.AreEqual(expectedExtractedValue, extractedValue);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtractBitsNegativeErrorTest()
        {
            1234.ExtractBits(-1, 64);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtractBitsLowerErrorTest()
        {
            1234.ExtractBits(0, -7);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtractBitsRangeErrorTest()
        {
            1234.ExtractBits(50, 20);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExtractBitsEqualErrorTest()
        {
            1234.ExtractBits(15, 15);
        }

        [TestMethod]
        public void NumberOfLeadingZerosTest()
        {
            // 0000 0000 1101 0000 1110 1011 0101 0110 0101 0111 1000 1000 1110 0011 1001 0111
            ulong initial = 58805551224120215UL;
            ulong expected;

            expected = 9; // 8 without in reality
            byte nbLeadingZeros_All = initial.NumberOfLeadingZeros();
            Assert.AreEqual(expected, nbLeadingZeros_All);

            expected = 5;
            byte nbLeadingZeros_Less4 = initial.NumberOfLeadingZeros(60);
            Assert.AreEqual(expected, nbLeadingZeros_Less4);
        }
    }
}
