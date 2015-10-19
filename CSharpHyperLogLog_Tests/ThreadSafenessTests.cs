using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using CSharpHyperLogLog;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSharpHyperLogLog_Tests
{
    [TestClass]
    public class ThreadSafenessTests
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
        public void HllNormalThreadSafenessTest()
        {
            const int NB_THREADS = 100;
            const int ITERATIONS = 500;
            const int PRECISION = 14;
            ulong expected = NB_THREADS * ITERATIONS;

            HyperLogLog hllThreaded = new HyperLogLog(PRECISION);
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

        [TestMethod]
        public void HllPlusThreadSafenessTest()
        {
            Assert.Fail("TODO : try thread safeness");
            const int NB_THREADS = 100;
            const int ITERATIONS = 500;
            const int PRECISION = 14;
            const int SPARSE_PRECISION = 25;
            ulong expected = NB_THREADS * ITERATIONS;

            HyperLogLog hllThreaded = new HyperLogLog(PRECISION, SPARSE_PRECISION);
            IList<Thread> threads = new List<Thread>();
            for (int i = 0; i < NB_THREADS; ++i)
            {
                Thread t = new Thread(() => ThreadTask(hllThreaded, ITERATIONS));
                threads.Add(t);
                t.Start();
            }
            for (int i = 0; i < NB_THREADS; ++i)
                threads[i].Join();

            TestsHelper.AssertRelativeError(expected, hllThreaded.Cardinality);
        }
    }
}
