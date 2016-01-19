using System;
using CSharpHyperLogLog.Utils;
using CSharpHyperLogLog.Hash;
using System.Linq;

namespace CSharpHyperLogLog
{
    public class HyperLogLogNormal : HyperLogLog
    {
        private const ushort MIN_PRECISION = 4;
        private const ushort MAX_PRECISION = 28;
        private const double MAX_USED_MEMORY = (1 << MAX_PRECISION) / 1000000D;

        private static readonly IHasher Hasher = new Murmur3();

        public HyperLogLogNormal(ushort precision)
            : base(precision)
        {
            if (precision < MIN_PRECISION)
                throw new ArgumentException(string.Format("A precision below {0} is useless. You won't be able to estimate any collections.", MIN_PRECISION));
            if (precision > MAX_PRECISION)
                throw new ArgumentException(string.Format("A precision above {0} will use too much memory (~{1} MegaBytes).", MAX_PRECISION, MAX_USED_MEMORY));
        }

        public override ulong Cardinality
        {
            get
            {
                double sum = 0D;
                int emptyRegisters = 0;
                foreach (byte r in Registers)// TODO parallel ?
                {
                    sum += 1D / (1 << r); // 2^-r = (2^r)^-1 = 1/(2^r)
                    if (r == 0)
                        ++emptyRegisters;
                }

                double estimate = AlphaMM * (1 / sum);

                double result;
                if (estimate <= 2.5 * M)
                    result = LinearCounting(Convert.ToInt32(M), emptyRegisters);
                else
                    result = estimate;

                return Convert.ToUInt64(result);
            }
        }

        public override bool Add(object value)
        {
            return AddHash(Hasher.Hash(value));
        }

        public override bool AddHash(ulong hash)
        {
            ulong firstBits = hash.ExtractBits(64 - Precision, 64); // Gives the index between 0 and M

            byte nbLeadingZeros = hash.ExtractBits(0, 64 - Precision).NumberOfLeadingZeros(64 - Precision);

            return UpdateIfGreater(ref Registers[firstBits], nbLeadingZeros);
        }

        public override bool Merge(HyperLogLog hll)
        {
            if (hll == null)
                throw new ArgumentException("Parameter hyperloglog cannot be null");

            if (!(hll is HyperLogLogNormal))
                throw new ArgumentException("Parameter hyperloglog instance must be 'normal' too");

            HyperLogLogNormal hllN = hll as HyperLogLogNormal;
            if (Precision != hllN.Precision)
                throw new ArgumentException("Both hyperloglog instances must have the same precision");

            return Merge(hllN);
        }

        private bool Merge(HyperLogLogNormal hll)
        {
            bool modified = false;
            for (uint i = 0; i < hll.Registers.Count(); ++i)
                if (UpdateIfGreater(ref Registers[i], hll.Registers[i]))
                    modified = true;

            return modified;
        }
    }
}
