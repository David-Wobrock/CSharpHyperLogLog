using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;

namespace CSharpHyperLogLog_Tests.PlusPlusHLLTests
{
    [TestClass]
    public class HyperLogLogPlusPlusMergeTests
    {
        private HyperLogLog hll1;
        private const byte PRECISION_DEFAULT = 14;
        private const byte SPARSE_PRECISION_DEFAULT = 25;

        [TestInitialize]
        public void TestsInit()
        {
            hll1 = new HyperLogLogPlusPlus(PRECISION_DEFAULT, SPARSE_PRECISION_DEFAULT);
        }

        [TestMethod]
        public void HllPlusPlusSmallValuesMergeTest()
        {
            HyperLogLog hll2 = new HyperLogLogPlusPlus(PRECISION_DEFAULT, SPARSE_PRECISION_DEFAULT);

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
        }

        [TestMethod]
        public void HllPlusPlusBigValuesMergeTest()
        {
            // Bigger values (dense representation)
            HyperLogLog hll2 = new HyperLogLogPlusPlus(PRECISION_DEFAULT, SPARSE_PRECISION_DEFAULT);

            int i;
            for (i = 0; i < 150000; ++i)
                hll1.Add(i);
            for (i = 100000; i < 300000; ++i)
                hll2.Add(i);

            Assert.IsTrue(hll1.Merge(hll2), "Merge should alter some registers and return true");
            TestsHelper.AssertRelativeError(300000UL, hll1.Cardinality, "Cardinality after merge should approximatively be as expected");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusDifferentPrecisionErrorTest()
        {
            HyperLogLog hll2 = new HyperLogLogPlusPlus(12, SPARSE_PRECISION_DEFAULT);
            hll1.Merge(hll2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusDifferentSparsePrecisionErrorTest()
        {
            HyperLogLog hll2 = new HyperLogLogPlusPlus(PRECISION_DEFAULT, 30);
            hll1.Merge(hll2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusDifferentPrecisionsErrorTest()
        {
            HyperLogLog hll2 = new HyperLogLogPlusPlus(10, 20);
            hll1.Merge(hll2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusNullParameterErrorTest()
        {
            hll1.Merge(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void HllPlusPlusNormalHllParameterErrorTest()
        {
            HyperLogLog hll2 = new HyperLogLogNormal(PRECISION_DEFAULT);
            hll1.Merge(hll2);
        }
    }
}
