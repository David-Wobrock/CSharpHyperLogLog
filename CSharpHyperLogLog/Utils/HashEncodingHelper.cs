using System;

namespace CSharpHyperLogLog.Utils
{
    internal class HashEncodingHelper
    {
        private readonly int Precision;
        private readonly int SparsePrecision;

        private readonly int PrecisionRemainder;
        private readonly int SparsePrecisionRemainder;

        private readonly int PrecisionDifference;

        internal HashEncodingHelper(int precision, int sparsePrecision)
        {
            Precision = precision;
            SparsePrecision = sparsePrecision;

            PrecisionRemainder = IntExtensions.LONG_SIZE - Precision;
            SparsePrecisionRemainder = IntExtensions.LONG_SIZE - sparsePrecision;

            PrecisionDifference = sparsePrecision - precision;
        }

        /// <summary>
        /// Encodes the hash value. See specifications in Google HLL++ paper.
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="precision"></param>
        /// <param name="sparsePrecision"></param>
        /// <returns></returns>
        public int EncodeHash(ulong hash)
        {
            int sparseIndex = (int)hash.ExtractBits(SparsePrecisionRemainder, IntExtensions.LONG_SIZE);

            if (hash.ExtractBits(SparsePrecisionRemainder, PrecisionRemainder) == 0)
            {
                // SparseValue : Add number of leading zeros (fits into 6 bits, because the highest value of leading zeros can be 63 max)
                sparseIndex <<= 6;
                sparseIndex |= hash.ExtractBits(0, SparsePrecisionRemainder).NumberOfLeadingZeros(SparsePrecisionRemainder);

                // Add a 1 at the end (same as result * 2 + 1)
                sparseIndex <<= 1;
                sparseIndex |= 1;

            }
            else // Or add a 0 at the end
                sparseIndex <<= 1;

            return sparseIndex;
        }

        /// <summary>
        /// Encodes the hash value. See specifications in Google HLL++ paper.
        /// </summary>
        /// <param name="encodedHash"></param>
        /// <param name="idx"></param>
        /// <param name="r"></param>
        public void DecodeHash(int encodedHash, out int idx, out byte r)
        {
            if (encodedHash.ExtractBits(0, 1) == 1)
            {
                r = Convert.ToByte((encodedHash >> 1) & 63);
                idx = encodedHash.ExtractBits(6, Precision + 6);
            }
            else
            {
                r = encodedHash.ExtractBits(1, PrecisionDifference).NumberOfLeadingZeros(PrecisionDifference - 1);
                idx = encodedHash.ExtractBits(1, Precision + 1);
            }
        }
    }
}
