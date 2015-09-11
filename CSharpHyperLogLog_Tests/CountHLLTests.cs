using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class CountHLLTests
    {
        [TestMethod]
        public void BasicCountTest()
        {
            HyperLogLog hll = new HyperLogLog();
            Assert.IsTrue(hll.Add(1), "should alter a register");
            Assert.IsTrue(hll.Add(2), "should alter a register");
            Assert.IsTrue(hll.Add(3), "should alter a register");
            Assert.IsTrue(hll.Add(4), "should alter a register");

            Assert.AreEqual(4UL, hll.Count, "should count 4 elements");
        }

        [TestMethod]
        public void CountWithDuplicatesTest()
        {
            HyperLogLog hll = new HyperLogLog();

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

            Assert.AreEqual(4UL, hll.Count, "should count 4 elements");
        }

        [TestMethod]
        public void CountHashedValuesTest()
        {
            HyperLogLog hll = new HyperLogLog();

            // Murmur3 hashes
            ulong hash1 = 3384900212040232317; // a
            ulong hash2 = 3470519952522631238; // b
            ulong hash3 = 13910917782391787153; // c

            Assert.IsTrue(hll.AddHash(hash1), "should add hash");
            Assert.IsTrue(hll.AddHash(hash2), "should add hash");
            Assert.IsTrue(hll.AddHash(hash3), "should add hash");

            // First hash corresponds to Murmur3 hash of "a"
            Assert.IsFalse(hll.Add("a"), "should not alter a register");

            Assert.AreEqual(3UL, hll.Count, "should count 3 elements");
        }
    }
}
