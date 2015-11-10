using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;
using System;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class MergeTests
    {
        [TestMethod]
        public void HllNormalMergeTest()
        {
            HyperLogLog hll1 = new HyperLogLog(14);
            HyperLogLog hll2 = new HyperLogLog(14);

            // Init
            hll1.Add(1);
            hll1.Add(2);
            hll1.Add(3);
            hll1.Add(4);
            hll1.Add(4);

            hll2.Add(3);
            hll2.Add(3);
            hll2.Add(4);
            hll2.Add(5);
            hll2.Add(6);
            hll2.Add(7);
            hll2.Add(7);

            Assert.IsTrue(hll1.Merge(hll2), "Merge should alter some registers and return true");
            Assert.AreEqual(7UL, hll1.Cardinality, "Cardinality after merge should be up-to-date");

            // Error tests
            hll2 = new HyperLogLog(15); // Different precision
            try
            {
                hll1.Merge(hll2);
                Assert.Fail("Should not reach this code");
            }
            catch (ArgumentException) { }

            hll2 = new HyperLogLog(14, 16); // HLL++
            try
            {
                hll1.Merge(hll2);
                Assert.Fail("Should not reach this code");
            }
            catch (ArgumentException) { }
        }

        [TestMethod]
        public void HllPlusMergeTest()
        {
            HyperLogLog hll1 = new HyperLogLog(14, 25);
            HyperLogLog hll2 = new HyperLogLog(14, 25);

            // Small values (sparse representation)
            hll1.Add(1);
            hll1.Add(2);
            hll1.Add(3);
            hll1.Add(4);
            hll1.Add(4);

            hll2.Add(3);
            hll2.Add(3);
            hll2.Add(4);
            hll2.Add(5);
            hll2.Add(6);
            hll2.Add(7);
            hll2.Add(7);

            Assert.IsTrue(hll1.Merge(hll2), "Merge should alter some registers and return true");
            Assert.AreEqual(7UL, hll1.Cardinality, "Cardinality after merge should be up-to-date");

            // Bigger values (dense representation)
            hll1 = new HyperLogLog(14, 25);
            hll2 = new HyperLogLog(14, 25);

            int i;
            for (i = 0; i < 150000; ++i)
                hll1.Add(i);
            for (i = 100000; i < 300000; ++i)
                hll2.Add(i);
                
            Assert.IsTrue(hll1.Merge(hll2), "Merge should alter some registers and return true");
            TestsHelper.AssertRelativeError(300000UL, hll1.Cardinality, "Cardinality after merge should approximatively be as expected");

            // Error tests
            hll2 = new HyperLogLog(12, 25);
            try
            {
                hll1.Merge(hll2);
                Assert.Fail("Should not reach this code");
            }
            catch (ArgumentException) { }

            hll2 = new HyperLogLog(14, 30);
            try
            {
                hll1.Merge(hll2);
                Assert.Fail("Should not reach this code");
            }
            catch (ArgumentException) { }

            hll2 = new HyperLogLog(10, 20);
            try
            {
                hll1.Merge(hll2);
                Assert.Fail("Should not reach this code");
            }
            catch (ArgumentException) { }

            hll2 = new HyperLogLog(10, 20);
            try
            {
                hll1.Merge(hll2);
                Assert.Fail("Should not reach this code");
            }
            catch (ArgumentException) { }
        }
    }
}
