using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CSharpHyperLogLog_Tests")]
namespace CSharpHyperLogLog.Utils
{
    /// <summary>
    /// Partially based on http://blog.teamleadnet.com/2012/08/murmurhash3-ultra-fast-hash-algorithm.html
    /// </summary>
    internal static class IntExtensions
    {
        public const byte LONG_SIZE = 64;
        public const byte INT_SIZE = 32;

        public static ulong RotateLeft(this ulong original, int bits)
        {
            return (original << bits) | (original >> (LONG_SIZE - bits));
        }

        public static ulong RotateRight(this ulong original, int bits)
        {
            return (original >> bits) | (original << (LONG_SIZE - bits));
        }

        // Uses BitConverter so we don't use unsafe compilation (which might lead to some very light performance loss)
        public static ulong GetUInt64(this byte[] bb, int pos)
        {
            return BitConverter.ToUInt64(bb, pos);
        }

        /// <summary>
        /// Extracts bits from an unsigned 64 bits integer.
        /// The indices have to be chosen like this :
        /// Value :     001011011111110111...000001110000111110‬
        /// Indices :   63...                          ...43210
        /// The ending index must be greater than the beginning one.
        /// The bit at the beginning index is included.
        /// The bit at the ending index is excluded !
        /// </summary>
        public static ulong ExtractBits(this ulong value, int beginningIdx, int endingIdx)
        {
            if (beginningIdx < 0)
                throw new ArgumentException("Beginning index must be positive or zero.");
            if (endingIdx <= beginningIdx)
                throw new ArgumentException("The ending index must be greater than the beginning one.");
            if (endingIdx > LONG_SIZE)
                endingIdx = LONG_SIZE;

            return (value >> beginningIdx) & ((1UL << (endingIdx - beginningIdx)) - 1);
        }

        /// <summary>
        /// Extracts bits from an unsigned 64 bits integer.
        /// The indices have to be chosen like this :
        /// Value :     001011011111110111...000001110000111110‬
        /// Indices :   31...                          ...43210
        /// The ending index must be greater than the beginning one.
        /// The bit at the beginning index is included.
        /// The bit at the ending index is excluded !
        /// </summary>
        public static int ExtractBits(this int value, int beginningIdx, int endingIdx)
        {
            if (beginningIdx < 0)
                throw new ArgumentException("Beginning index must be positive or zero.");
            /*if (endingIdx == beginningIdx)
                return 0;*/
            if (endingIdx <= beginningIdx)
                throw new ArgumentException("The ending index must be greater than the beginning one.");
            if (endingIdx > INT_SIZE)
                endingIdx = INT_SIZE;

            return (value >> beginningIdx) & ((1 << (endingIdx - beginningIdx)) - 1);
        }

        /// <summary>
        /// Returns the number of leading zeros of value PLUS ONE, considering maxSize is the number of bits nb can have.
        /// </summary>
        /// <param name="nb">The value of which we will count the number of leading zeros.</param>
        /// <param name="maxSize">Maximal number of bits nb can be composed of. Cannot exceed 64, nor be negative.</param>
        /// <returns>Number of leading zeros + 1 (see paper)</returns>
        public static byte NumberOfLeadingZeros(this ulong nb, int maxSize = LONG_SIZE)
        {
            if (maxSize < 0)
                throw new ArgumentException("Maximal size of number cannot be negative.");
            if (maxSize > LONG_SIZE)
                maxSize = LONG_SIZE;

            // Convert.ToString(long, 2) gives the binary reprensentation, beginning with the first 1 (leading zeros are trimmed like a usual integer)
            // Casting the ulong to long doesn't matter because the binary representation doesn't change
            // So the full length in bits - the length trimmed to the first 1 gives us the number of leading zeros (for the "+1" see paper)
            return Convert.ToByte(maxSize - Convert.ToString((long)nb, 2).Length + 1);
        }

        /// <summary>
        /// Returns the number of leading zeros of value PLUS ONE, considering maxSize is the number of bits nb can have.
        /// </summary>
        /// <param name="nb">The value of which we will count the number of leading zeros.</param>
        /// <param name="maxSize">Maximal number of bits nb can be composed of. Cannot exceed 32, nor be negative.</param>
        /// <returns>Number of leading zeros + 1 (see paper)</returns>
        public static byte NumberOfLeadingZeros(this int nb, int maxSize = INT_SIZE)
        {
            if (maxSize < 0)
                throw new ArgumentException("Maximal size of number cannot be negative.");
            if (maxSize > INT_SIZE)
                maxSize = INT_SIZE;

            return Convert.ToByte(maxSize - Convert.ToString(nb, 2).Length + 1);
        }
    }
}
