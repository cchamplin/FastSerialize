﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    public class Serializer
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
        /*public object Deserialize(String s)
        {
            return null;
        }*/
        public T Deserialize<T>(String s)
        {
            return _serializer.Deserialize<T>(s);
        }
        public T Deserialize<T>(Stream s)
        {
            return _serializer.Deserialize<T>(s);
        }
    }
}