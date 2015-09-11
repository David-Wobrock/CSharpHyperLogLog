using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using CSharpHyperLogLog;
using System.Collections.Generic;

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
            HyperLogLog hll = new HyperLogLog(PRECISION);

            IList<Thread> threads = new List<Thread>();
            for (int i = 0; i < NB_THREADS; ++i)
            {
                Thread t = new Thread(() => ThreadTask(hll, ITERATIONS));
                threads.Add(t);
                t.Start();
            }
            for (int i = 0; i < NB_THREADS; ++i)
                threads[i].Join();

            ulong a = hll.Cardinality;
            ulong expected = NB_THREADS * ITERATIONS;
            double delta = TestsHelper.GetDelta(expected, PRECISION);
            Assert.AreEqual(expected, a, delta, "should be approximatly equal (error of {0} possible)", delta);
        }
    }
}
