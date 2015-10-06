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
            const int ITERATIONS = 200;
            const int PRECISION = 14;
            ulong expected = NB_THREADS * ITERATIONS;

            // Getting result without threads
            HyperLogLog hllNormal = new HyperLogLog(PRECISION);
            for (ulong i = 0; i < expected; ++i)
                hllNormal.Add(i);
            ulong noThreadResult = hllNormal.Cardinality;

            HyperLogLog hllThreaded = new HyperLogLog(PRECISION);
            IList<Thread> threads = new List<Thread>();
            for (int i = 0; i < NB_THREADS; ++i)
            {
                Thread t = new Thread(() => ThreadTask(hllThreaded, ITERATIONS));
                threads.Add(t);
                t.Start();
            }
            for (int i = 0; i < NB_THREADS; ++i)
                threads[i].Join();

            ulong threadsResult = hllThreaded.Cardinality;


            // Verify results
            double delta = TestsHelper.GetDelta(expected, PRECISION);
            Assert.AreEqual(noThreadResult, expected, delta, "the first result without threads should be equal with a delta of {0} max", delta);
            Assert.AreEqual(noThreadResult, threadsResult, "both results, with and without threads, should be equal");
        }

        public void HllPlusThreadSafenessTest()
        {
            const int NB_THREADS = 100;
            const int ITERATIONS = 200;
            const int PRECISION = 16;
            const int SPARSE_PRECISION = 25;
            ulong expected = NB_THREADS * ITERATIONS;

            // Getting result without threads
            HyperLogLog hllNormal = new HyperLogLog(PRECISION, SPARSE_PRECISION);
            for (ulong i = 0; i < expected; ++i)
                hllNormal.Add(i);
            ulong noThreadResult = hllNormal.Cardinality;

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

            ulong threadsResult = hllThreaded.Cardinality;


            // Verify results
            double delta = TestsHelper.GetDelta(expected, PRECISION);
            Assert.AreEqual(noThreadResult, expected, delta, "the first result without threads should be equal with a delta of {0} max", delta);
            Assert.AreEqual(noThreadResult, threadsResult, "both results, with and without threads, should be equal");
        }
    }
}
