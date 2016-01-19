using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpHyperLogLog.Hash
{
    internal interface IHasher
    {
        ulong Hash(object entry);
    }
}
