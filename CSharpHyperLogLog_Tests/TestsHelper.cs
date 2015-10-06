﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        public static void AssertRelativeError(ulong expected, ulong actual, int precision, string message = null)
        {
            double expectedError = GetAccuracy(precision);
            double realError = (actual > expected ? actual - expected : expected - actual) / (double)expected;

            if (string.IsNullOrWhiteSpace(message))
                message = string.Format("{0} error should be lower than {1} (precision {2})", realError, expectedError, precision);
            
            Assert.IsTrue(realError < expectedError, message);
        }

        /// <summary>
        /// Asserts that the relative error is below 0.1
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="message"></param>
        public static void AssertRelativeError(ulong expected, ulong actual, string message = null)
        {
            double realError = (actual > expected ? actual - expected : expected - actual) / (double)expected;

            if (string.IsNullOrWhiteSpace(message))
                message = string.Format("{0} error should be lower than 0.1", realError);

            Assert.IsTrue(realError < 0.1, message);
        }
    }
}