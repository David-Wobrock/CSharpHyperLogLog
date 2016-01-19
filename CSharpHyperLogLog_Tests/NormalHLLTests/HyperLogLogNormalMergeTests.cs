using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;

namespace CSharpHyperLogLog_Tests.NormalHLLTests
{
    [TestClass]
    public class HyperLogLogNormalMergeTests
    {
        [TestMethod]
        public void HllNormalMergeTest()
        {
            HyperLogLog hll1 = new HyperLogLogNormal(14);
            HyperLogLog hll2 = new HyperLogLogNormal(14);

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
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllNormalMergePrecisionErrorTest()
        {
            HyperLogLog hll1 = new HyperLogLogNormal(10);
            hll1.Add(1);
            HyperLogLog hll2 = new HyperLogLogNormal(15);
            hll2.Add(2);

            hll1.Merge(hll2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllNormalMergeNullErrorTest()
        {
            HyperLogLog hll1 = new HyperLogLogNormal(10);
            hll1.Add(1);

            hll1.Merge(null);
        }
    }
}
