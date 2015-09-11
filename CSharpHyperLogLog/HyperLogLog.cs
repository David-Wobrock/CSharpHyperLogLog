﻿using CSharpHyperLogLog.Hash;
using CSharpHyperLogLog.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        // Accuracy of 1.04/sqrt(2^Precision) for now
        private readonly int Precision;
        private readonly int m;
        private int[] registers;
        private readonly double AlphaMM;
        private readonly int LastBitsCount;
        private readonly ulong LastBitsMask;

        /// <summary>
        /// Creates a hyperloglog instance.
        /// </summary>
        /// <param name="precision">Has a default value.</param>
        public HyperLogLog(int precision = DEFAULT_PRECISION)
        {
            Precision = precision; // Also called b in the paper
            m = 1 << precision; // b = log2(m), so m = 2^b
            registers = new int[m];
            AlphaMM = Alpha * m * m;

            LastBitsCount = HASH_SIZE - Precision;
            LastBitsMask = (1UL << LastBitsCount) - 1;
        }

        public bool Add(object value)
        {
            return AddHash(Hash(value));
        }

        public ulong Count
        {
            get
            {
                // Sum all registers
                double sum = 0;
                registers.ToList().ForEach(r => sum += 1.0 / (1 << r)); // TODO parallel ?

                double estimate = AlphaMM * (1 / sum);

                double result;
                if (estimate <= 2.5 * m)
                {
                    // Linear counting
                    int emptyRegisters = registers.Where(r => r == 0).Count();
                    result = (m * Math.Log((double)m / emptyRegisters));
                }
                else
                    result = estimate;

                return Convert.ToUInt64(result);
            }
        }

        /// <summary>
        /// Directly adds a hashed value.
        /// Can be useful if you don't want to use the default hash algorithm used here (Murmur3)
        /// </summary>
        /// <param name="hash">The hashed value</param>
        /// <returns>True if a register has been altered</returns>
        public bool AddHash(ulong hash)
        {
            ulong firstBits = hash >> LastBitsCount; // Is between 0 and m
            ulong lastBits = hash & LastBitsMask;

            int nbLeadingZeros = NumberOfLeadingZeros(lastBits);

            // Returns true if the register has been changed
            if (registers[firstBits] >= nbLeadingZeros)
                return false;
            registers[firstBits] = nbLeadingZeros;
            return true;
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
                        return 0.7213 / (1D + 1.079 / m);
                }
            }
        }

        /// <summary>
        /// Returns the number of leading zeros of the last X bits of the hashed value.
        /// </summary>
        /// <param name="nb"></param>
        /// <returns>Number of leading zeros + 1 (see paper)</returns>
        private int NumberOfLeadingZeros(ulong nb)
        {
            return LastBitsCount - Convert.ToString((long)nb, 2).Length + 1;
        }

        private static ulong Hash(object entry)
        {
            return MurmurHasher.ComputeHash(entry.ToByteArray()).GetUInt64(0);
        }
    }
}
