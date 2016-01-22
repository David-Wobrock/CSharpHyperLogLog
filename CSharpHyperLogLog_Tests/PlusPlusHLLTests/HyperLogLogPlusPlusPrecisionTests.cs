using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;

namespace CSharpHyperLogLog_Tests.PlusPlusHLLTests
{
    [TestClass]
    public class HyperLogLogPlusPlusPrecisionTests
    {
        [TestMethod]
        public void HllPlusPlusPrecisionTest()
        {
            const ulong expected = 10000;

            byte[] testPrecisions = new byte[] { 4, 12, 16 };
            byte[] testSparsePrecisions = new byte[] { 18, 20, 24, 25, 28, 32, 46, 52, 63 };
            foreach (byte p in testPrecisions)
            {
                foreach (byte sp in testSparsePrecisions)
                {
                    HyperLogLog hll = new HyperLogLogPlusPlus(p, sp);
                    for (ulong i = 0; i < expected; ++i)
                        hll.Add(i);
                        
                    TestsHelper.AssertRelativeError(expected, hll.Cardinality);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusInvalidPrecisionErrorTest()
        {
            new HyperLogLogPlusPlus(3, 25);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusInvalidRangeErrorTest()
        {
            new HyperLogLogPlusPlus(20, 16);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusSPTooLargeErrorTest()
        {
            new HyperLogLogPlusPlus(16, 64);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusPTooLargeErrorTest()
        {
            new HyperLogLogPlusPlus(16, 15);
        }
    }
}
