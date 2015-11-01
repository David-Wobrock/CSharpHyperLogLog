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

        [TestMethod]
        public void EncodingTest()
        {
            // Hash of integer "0".
            // 1110 1010 0010 0101 1010 1000 0001 0110 0100 0011 1101 1111 1011 1111 0111 1101
            ulong hash = 16872076392594915197UL;
            int encodedHash;

            encodedHash = new HashEncodingHelper(16, 25).EncodeHash(hash);
            // 11 1010 1000 1001 0110 1010 0000
            Assert.AreEqual(61380256, encodedHash, "encoded hashes should be the same");

            encodedHash = new HashEncodingHelper(12, 16).EncodeHash(hash);
            // 1 1101 0100 0100 1010
            Assert.AreEqual(119882, encodedHash, "encoded hashes should be the same");

            encodedHash = new HashEncodingHelper(12, 60).EncodeHash(hash);
            // 1100 1000 0111 1011 1111 0111 1110 1110
            Assert.AreEqual(3363567598, (UInt32)encodedHash, "encoded hashes should be the same");

            encodedHash = new HashEncodingHelper(10, 11).EncodeHash(hash);
            // 1110 1010 0010
            Assert.AreEqual(3746, encodedHash, "encoded hashes should be the same");

            encodedHash = new HashEncodingHelper(11, 12).EncodeHash(hash);
            // 111 0101 0001 0000 0101
            Assert.AreEqual(479493, encodedHash, "encoded hashes should be the same");

            encodedHash = new HashEncodingHelper(27, 63).EncodeHash(hash);
            // 0100 0011 1101 1111 1011 1111 0111 1100
            Assert.AreEqual(1138737020U, (UInt32)encodedHash, "encoded hashes should be the same");

            try
            {
                encodedHash = new HashEncodingHelper(10, 10).EncodeHash(hash);
                Assert.Fail("Cannot encode hash with precision = sparse precision");
            }
            catch (Exception) { }

            // 2158 = 1000 0110 1110
            // Encoded hash : 35 = 0010 0011
            encodedHash = new HashEncodingHelper(35, 36).EncodeHash(2158);
            Assert.AreEqual(35, encodedHash, "encoded hashes should be the same");
        }

        [TestMethod]
        public void DecodingTest()
        {
            Assert.Fail("TODO");
        }

        [TestMethod]
        public void EstimateBiasTest()
        {
            Assert.Fail("TODO");
        }

        [TestMethod]
        public void CalculateDistancesTest()
        {
            Assert.Fail("TODO");
        }

        [TestMethod]
        public void NearestNeighborsTest()
        {
            Assert.Fail("TODO");
        }

        [TestMethod]
        public void BiasTest()
        {
            Assert.Fail("TODO");
        }
    }
}
