using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    internal interface ISerializer
    {
        string Serialize(Object o, bool outputNull = false, bool typeHints = true);

        // object Deserialize(String s);

        T Deserialize<T>(String s);
        T Deserialize<T>(Stream s);
    }
}
