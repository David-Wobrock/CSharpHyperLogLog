using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        public static double GetRelativeError(ulong expected, ulong actual)
        {
            return (actual > expected ? actual - expected : expected - actual) / (double)expected;
        }

        public static void AssertRelativeError(ulong expected, ulong actual, int precision, string message = null)
        {
            double expectedError = GetAccuracy(precision);
            double realError = GetRelativeError(expected, actual);

            if (string.IsNullOrWhiteSpace(message))
                message = string.Format("{0} error should be lower than {1} (precision {2})", realError, expectedError, precision);
            
            Assert.IsTrue(realError < expectedError, message);
        }

        /// <summary>
        /// Asserts that the relative error is below a certain value (0.1 now)
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AssertRelativeError(ulong expected, ulong actual, string message = null)
        {
            const double testedRelativeError = 0.15;

            double realError = (actual > expected ? actual - expected : expected - actual) / (double)expected;

            if (string.IsNullOrWhiteSpace(message))
                message = string.Format("{0} error should be lower than {1}", realError, testedRelativeError);

            Assert.IsTrue(realError < testedRelativeError, message);
        }
    }
}
