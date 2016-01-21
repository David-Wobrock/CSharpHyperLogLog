using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using CSharpHyperLogLog;
using System;

namespace CSharpHyperLogLog_Tests.NormalHLLTests
{
    [TestClass]
    public class HyperLogLogNormalPrecisionTests
    {
        [TestMethod]
        public void HllNormalDifferentPrecisionsTest()
        {
            const ulong expected = 10000;

            IList<int> testPrecisions = new List<int>() { 4, 12, 16, 24, 28 };
            foreach (byte p in testPrecisions)
            {
                HyperLogLog hll = new HyperLogLogNormal(p);
                for (ulong i = 0; i < expected; ++i)
                    hll.Add(i);

                double delta = TestsHelper.GetDelta(expected, p);
                TestsHelper.AssertRelativeError(expected, hll.Cardinality);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllNormalLowPrecisionErrorTest()
        {
            new HyperLogLogNormal(3);
        }
    }
}
