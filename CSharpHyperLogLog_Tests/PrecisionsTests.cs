using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;
using System.Collections.Generic;

namespace CSharpHyperLogLog_Tests
{
    public class PrecisionsTests
    {
        public void HllNormalPrecisionTest()
        {
            const ulong expected = 10000;

            IList<int> testPrecisions = new List<int>() { 4, 12, 16, 24, 28 };
            foreach (int p in testPrecisions)
            {
                HyperLogLog_Old hll = new HyperLogLog_Old(p);
                for (ulong i = 0; i < expected; ++i)
                    hll.Add(i);

                double delta = TestsHelper.GetDelta(expected, p);
                TestsHelper.AssertRelativeError(expected, hll.Cardinality);
            }
        }

        public void HllNormalAllPrecisionsTest()
        {
            // Negative
            for (int i = -50; i < -1; ++i)
            {
                try
                {
                    HyperLogLog_Old hll = new HyperLogLog_Old(i);
                    Assert.Fail("Should not be able to create HLL instance with precision {0}", i);
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual("The precision cannot be negative.", ex.Message, "error message not as expected");
                }
            }

            // Above 0 but too small
            for (int i = 0; i < 4; ++i)
            {
                try
                {
                    HyperLogLog_Old hll = new HyperLogLog_Old(i);
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
                HyperLogLog_Old hll = new HyperLogLog_Old(i);
                for (ulong j = 0; j < NB_IT; ++j)
                    hll.Add(j);
                TestsHelper.AssertRelativeError(NB_IT, hll.Cardinality);
            }

            // Above 29
            for (int i = 29; i < 50; ++i)
            {
                try
                {
                    HyperLogLog_Old hll = new HyperLogLog_Old(i);
                    Assert.Fail("Should not be able to create HLL instance with precision {0}", i);
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual("A precision above 28 will use too much memory (~500 MegaBytes).", ex.Message, "error message not as expected");
                }
            }
        }

        public void HllPlusPrecisionTest()
        {
            const ulong expected = 10000;

            int[] testPrecisions = new int[] { 4, 12, 16 };
            int[] testSparsePrecisions = new int[] { 18, 20, 24, 25, 28, 32, 46, 52, 64 };
            foreach (int p in testPrecisions)
            {
                foreach (int sp in testSparsePrecisions)
                {
                    HyperLogLog_Old hll = new HyperLogLog_Old(p, sp);
                    for (ulong i = 0; i < expected; ++i)
                        hll.Add(i);

                    double delta = TestsHelper.GetDelta(expected, p);
                    TestsHelper.AssertRelativeError(expected, hll.Cardinality);
                }
            }
        }

        public void HllPlusAllPrecisionsTest()
        {
            int p, sp;

            // Precision invalid
            sp = 25;
            for (p = -50; p < 4; ++p)
            {
                try
                {
                    new HyperLogLog_Old(p, sp);
                    Assert.Fail("Should fail because invalid precision {0}", p);
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual(string.Format("The precision {0} must be between 4 and {1}", p, Math.Min(sp, 28)), ex.Message);
                }
            }

            // Precision > sparse precision
            p = 20;
            sp = 16;
            try
            {
                new HyperLogLog_Old(p, sp);
                Assert.Fail("Should fail because precision {0} greater than sparse {1}", p, Math.Min(sp, 28));
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(string.Format("The precision {0} must be between 4 and {1}", p, Math.Min(sp, 28)), ex.Message);
            }

            // Okay
            const ulong expected = 10000;
            for (sp = 4; sp < 65; ++sp)
            {
                for (p = 4; p < sp; ++p)
                {
                    HyperLogLog_Old hll = new HyperLogLog_Old(p, sp);
                    for (ulong i = 0; i < expected; ++i)
                        hll.Add(i);
                    TestsHelper.AssertRelativeError(expected, hll.Cardinality);
                }
            }

            // Sparse precision too large
            p = 16;
            for (sp = 65; sp < 100; ++sp)
            {
                try
                {
                    new HyperLogLog_Old(p, sp);
                    Assert.Fail("Should not reach this code because sp is greater than 64.");
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual(string.Format("The sparse precision {0} must be inferior or equal to 64.", sp), ex.Message);
                }
            }

            // Precision too large
            for (sp = 10; sp < 64; ++sp)
            {
                try
                {
                    p = sp + 1;
                    new HyperLogLog_Old(p, sp);
                    Assert.Fail("Should not reach this code because precision is greater than the sparse precision");
                }
                catch (ArgumentException ex)
                {
                    Assert.AreEqual(string.Format("The precision {0} must be between 4 and {1}", p, Math.Min(sp, 28)), ex.Message);
                }
            }
        }
    }
}
