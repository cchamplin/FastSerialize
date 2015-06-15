using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    public class Serializer : ISerializer
    {
        private ISerializer _serializer;

        public Serializer(Type t)
        {
            _serializer = (ISerializer)TypeHelper.GetConstructor(t)();
        }
        public string Serialize(Object o)
        {
            return _serializer.Serialize(o);
        }
        public string Serialize(Object o, bool outputNulls)
        {
            return _serializer.Serialize(o, outputNulls);
        }

        public string Serialize(Object o, bool outputNulls, bool typeHints)
        {
            return _serializer.Serialize(o, outputNulls, typeHints);
        }
        /*public object Deserialize(String s)
        {
            return null;
        }*/
        public T Deserialize<T>(String s, bool @explicit = true)
        {
            return _serializer.Deserialize<T>(s, @explicit);
        }
        public T Deserialize<T>(Stream s,bool @explicit = true)
        {
            return _serializer.Deserialize<T>(s, @explicit);
        }
        public object Deserialize(Type t, String s, bool @explicit = true)
        {
            return _serializer.Deserialize(t, s, @explicit);
        }
        public object Deserialize(Type t, Stream s, bool @explicit = true)
        {
            return _serializer.Deserialize(t, s, @explicit);
        }
    }
}
