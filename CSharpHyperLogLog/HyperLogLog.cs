﻿using System;

namespace CSharpHyperLogLog
{
    public abstract class HyperLogLog
    {
        protected const ushort MIN_PRECISION = 4;
        protected const ushort MAX_PRECISION = 28;

        protected readonly byte Precision;
        protected readonly uint M;
        protected readonly double AlphaMM;

        // We use the "byte" type because the size of each register must be log2(log2(N)), when cardinalities <= N.
        // Since we use hashed values on 64 bits, N = 2^64. So, log2(log2(2^64)) = 6. So we only need 6 bits
        protected byte[] Registers;

        protected object RegisterLock = new object();

        public HyperLogLog(byte precision)
        {
            Precision = precision;
            M = 1U << precision;
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
        protected static double LinearCounting(ulong m, double v)
        {
            return m * Math.Log(m / v);
        }
    }
}
