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

            TestsHelper.AssertRelativeError(1000000UL, hll.Cardinality, 14);
        }
    }
}
