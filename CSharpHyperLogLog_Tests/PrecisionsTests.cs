using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;
using System.Collections.Generic;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class PrecisionsTests
    {
        [TestMethod]
        public void HllNormalPrecisionTest()
        {
            const ulong expected = 10000;

            IList<int> testPrecisions = new List<int>() { 4, 12, 16, 24, 28 };
            foreach (int p in testPrecisions)
            {
                HyperLogLog hll = new HyperLogLog(p);
                for (ulong i = 0; i < expected; ++i)
                    hll.Add(i);

                double delta = TestsHelper.GetDelta(expected, p);
                Assert.AreEqual(expected, hll.Cardinality, delta, "should be approximately equal (precision {0} and delta {1})", p, delta);
            }
        }

        [TestMethod]
        public void HllNormalAllPrecisionsTest()
        {
            // Negative
            for (int i = -50; i < -1; ++i)
            {
                try
                {
                    HyperLogLog hll = new HyperLogLog(i);
                    Assert.Fail("Should not be able to create HLL instance with precision {0}", i);
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual("The precision cannot be negative.", ex.Message, "error message not as expected");
                }
            }

            // Below 0
            for (int i = 0; i < 4; ++i)
            {
                try
                {
                    HyperLogLog hll = new HyperLogLog(i);
                    Assert.Fail("Should not be able to create HLL instance with precision {0}", i);
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual("A precision below 4 is useless. You won't be able to estimate any collections.", ex.Message, "error message not as expected");
                }
            }

            // Okay
            const ulong NB_IT = 200;
            for (int i = 4; i < 29; ++i)
            {
                HyperLogLog hll = new HyperLogLog(i);
                for (ulong j = 0; j < NB_IT; ++j)
                    hll.Add(j);
                TestsHelper.AssertRelativeError(NB_IT, hll.Cardinality);
            }

            // Above 29
            for (int i = 29; i < 50; ++i)
            {
                try
                {
                    HyperLogLog hll = new HyperLogLog(i);
                    Assert.Fail("Should not be able to create HLL instance with precision {0}", i);
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual("A precision above 28 will use too much memory (~500 MegaBytes).", ex.Message, "error message not as expected");
                }
            }
        }

        [TestMethod]
        public void HllPlusPrecisionTest()
        {
            Assert.Fail("TODO");
        }

        [TestMethod]
        public void HllPlusAllPrecisionsTest()
        {
            Assert.Fail("TODO");
        }
    }
}
