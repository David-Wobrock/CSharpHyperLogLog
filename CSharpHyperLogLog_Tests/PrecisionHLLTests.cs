using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;
using System.Collections.Generic;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class PrecisionHLLTests
    {
        [TestMethod]
        public void PrecisionTest()
        {
            const ulong expected = 1000000;

            IList<int> testPrecisions = new List<int>() { 4, 12, 16, 24, 28 };
            foreach (int p in testPrecisions)
            {
                double delta = TestsHelper.GetDelta(expected, p);
                Assert.AreEqual(1000000, HyperLogLog.Count<int>(TestsHelper.GetOneMillionDifferentElements(), p), delta, "should be approximately equal (precision {0} and delta {1})", p, delta);
            }
        }
    }
}
