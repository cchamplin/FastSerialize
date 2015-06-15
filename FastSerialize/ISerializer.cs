using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    public interface ISerializer
    {
        string Serialize(Object o, bool outputNull = false, bool typeHints = true);

        // object Deserialize(String s);

        T Deserialize<T>(String s, bool @explicit = true);
        T Deserialize<T>(Stream s, bool @explicit = true);
        object Deserialize(Type t, Stream s, bool @explicit = true);
        object Deserialize(Type t, string s, bool @explicit = true);
    }
}
