using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;
using System.Collections.Generic;

namespace CSharpHyperLogLog_Tests.PlusPlusHLLTests
{
    [TestClass]
    public class HyperLogLogPlusPlusTests
    {
        private HyperLogLog hll;

        [TestInitialize]
        public void TestsInit()
        {
            hll = new HyperLogLogPlusPlus(14, 18);
        }

        [TestMethod]
        public void HllPlusPlusBasicCountTest()
        {
            Assert.IsTrue(hll.Add(1), "should alter a register");
            Assert.IsTrue(hll.Add(2), "should alter a register");
            Assert.IsTrue(hll.Add(3), "should alter a register");
            Assert.IsTrue(hll.Add(4), "should alter a register");

            Assert.AreEqual(4UL, hll.Cardinality, "should count 4 elements");

            Assert.IsTrue(hll.Add(5), "should alter a register");
            Assert.AreEqual(5UL, hll.Cardinality, "should count 5 elements");
        }

        [TestMethod]
        public void HllPlusPlusCountWithDuplicatesTest()
        {
            // Add ones
            Assert.IsTrue(hll.Add(1), "should alter a register");
            Assert.IsFalse(hll.Add(1), "should not alter a register");
            Assert.IsFalse(hll.Add(1), "should not alter a register");

            // Add 2
            Assert.IsTrue(hll.Add(2), "should alter a register");

            // Add threes
            Assert.IsTrue(hll.Add(3), "should alter a register");
            Assert.IsFalse(hll.Add(3), "should not alter a register");
            Assert.IsFalse(hll.Add(3), "should not alter a register");

            // Add 4
            Assert.IsTrue(hll.Add(4), "should alter a register");
            // Re-add 1
            Assert.IsFalse(hll.Add(1), "should not alter a register");

            Assert.AreEqual(4UL, hll.Cardinality, "should count 4 elements");
        }

        [TestMethod]
        public void HllPlusPlusCountHashedValuesTest()
        {
            // Murmur3 hashes
            ulong hash1 = 3384900212040232317; // a
            ulong hash2 = 3470519952522631238; // b
            ulong hash3 = 13910917782391787153; // c

            Assert.IsTrue(hll.AddHash(hash1), "should add hash");
            Assert.IsTrue(hll.AddHash(hash2), "should add hash");
            Assert.IsTrue(hll.AddHash(hash3), "should add hash");

            // First hash corresponds to Murmur3 hash of "a"
            Assert.IsFalse(hll.Add("a"), "should not alter a register");

            Assert.AreEqual(3UL, hll.Cardinality, "should count 3 elements");
        }

        [TestMethod]
        public void HllPlusPlusCountDifferentValuesTest()
        {
            IList<ulong> testedValues = new List<ulong> { 10, 200, 10000, 11500, 15000, 30000, 100000 };

            foreach (ulong value in testedValues)
            {
                hll = new HyperLogLogPlusPlus(14, 18);
                for (ulong i = 0; i < value; ++i)
                    hll.Add(i);
                TestsHelper.AssertRelativeError(value, hll.Cardinality);
            }
        }
    }
}
