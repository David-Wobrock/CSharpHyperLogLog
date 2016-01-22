using CSharpHyperLogLog.Hash;
using CSharpHyperLogLog.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpHyperLogLog
{
    /// <summary>
    /// TODO
    /// </summary>
    public class HyperLogLogPlusPlus : HyperLogLog
    {
        private const byte MAX_SPARSE_PRECISION = 63;

        private static readonly IHasher Hasher = new Murmur3();

        private static readonly double[] ThresholdData = new double[] { 10, 20, 40, 80, 220, 400, 900, 1800, 3100, 6500, 11500, 20000, 50000, 120000, 350000 };

        private readonly ushort SparsePrecision;
        private ISet<int> SparseSet = new SortedSet<int>();
        private readonly int SparseRepresentationThreshold;

        private bool IsInSparseRepresentation = true;

        private readonly HashEncodingHelper HashEncoder;

        /// <summary>
        /// Creates a hyperloglog++ instance.
        /// </summary>
        /// <param name="precision">The higher the precision, the higher the accuracy, but also the memory usage. Must be in [4, max(28, sparsePrecision)].</param>
        /// <param name="sparsePrecision">The precision of the sparse representation. Must be inferior or equal to 63.</param>
        public HyperLogLogPlusPlus(byte precision, byte sparsePrecision)
            : base(precision)
        {
            int precisionLimit = Math.Min(sparsePrecision, (byte)28);
            if (precision < MIN_PRECISION || precision > precisionLimit)
                throw new ArgumentException(string.Format("The precision {0} must be between 4 and {1}", precision, precisionLimit));
            if (sparsePrecision > MAX_SPARSE_PRECISION)
                throw new ArgumentException(string.Format("The sparse precision {0} must be inferior or equal to 63.", sparsePrecision));

            SparseRepresentationThreshold = Convert.ToInt32(6 * M);

            SparsePrecision = sparsePrecision;

            HashEncoder = new HashEncodingHelper(precision, sparsePrecision);
        }

        public override ulong Cardinality
        {
            get
            {
                double cardinality;

                if (IsInSparseRepresentation)
                {
                    ulong sparseM = 1UL << SparsePrecision;
                    cardinality = LinearCounting(sparseM, sparseM - (uint) (SparseSet.Count));
                }
                else
                {
                    double sum = 0D;
                    int emptyRegisters = 0;

                    lock (RegisterLock)
                    {
                        sum = Registers.AsParallel().Sum(reg => 1D / (1 << reg));
                        emptyRegisters = Registers.AsParallel().Count(reg => reg == 0);
                    }

                    double estimate = AlphaMM * (1D / sum);

                    // TODO Write my own GetEstimateBias
                    double estimatePrime = estimate <= 5 * M ? (estimate - EstimateBiasHelper.GetEstimateBias(estimate, Precision)) : estimate;

                    double H;
                    if (emptyRegisters != 0)
                        H = LinearCounting((int)M, emptyRegisters);
                    else
                        H = estimatePrime;

                    if (H < Threshold)
                        cardinality = H;
                    else
                        cardinality = estimatePrime;
                }

                return Convert.ToUInt64(cardinality);
            }
        }

        private double Threshold
        {
            get
            {
                return ThresholdData[Precision - MIN_PRECISION];
            }
        }

        public override bool Add(object value)
        {
            return AddHash(Hasher.Hash(value));
        }

        public override bool AddHash(ulong hash)
        {
            bool hasBeenModified;
            if (IsInSparseRepresentation)
            {
                int k = HashEncoder.EncodeHash(hash);
                hasBeenModified = SparseSet.Add(k);

                if (SparseSet.Count > SparseRepresentationThreshold)
                    ToNormalRepresentation();
            }
            else
            {
                ulong firstBits = hash.ExtractBits(64 - Precision, 64); // Gives the index between 0 and M
                byte nbLeadingZeros = hash.ExtractBits(0, 64 - Precision).NumberOfLeadingZeros(64 - Precision);
                hasBeenModified = UpdateIfGreater(ref Registers[firstBits], nbLeadingZeros);
            }

            return hasBeenModified;
        }

        private void ToNormalRepresentation()
        {
            Registers = new byte[M];

            foreach (int elem in SparseSet)
            {
                int idx;
                byte value;
                HashEncoder.DecodeHash(elem, out idx, out value);
                Registers[idx] = Math.Max(Registers[idx], value);
            }

            SparseSet = null;
            IsInSparseRepresentation = false;
        }


        public override bool Merge(HyperLogLog hll)
        {
            if (hll == null)
                throw new ArgumentException("Parameter hyperloglog cannot be null");

            if (!(hll is HyperLogLogPlusPlus))
                throw new ArgumentException("Parameter hyperloglog instance must be 'normal' too");


            HyperLogLogPlusPlus hllPP = hll as HyperLogLogPlusPlus;

            if (IsInSparseRepresentation != hllPP.IsInSparseRepresentation)
                throw new ArgumentException("Both hyperloglog instances must have the same representation.");
            if (Precision != hllPP.Precision)
                throw new ArgumentException("Both hyperloglog instances must have the same precision");
            if (IsInSparseRepresentation && SparsePrecision != hllPP.SparsePrecision)
                throw new ArgumentException("Both hyperloglog instances must have the same sparse precision");

            bool smthingHasBeenModified = Merge(hllPP);
            return smthingHasBeenModified;
        }

        private bool Merge(HyperLogLogPlusPlus hll)
        {
            bool modified = false;

            if (IsInSparseRepresentation)
            {
                foreach (int encodedValue in hll.SparseSet)
                    if (SparseSet.Add(encodedValue))
                        modified = true;

                if (SparseSet.Count > SparseRepresentationThreshold)
                    ToNormalRepresentation();
            }
            else
            {
                for (int i = 0; i < hll.Registers.Count(); ++i)
                    if (UpdateIfGreater(ref Registers[i], hll.Registers[i]))
                        modified = true;
            }

            return modified;
        }
    }
}
