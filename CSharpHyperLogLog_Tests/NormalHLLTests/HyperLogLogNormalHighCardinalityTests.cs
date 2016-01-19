using Microsoft.VisualStudio.TestTools.UnitTesting;
using CSharpHyperLogLog;

namespace CSharpHyperLogLog_Tests.NormalHLLTests
{
    [TestClass]
    public class HyperLogLogNormalHighCardinalityTests
    {
        [TestMethod]
        public void HllNormalHighCardinalityTest()
        {
            HyperLogLog hll = new HyperLogLogNormal(14);
            foreach (int i in TestsHelper.GetOneMillionDifferentElements())
                hll.Add(i);

            // Test is with precision 14, so relative error is around 1.04 / sqrt(2^14) = 0.008125

            Assert.IsTrue(TestsHelper.GetRelativeError(1000000UL, hll.Cardinality) < 0.009, "relative error should be around 0.008125 (inferior to 0.009 at least)");
        }
    }
}
