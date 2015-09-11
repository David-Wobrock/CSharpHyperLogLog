using System;
using System.Collections.Generic;

namespace CSharpHyperLogLog_Tests
{
    public static class TestsHelper
    {
        public static IEnumerable<int> GetOneMillionDifferentElements()
        {
            for (int i = 0; i < 1000000; ++i)
                yield return i;
        }

        public static double GetDelta(ulong expected, int precision)
        {
            return GetAccuracy(precision) * expected;
        }

        public static double GetAccuracy(int precision)
        {
            return 1.04 / (Math.Sqrt(Math.Pow(2, precision)));
        }
    }
}
