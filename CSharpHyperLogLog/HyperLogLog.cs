using CSharpHyperLogLog.Hash;
using CSharpHyperLogLog.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpHyperLogLog
{
    /// <summary>
    /// Based on Philippe Flajolet's algorithm : http://algo.inria.fr/flajolet/Publications/FlFuGaMe07.pdf
    /// Improvements based on Google engineer's publication : http://static.googleusercontent.com/media/research.google.com/en//pubs/archive/40671.pdf
    /// </summary>
    public class HyperLogLog
    {
        private static readonly Murmur3 MurmurHasher = new Murmur3();
        private const int DEFAULT_PRECISION = 16;
        private const int HASH_SIZE = 64;

        private static readonly double[] ThresholdData = { 10, 20, 40, 80, 220, 400, 900, 1800, 3100, 6500, 15500, 20000, 50000, 120000, 350000 };

        // Accuracy of 1.04/sqrt(2^Precision) for now.
        private readonly int Precision;
        private readonly int SparsePrecision;
        private readonly int M;
        // We use the "byte" type because the size of each register must be log2(log2(N)), when cardinalities <= N.
        // Since  we use hashed values on 64 bits, N = 2^64. So, log2(log2(2^64)) = 6.
        private byte[] Registers;
        private readonly Mutex RegistersMutex = new Mutex();
        private readonly double AlphaMM;
        private readonly byte LastBitsCount;
        private readonly ulong LastBitsMask;
        private HyperLogLogRepresentation Format;
        private int MaxTempSetSize = 4; // TODO Only initialize when sparse reprensetation
        private ISet<int> TempSet = new SortedSet<int>(); // TODO only initialize when we want to use sparse reprensentation
        private ISet<int> SparseSet = new SortedSet<int>();

        // TODO a constructor to normal HLL algorithm
        // TODO construtor with sparse precision to use HLL++

        public HyperLogLog(int precision)
        {
            // TODO test precision

            Precision = precision;
            M = 1 << precision;
            Registers = new byte[M];
            AlphaMM = Alpha * M * M;

            LastBitsCount = Convert.ToByte(HASH_SIZE - Precision);
            LastBitsMask = (1UL << LastBitsCount) - 1;

            Format = HyperLogLogRepresentation.Normal;
        }

        /// <summary>
        /// Creates a hyperloglog++ instance.
        /// </summary>
        /// <param name="precision">The higher the precision, the higher the accuracy, but also the memory usage. Must be in [4, sparsePrecision].</param>
        /// <param name="sparsePrecision">The precision of the sparse representation. Must be inferior or equal to 64.</param>
        public HyperLogLog(int precision, int sparsePrecision)
        {
            if (precision < 4 || precision > sparsePrecision)
                throw new ArgumentException(string.Format("The precision {0} must be between 4 and {1} (sparse precision)", precision, sparsePrecision));
            if (sparsePrecision > 64)
                throw new ArgumentException(string.Format("The sparse precision {0} must be inferior or equal to 64.", sparsePrecision));

            Precision = precision; // Also called b in the paper
            SparsePrecision = sparsePrecision;
            M = 1 << precision; // b = log2(m), so m = 2^b
            AlphaMM = Alpha * M * M;

            LastBitsCount = Convert.ToByte(HASH_SIZE - Precision);
            LastBitsMask = (1UL << LastBitsCount) - 1;

            Format = HyperLogLogRepresentation.Sparse;
        }

        public bool Add(object value)
        {
            return AddHash(Hash(value));
        }

        /// <summary>
        /// Directly adds a hashed value.
        /// Can be useful if you don't want to use the default hash algorithm used here (Murmur3)
        /// </summary>
        /// <param name="hash">The hashed value</param>
        /// <returns>True if a register has been altered</returns>
        public bool AddHash(ulong hash)
        {
            switch (Format)
            {
                case HyperLogLogRepresentation.Normal:
                    ulong firstBits = hash >> LastBitsCount; // Gives the index between 0 and m
                    byte nbLeadingZeros = NumberOfLeadingZeros(hash & LastBitsMask);
                    return UpdateIfGreater(ref Registers[firstBits], nbLeadingZeros);

                case HyperLogLogRepresentation.Sparse:
                    int k = EncodeHash(hash);
                    bool added = TempSet.Add(k);
                    
                    if (TempSet.Count >= MaxTempSetSize) // TODO like in java implementation. Good ?
                    {
                        MergeTempSetToSparseList();

                        if (SparseSet.Count() > 0.75 * M) // TODO correct??? Paper says 6*m, java impl does m*0.75 ? + constant
                        {
                            ConvertToNormalRepresentation();
                        }
                        else if ((TempSet.Count * 2) < (SparseSet.Count / 4)) // temp set size grows proportionally to sparse set
                        {
                            MaxTempSetSize = SparseSet.Count / 4;
                        }

                        if (TempSet != null)
                            TempSet.Clear();
                    }
                    return added;

                default:
                    throw new Exception("Should not reach this code. Invalid hyperloglog representation");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hll"></param>
        /// <returns>True if at least one register has been altered</returns>
        public bool Merge(HyperLogLog hll)
        {
            // TODO
            return false;
        }

        /// <summary>
        /// Gets the cardinality of the set
        /// </summary>
        public ulong Cardinality
        {
            get
            {
                if (Format == HyperLogLogRepresentation.Sparse)
                {
                    // TODO method
                    MergeTempSetToSparseList();

                    if (SparseSet.Count() > 0.75 * M) // TODO correct??? Paper says 6*m, java impl does m*0.75 ? + constant
                    {
                        ConvertToNormalRepresentation();
                    }
                    else if ((TempSet.Count * 2) < (SparseSet.Count / 4)) // temp set size grows proportionally to sparse set
                    {
                        MaxTempSetSize = SparseSet.Count / 4; // TODO we care about increasing maxtempsize ?? Maybe for later adding
                    }

                    if (TempSet != null)
                        TempSet.Clear();
                }

                switch (Format)
                {
                    case HyperLogLogRepresentation.Sparse:
                        int sparseM = 1 << SparsePrecision;
                        return Convert.ToUInt64(LinearCounting(sparseM, sparseM - SparseSet.Count));

                    case HyperLogLogRepresentation.Normal:
                        // Sum all registers
                        double sum = 0D;
                        int emptyRegisters = 0;
                        RegistersMutex.WaitOne();
                        foreach (byte r in Registers)// TODO parallel ?
                        {
                            sum += 1.0 / (1 << r); // 2^-r = (2^r)^-1 = 1/(2^r)
                            if (r == 0)
                                ++emptyRegisters;
                        }
                        RegistersMutex.ReleaseMutex();

                        double estimate = AlphaMM * (1 / sum);
                        double estimatePrime = estimate;
                        if (estimate <= 5*M)
                            estimatePrime = estimate - EstimateBiasHelper.GetEstimateBias(estimate, Precision);

                        double H;
                        if (emptyRegisters > 0)
                            H = LinearCounting(M, emptyRegisters);
                        else
                            H = estimatePrime;

                        // When precision is larger, the threshold is just 5*m
                        if (((Precision <= 18) && (H < ThresholdData[Precision - 4])) || ((Precision > 18) && (estimate <= (5 * M))))
                        {
                            return Convert.ToUInt64(H);
                        }
                        else
                        {
                            return Convert.ToUInt64(estimatePrime);
                        }

                    default:
                        throw new Exception("Should not reach this code. Invalid hyperloglog representation");
                }


                /*
                // Sum all registers
                double sum = 0D;
                int emptyRegisters = 0;
                RegistersMutex.WaitOne();
                foreach (byte r in Registers)// TODO parallel ?
                {
                    sum += 1.0 / (1 << r); // 2^-r = (2^r)^-1 = 1/(2^r)
                    if (r == 0)
                        ++emptyRegisters;
                }
                RegistersMutex.ReleaseMutex();

                double estimate = AlphaMM * (1 / sum);

                double result;
                if (estimate <= 2.5 * M) // TODO Constant
                {
                    if (emptyRegisters != 0)
                        result = LinearCounting(M, emptyRegisters);
                    else
                        result = estimate;
                }
                else if (estimate <= 143165576.533) // TODO constant
                    result = estimate;
                else
                    result = -(Math.Pow(2, 32) * Math.Log(1 - (estimate/Math.Pow(2, 32))));

                return Convert.ToUInt64(result);*/
            }
        }

        private static double LinearCounting(int m, double v)
        {
            return m * Math.Log((double)m / v);
        }
        
        public static ulong Count<T>(IEnumerable<T> values, int precision = DEFAULT_PRECISION)
        {
        // TODO with sparse representation if small number of values
            HyperLogLog hll = new HyperLogLog(precision);
            // TODO parallel ?
            values.ToList().ForEach(v => hll.Add(v));

            return hll.Cardinality;
        }

        private double Alpha
        {
            get
            {
                switch (Precision)
                {
                    case 4:
                        return 0.673;
                    case 5:
                        return 0.697;
                    case 6:
                        return 0.709;
                    default:
                        return 0.7213 / (1D + 1.079 / M);
                }
            }
        }

        /// <summary>
        /// Returns the number of leading zeros of the last X bits of the hashed value.
        /// </summary>
        /// <param name="nb"></param>
        /// <returns>Number of leading zeros + 1 (see paper)</returns>
        private byte NumberOfLeadingZeros(ulong nb)
        {
            // Convert.ToString(long, 2) gives the binary reprensentation, beginning with the first 1 (leading zeros are trimmed like a usual integer)
            // Casting the ulong to long doesn't matter because the binary representation doesn't change
            // So the full length in bits - the length trimmed to the first 1 gives us the number of leading zeros (for the "+1" see paper)
            return Convert.ToByte(LastBitsCount - Convert.ToString((long)nb, 2).Length + 1);
            // TODO do not add +1 (method logic)
        }

        private byte NumberOfLeadingZeros(int nb)
        {
            return Convert.ToByte(32 - Convert.ToString(nb, 2).Length);
        }

        #region Encoding methods
        private int EncodeHash(ulong hash) // TODO verify is correct
        {
            // Index given by the hash with sparse precision
            int firstBits = (int)(hash >> (HASH_SIZE - SparsePrecision));
            // First bits beginning from 63 - precision
            // TODO if precision = sparce precision
            int zeroTest = 0;
            if (Precision < SparsePrecision) // TODO can it be ??
            {
                zeroTest = firstBits << ((32 - SparsePrecision) + Precision);
            }

            if (zeroTest == 0)
            {
                // Make room for a byte (the number of leading zeros)
                firstBits <<= 6;
                // Add them
                firstBits |= NumberOfLeadingZeros(hash & LastBitsMask); // TODO should I invert ?
                // Make room for one more
                firstBits <<= 1;
                // Put it to one
                firstBits |= 1;
                return firstBits;
            }

            // Else only add one space for the new zero at the end
            return firstBits << 1;
        }

        private void DecodeHash(int encodedHash, out int idx, out byte r)
        {
            // See encodedHash flag (if last bit is 0 or 1)
            if ((encodedHash & 1) == 1)
            {
                // r = (encodedHash from bit 9 to 1) + (sparsePrecision + precision)
                //r = (byte)((encodedHash & ((1 << 10) - 2)) + (SparsePrecision - Precision)); // TODO verify correctness
                // TODO how to have a value of 6 bits (instead of 8 (byte)) ? see how java impl does it
                r = (byte)((encodedHash >> 1) & 63);
                idx = encodedHash >> 7;
            }
            else
            {
                //r = NumberOfLeadingZeros(((ulong)(encodedHash << (Precision + (31 - SparsePrecision)))) & LastBitsMask);
                r = NumberOfLeadingZeros(encodedHash << (Precision + (31 - SparsePrecision)));
                r += 1;
                //r = NumberOfLeadingZeros((ulong)(encodedHash & ((1 << (SparsePrecision - Precision - 1)) - 1))); // TODO verify correctness
                idx = encodedHash >> 1;
            }
            idx >>= (SparsePrecision - Precision);
        }
        #endregion

        private void MergeTempSetToSparseList()
        {
        // TODO is correct ?
        // TODO parallel ?
            TempSet.ToList().ForEach(val => SparseSet.Add(val));
        }

        private void ConvertToNormalRepresentation()
        {
            Registers = new byte[M];

            foreach (int elem in SparseSet)
            {
                int idx;
                byte value;
                DecodeHash(elem, out idx, out value);
                Registers[idx] = Math.Max(Registers[idx], value);
            }
            Format = HyperLogLogRepresentation.Normal;

            TempSet = null;
            SparseSet = null;
            var a = Cardinality;
        }

        /// <summary>
        /// Updates the register value if the new value is greater.
        /// Returns the if the new value is greater than register.
        /// </summary>
        /// <param name="register"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        private bool UpdateIfGreater(ref byte register, byte newValue)
        {
            bool isGreater = true;

            RegistersMutex.WaitOne();
            if (register >= newValue)
                isGreater = false;
            else
                register = newValue;
            RegistersMutex.ReleaseMutex();

            return isGreater;
        }

        private static ulong Hash(object entry)
        {
            return MurmurHasher.ComputeHash(entry.ToByteArray()).GetUInt64(0);
        }
    }

    internal enum HyperLogLogRepresentation
    {
        Sparse,
        Normal
    }
}
