using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog.Utils;

namespace CSharpHyperLogLog_Tests.UtilsTests
{
    [TestClass]
    public class HashEncodingTests
    {
        [TestMethod]
        public void EncodingTest()
        {
            // Hash of integer "0".
            // 1110 1010 0010 0101 1010 1000 0001 0110 0100 0011 1101 1111 1011 1111 0111 1101
            ulong hash = 16872076392594915197UL;

            // 11 1010 1000 1001 0110 1010 0000
            EncodeWithPrecisionsAndAssert(hash, 61380256, 16, 25);

            // 1 1101 0100 0100 1010
            EncodeWithPrecisionsAndAssert(hash, 119882, 12, 16);

            // 1100 1000 0111 1011 1111 0111 1110 1110
            EncodeWithPrecisionsAndAssert(hash, 3363567598U, 12, 60);

            // 1110 1010 0010
            EncodeWithPrecisionsAndAssert(hash, 3746, 10, 11);

            // 111 0101 0001 0000 0101
            EncodeWithPrecisionsAndAssert(hash, 479493, 11, 12);

            // 0100 0011 1101 1111 1011 1111 0111 1100
            EncodeWithPrecisionsAndAssert(hash, 1138737020U, 27, 63);

            // 2158 = 1000 0110 1110
            // Encoded hash : 35 = 0010 0011
            EncodeWithPrecisionsAndAssert(2158, 35, 35, 36);
        }

        private void EncodeWithPrecisionsAndAssert(ulong hash, int expectedEncodedHash, byte precision, byte sparsePrecision)
        {
            int encodedHash = new HashEncodingHelper(precision, sparsePrecision).EncodeHash(hash);
            Assert.AreEqual(expectedEncodedHash, encodedHash, "encoded hashes should be the same");
        }

        private void EncodeWithPrecisionsAndAssert(ulong hash, uint expectedEncodedHash, byte precision, byte sparsePrecision)
        {
            int encodedHash = new HashEncodingHelper(precision, sparsePrecision).EncodeHash(hash);
            Assert.AreEqual(expectedEncodedHash, (uint)encodedHash, "encoded hashes should be the same");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EncodingFailTest()
        {
            new HashEncodingHelper(10, 10).EncodeHash(1234);
        }

        [TestMethod]
        public void DecodingTest()
        {
            HashEncodingHelper encodingHelper = new HashEncodingHelper(14, 25);

            int encodedHash;
            int expectedIndex;
            byte expectedR;

            encodedHash = 638739; // 1001 1011 1111 0001 0011
            expectedIndex = 4990; // 1 0011 0111 1110
            expectedR = 20; // 1001 + ToBinary(25 - 14)
            DecodeAndAssert(encodingHelper, encodedHash, expectedIndex, expectedR);

            encodedHash = 3567856; // 11 0110 0111 0000 1111 0000
            expectedIndex = 14456; // 11 1000 0111 1000
            expectedR = 3; // 11
            DecodeAndAssert(encodingHelper, encodedHash, expectedIndex, expectedR);

            encodedHash = 10; // 1010
            expectedIndex = 5; // 111
            expectedR = 7; // 101
            DecodeAndAssert(encodingHelper, encodedHash, expectedIndex, expectedR);

            encodedHash = 265822207; // 1111 1101 1000 0001 1111 1111 1111
            expectedIndex = 12351; // 11 0000 0011 1111
            expectedR = 74; // 11 1111 + ToBinary(25 - 14)
            DecodeAndAssert(encodingHelper, encodedHash, expectedIndex, expectedR);
        }

        private void DecodeAndAssert(HashEncodingHelper encodingHelper, int encodedHash, int expectedIndex, byte expectedR)
        {
            int idx;
            byte r;

            encodingHelper.DecodeHash(encodedHash, out idx, out r);
            Assert.AreEqual(expectedR, r, "decoded value should be as expected"); // 11
            Assert.AreEqual(expectedIndex, idx, "decoded index should be as expected"); // 11 1000 0111 1000
        }
    }
}
