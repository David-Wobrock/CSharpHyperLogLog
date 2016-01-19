using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CSharpHyperLogLog.Utils
{
    internal static class ObjectExtensions
    {
        /// <summary>
        /// Converts any object to a byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>A byte array</returns>
        public static byte[] ToByteArray(this object obj)
        {
            if (obj == null)
                throw new ArgumentException("Caller object is null. Cannot hash null.");

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
    }
}
