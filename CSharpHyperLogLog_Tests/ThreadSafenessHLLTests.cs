using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using CSharpHyperLogLog;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class ThreadSafenessHLLTests
    {
        private static ulong Counter = 0;
        private static Mutex CounterLock = new Mutex();
        private static ulong GetCounter()
        {
            ulong value;
            CounterLock.WaitOne();
            value = Counter++;
            CounterLock.ReleaseMutex();
            return value;
        }

        private void ThreadTask(HyperLogLog hll, int nbIt)
        {
            for (int i = 0; i < nbIt; ++i)
            {
                hll.Add(GetCounter());
            }
        }

        [TestMethod]
        public void ThreadSafenessTest()
        {
            const int NB_THREADS = 100;
            const int ITERATIONS = 200;
            const int PRECISION = 14;
            ulong expected = NB_THREADS * ITERATIONS;

            // Getting result without threads
            HyperLogLog hll1 = new HyperLogLog(PRECISION);
            for (ulong i = 0; i < expected; ++i)
                hll1.Add(i);
            ulong noThreadResult = hll1.Cardinality;

            // Getting results without threads (other order)
            HyperLogLog hll3 = new HyperLogLog(PRECISION);
            for (ulong i = expected - 1; i != 0; --i)
                hll3.Add(i);
            ulong noThreadsResultsDesc = hll3.Cardinality;

            // Check if with threads we have the same result
            HyperLogLog hll2 = new HyperLogLog(PRECISION);

            IList<Thread> threads = new List<Thread>();
            for (int i = 0; i < NB_THREADS; ++i)
            {
                Thread t = new Thread(() => ThreadTask(hll2, ITERATIONS));
                threads.Add(t);
                t.Start();
            }
            for (int i = 0; i < NB_THREADS; ++i)
                threads[i].Join();

            ulong threadsResult = hll2.Cardinality;

            // Verify results
            double delta = TestsHelper.GetDelta(expected, PRECISION);
            Assert.AreEqual(noThreadResult, expected, delta, "the first result without threads should be equal with a delta of {0} max", delta);
            Assert.AreEqual(noThreadResult, noThreadsResultsDesc, 20D, "both results without threads and different order should be equal with a difference of 20 maximum");
            Assert.AreEqual(noThreadResult, threadsResult, 20D, "both results, with and without threads, should be equal with a difference of 100 maximum");
        }
    }
}
