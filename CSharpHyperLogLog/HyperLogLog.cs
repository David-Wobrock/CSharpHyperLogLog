using CSharpHyperLogLog.Hash;
using System;

namespace CSharpHyperLogLog
{
    public abstract class HyperLogLog
    {
        protected readonly ushort Precision;
        protected readonly uint M;
        protected readonly double AlphaMM;
        protected byte[] Registers;

        protected object RegisterLock = new object();

        public HyperLogLog(ushort precision)
        {
            Precision = precision;
            M = 1U << precision;
            Registers = new byte[M];
            AlphaMM = Alpha * M * M;
        }

        public abstract ulong Cardinality { get; }

        public abstract bool Add(object value);
        public abstract bool AddHash(ulong hash);

        public abstract bool Merge(HyperLogLog hll);

        protected bool UpdateIfGreater(ref byte register, byte newValue)
        {
            bool isGreater = true;

            lock(RegisterLock)
            {
                if (register >= newValue)
                    isGreater = false;
                else
                    register = newValue;
            }

            return isGreater;
        }

        protected double Alpha
        {
            get
            {
                // Since precision must be higher than 3,
                // M (= 2^precision) cannot be inferior to 16
                switch (M)
                {
                    case 16:
                        return 0.673;
                    case 32:
                        return 0.697;
                    case 64:
                        return 0.709;
                    default:
                        return 0.7213 / (1 + 1.079 / M);
                }
            }
        }

        protected static double LinearCounting(int m, double v)
        {
            return m * Math.Log(m / v);
        }
    }
}
