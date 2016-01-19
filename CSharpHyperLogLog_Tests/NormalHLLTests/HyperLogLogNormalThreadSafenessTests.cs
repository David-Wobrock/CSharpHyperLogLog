using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Collections.Generic;
using CSharpHyperLogLog;

namespace CSharpHyperLogLog_Tests.NormalHLLTests
{
    [TestClass]
    public class HyperLogLogNormalThreadSafenessTests
    {
        private static ulong Counter = 0;
        private static object CounterLocker = new object();

        private static ulong GetCounter()
        {
            ulong value;

            lock(CounterLocker)
                value = Counter++;

            return value;
        }

        private void ThreadTask(HyperLogLog hll, int nbIt)
        {
            for (int i = 0; i < nbIt; ++i)
                hll.Add(GetCounter());
        }

        [TestMethod]
        public void HllNormalThreadSafenessTest()
        {
            const int NB_THREADS = 200;
            const int ITERATIONS = 5000;
            const int PRECISION = 14;
            const ulong expected = NB_THREADS * ITERATIONS;

            HyperLogLog hllThreaded = new HyperLogLogNormal(PRECISION);
            // Launch all threads
            IList<Thread> threads = new List<Thread>();
            for (int i = 0; i < NB_THREADS; ++i)
            {
                Thread t = new Thread(() => ThreadTask(hllThreaded, ITERATIONS));
                threads.Add(t);
                t.Start();
            }
            // Wait for threads
            for (int i = 0; i < NB_THREADS; ++i)
                threads[i].Join();

            // Assert
            TestsHelper.AssertRelativeError(expected, hllThreaded.Cardinality);
        }
    }
}
