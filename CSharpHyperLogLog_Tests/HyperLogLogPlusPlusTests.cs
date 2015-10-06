using CSharpHyperLogLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class HyperLogLogPlusPlusTests
    {
        [TestMethod]
        public void HllPlusBasicCountTest()
        {
            HyperLogLog hllPlus = new HyperLogLog(16, 25);

            hllPlus.Add("Hello world!");
            hllPlus.Add("Hallo Welt!");
            hllPlus.Add("Bonjour monde!");
            Assert.AreEqual(3UL, hllPlus.Cardinality, "should find exact cardinility when very small (3 elements)");

            const ulong limit = 25000;
            for (ulong i = 0; i < limit; ++i)
                hllPlus.Add(i);

            ulong result = hllPlus.Cardinality;
            Assert.AreEqual(limit+3, result, TestsHelper.GetDelta(limit+3, 25), "should count correctly small cardinalities in sparse representation");
        }

        [TestMethod]
        public void HllPlusHighCountTest()
        {
            HyperLogLog hllPlus = new HyperLogLog(16, 25);

            const ulong NB = 300000;
            for (ulong i = 0; i < NB; ++i)
                hllPlus.Add(i);

            ulong result = hllPlus.Cardinality;
            Assert.AreEqual(NB, result, TestsHelper.GetDelta(NB, 16), "should have converted to a normal/dense representation");
        }

        [TestMethod]
        public void HllPlusCountListTest()
        {
            IList<string> testList = new List<string>()
            {
                "a",
                "a",
                "b",
                "c",
                "d",
                "e",
                "d"
            };

            Assert.AreEqual(5UL, HyperLogLog.Count<string>(testList, 14, 25), "should count 5 elements");
        }
    }
}
