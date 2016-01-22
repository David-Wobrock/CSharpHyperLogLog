﻿using CSharpHyperLogLog.Hash;
using CSharpHyperLogLog.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CSharpHyperLogLog
{
    /// <summary>
    /// Based on Philippe Flajolet's algorithm : http://algo.inria.fr/flajolet/Publications/FlFuGaMe07.pdf
    /// Improvements based on Google engineer's publication : http://static.googleusercontent.com/media/research.google.com/en//pubs/archive/40671.pdf
    /// And the appendix to this publication : http://goo.gl/iU8Ig
    /// Helped and inspired by : https://github.com/addthis/stream-lib HyperLogLog and HyperLogLogPlus implementations
    /// </summary>
    public class HyperLogLog_Old
    {
        private static readonly IHasher Hasher = new Murmur3();
        private readonly HashEncodingHelper HashEncoder;
        private static readonly double[] ThresholdData = new double[] { 10, 20, 40, 80, 220, 400, 900, 1800, 3100, 6500, 11500, 20000, 50000, 120000, 350000 };
        private const int MIN_PRECISION = 4;

        // Accuracy of 1.04/sqrt(2^Precision) for now.
        private readonly int Precision;
        private readonly int M;
        // We use the "byte" type because the size of each register must be log2(log2(N)), when cardinalities <= N.
        // Since we use hashed values on 64 bits, N = 2^64. So, log2(log2(2^64)) = 6.
        private byte[] Registers;
        private readonly Mutex RegistersMutex = new Mutex();
        private readonly double AlphaMM;
        private HyperLogLogRepresentation Format;

        // Sparse representation attributes
        private readonly int SparsePrecision;
        private readonly int SparseRepresentationThreshold;
        private readonly int TempSetThreshold;
        private ISet<int> TempSet;
        private ISet<int> SparseSet;

        /// <summary>
        /// Constructor for the basic HyperLogLog algorithm by Philippe Flajolet. The one and only.
        /// It will only use dense representation and may be less accurate and use more memory for small cardinalities.
        /// It can be used to make tests and compare this version to the improved version.
        /// </summary>
        /// <param name="precision">
        /// Determines the number of registers. The higher the precision, the more accurate the algorithm is, but also the most memory it uses.
        /// The precision must be between 4 and 28.</param>
        public HyperLogLog_Old(int precision)
        {
            if (precision < 0)
                throw new ArgumentException("The precision cannot be negative.");
            if (precision < MIN_PRECISION)
                throw new ArgumentException("A precision below 4 is useless. You won't be able to estimate any collections.");
            if (precision > 28)
                throw new ArgumentException("A precision above 28 will use too much memory (~500 MegaBytes).");
                
            Precision = precision;
            M = 1 << precision;
            Registers = new byte[M];
            AlphaMM = Alpha * M * M;

            Format = HyperLogLogRepresentation.Normal;
        }

        /// <summary>
        /// Creates a hyperloglog++ instance.
        /// </summary>
        /// <param name="precision">The higher the precision, the higher the accuracy, but also the memory usage. Must be in [4, max(28, sparsePrecision)].</param>
        /// <param name="sparsePrecision">The precision of the sparse representation. Must be inferior or equal to 64.</param>
        public HyperLogLog_Old(int precision, int sparsePrecision)
        {
            int precisionLimit = Math.Min(sparsePrecision, 28);
            if (precision < MIN_PRECISION || precision > precisionLimit)
                throw new ArgumentException(string.Format("The precision {0} must be between 4 and {1}", precision, precisionLimit));
            if (sparsePrecision > 64)
                throw new ArgumentException(string.Format("The sparse precision {0} must be inferior or equal to 64.", sparsePrecision));

            Precision = precision;
            M = 1 << precision;
            AlphaMM = Alpha * M * M;

            SparseRepresentationThreshold = Convert.ToInt32(0.75 * M); // TODO 6*M or 0.75*M?
            TempSetThreshold = Convert.ToInt32(0.25 * SparseRepresentationThreshold);

            SparsePrecision = sparsePrecision;
            TempSet = new SortedSet<int>();
            SparseSet = new SortedSet<int>();

            HashEncoder = new HashEncodingHelper(precision, sparsePrecision);

            Format = HyperLogLogRepresentation.Sparse;
        }

        /// <summary>
        /// Adds any object to the HyperLogLog instance.
        /// </summary>
        /// <param name="value">Any object that will be hashed</param>
        /// <returns>True if a register has been altered</returns>
        public bool Add(object value)
        {
            return AddHash(Hasher.Hash(value));
        }

        /// <summary>
        /// Directly adds a hashed value.
        /// Can be useful if you don't want to use the default hash algorithm used here (Murmur3)
        /// </summary>
        /// <param name="hash">The 64 bit hashed value</param>
        /// <returns>True if a register has been altered</returns>
        public bool AddHash(ulong hash)
        {
            switch (Format)
            {
                case HyperLogLogRepresentation.Normal:
                    ulong firstBits = hash.ExtractBits(64 - Precision, 64); // Gives the index between 0 and M
                    byte nbLeadingZeros = hash.ExtractBits(0, 64 - Precision).NumberOfLeadingZeros(64 - Precision);
                    return UpdateIfGreater(ref Registers[firstBits], nbLeadingZeros);

                case HyperLogLogRepresentation.Sparse:
                    int k = HashEncoder.EncodeHash(hash);
                    return AddEncodedHash(k);

                default:
                    throw new Exception("Should not reach this code. Invalid hyperloglog representation");
            }
        }

        private bool AddEncodedHash(int encodedHash)
        {
            bool added = TempSet.Add(encodedHash);

            if (TempSet.Count() >= TempSetThreshold) // TODO like in java implementation. Good ?
            {
                MergeTempSetToSparseList();
                if (SparseSet.Count() > SparseRepresentationThreshold)
                    ToNormalRepresentation();
            }

            return added;
        }

        /// <summary>
        /// Fusions two hyperloglog instances that actual have the same representation.
        /// The precision (and sparse precision if sparse reprensentation) must have the same value. Otherwise an ArgumentException is thrown.
        /// </summary>
        /// <param name="hll"></param>
        /// <returns>True if at least one register has been altered</returns>
        public bool Merge(HyperLogLog_Old hll)
        {
            // TODO : tests + comments

            if (Format != hll.Format)
                throw new ArgumentException("Both hyperloglog instances must have the same representation.");
            if (Precision != hll.Precision)
                throw new ArgumentException("Both hyperloglog instances must have the same precision");
            if (Format == HyperLogLogRepresentation.Sparse && SparsePrecision != hll.SparsePrecision)
                throw new ArgumentException("Both hyperloglog instances must have the same sparse precision");

            bool modified = false;

            switch (Format)
            {
                case HyperLogLogRepresentation.Normal:
                    for (int i = 0; i < hll.Registers.Count(); ++i)
                        if (UpdateIfGreater(ref Registers[i], hll.Registers[i]))
                            modified = true;
                    break;

                case HyperLogLogRepresentation.Sparse:
                    hll.MergeTempSetToSparseList();
                    foreach (int encodedVal in hll.SparseSet)
                        if (AddEncodedHash(encodedVal))
                            modified = true;
                    break;
            }
            return modified;
        }

        /// <summary>
        /// Gets the cardinality of the set
        /// </summary>
        public ulong Cardinality
        {
            get
            {
                switch (Format)
                {
                    case HyperLogLogRepresentation.Sparse:
                        if (SparseSet.Count == 1)
                            System.Diagnostics.Debugger.Break();
                        MergeTempSetToSparseList();
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
                        double estimatePrime = estimate <= 5 * M ? estimate - EstimateBiasHelper.GetEstimateBias(estimate, Precision) : estimate;
                        
                        double H;
                        if (emptyRegisters != 0)
                            H = LinearCounting(M, emptyRegisters);
                        else
                            H = estimatePrime;


                        double result;
                        // If precision is above 18, no threshold data is furnished by google. So we use the threshold 5*m
                        if (((Precision <= 18) && (H < ThresholdData[Precision - MIN_PRECISION]))
                                || ((Precision > 18) && (estimate <= (5 * M))))
                            result = H;
                        else
                            result = estimatePrime;

                        return Convert.ToUInt64(result);

                    default:
                        throw new Exception("Should not reach this code. Invalid hyperloglog representation");
                }
            }
        }

        private static double LinearCounting(int m, double v)
        {
            return m * Math.Log(m / v);
        }
        
        public static ulong Count<T>(IEnumerable<T> values, int precision)
        {
        // TODO with sparse representation if small number of values
            HyperLogLog_Old hll = new HyperLogLog_Old(precision);
            // TODO parallel ?
            values.ToList().ForEach(v => hll.Add(v));

            return hll.Cardinality;
        }

        public static ulong Count<T>(IEnumerable<T> values, int precision, int sparsePrecision)
        {
            // TODO with sparse representation if small number of values
            HyperLogLog_Old hll = new HyperLogLog_Old(precision, sparsePrecision);
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

        private void MergeTempSetToSparseList()
        {
            // TODO is correct ?
            // TODO parallel ?
            foreach (int val in TempSet)
                SparseSet.Add(val);

            TempSet.Clear();
        }

        /// <summary>
        /// Converts the current sparse representation to the normal (dense) representation.
        /// </summary>
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

            TempSet = null;
            SparseSet = null;
            Format = HyperLogLogRepresentation.Normal;
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
    }

    internal enum HyperLogLogRepresentation
    {
        Sparse,
        Normal
    }
}
