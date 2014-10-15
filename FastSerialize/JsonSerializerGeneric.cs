using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    public class JsonSerializerGeneric : ISerializer
    {
        Dictionary<Type, TypeCache> typeCache;
        private StringBuilder bldr = new StringBuilder(32000);

        static sbyte[] ht =
        { -1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       , 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,-1,-1,-1,-1,-1,-1
       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,10,11,12,13,14,15,-1,-1,-1,-1,-1,-1,-1,-1,-1
       ,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1
        };

        internal JsonSerializerGeneric()
        {
            typeCache = new Dictionary<Type, TypeCache>();
        }
        public string Serialize(object o, bool outputNull = false)
        {
            StringBuilder result = new StringBuilder();
            SerializeObject(o, result, outputNull);
            return result.ToString();
        }
        private string EscapeString(String o)
        {
            o=o.Replace("\\","\\\\");
            o=o.Replace("\"", "\\\"");
            o=o.Replace("\t", "\\t");
            o=o.Replace("\r", "\\r");
            o=o.Replace("\n", "\\n");
            return o;

        }
        private void SerializeObject(object o, StringBuilder result, bool outputNull)
        {
            if (outputNull && o == null)
                result.Append("null");
            Type dataType = o.GetType();
            if (dataType.IsGenericType)
            {
                foreach (Type iType in dataType.GetInterfaces())
                {
                    if (iType == typeof(IList))
                    {
                        result.Append('[');
                        bool first = true;
                        foreach (var value in (IList)o)
                        {
                            if (!first)
                                result.Append(',');
                            first = false;
                            SerializeObject(value, result, outputNull);
                        }
                        result.Append(']');
                        break;
                    }
                    if (iType == typeof(IDictionary))
                    {
                        bool first = true;
                        result.Append('{');
                        foreach (DictionaryEntry value in ((IDictionary)o))
                        {
                            if (!first)
                                result.Append(',');
                            first = false;
                            result.Append('"');
                            result.Append(value.Key);
                            result.Append('"');
                            result.Append(':');
                            SerializeObject(value.Value, result, outputNull);
                        }
                        result.Append('}');
                        break;
                    }
                }
            }
            else if (dataType.IsArray)
            {
                result.Append('[');
                foreach (var x in (Array)o)
                {

                }
                result.Append(']');
            }
            else if (dataType.IsValueType)
            {
                if (dataType == typeof(bool))
                {
                    result.Append(o.ToString().ToLower());
                }
                else
                {
                    result.Append(o.ToString());
                }
            }
            else if (dataType == typeof(string))
            {
                result.Append('"');
                result.Append(EscapeString((string)o));
                result.Append('"');
            }
            else
            {
                result.Append('{');
                TypeCache cache = null;
                if (!typeCache.TryGetValue(dataType, out cache))
                {
                    cache = new TypeCache(dataType);
                    typeCache.Add(dataType, cache);
                }
                int count = 0;
                foreach (string key in cache.properties.Keys)
                {
                    object pObj = cache.properties[key].getter(o);
                    if (pObj != null)
                    {
                        result.Append('"');
                        result.Append(key);
                        result.Append('"');
                        result.Append(':');
                        SerializeObject(pObj, result, outputNull);
                        if (count < cache.properties.Keys.Count - 1)
                            result.Append(',');
                    }
                    else if (outputNull)
                    {
                        result.Append('"');
                        result.Append(key);
                        result.Append('"');
                        result.Append(':');
                        result.Append("null");
                        if (count < cache.properties.Keys.Count - 1)
                            result.Append(',');
                    }
                    count++;
                }
                result.Append('}');

            }

        }

        public T Deserialize<T>(Stream s)
        {
            return Deserialize<T>(s.AsEnumerableChar().GetEnumerator());
        }
        public T Deserialize<T>(string s)
        {
            return Deserialize<T>(s.GetEnumerator());
        }
        public T Deserialize<T>(IEnumerator s)
        {
            
            char c = ' ';
            while (s.MoveNext())
            {
                c = (char)s.Current;
                switch (c)
                {
                    case '{':
                        return ConsumeObject<T>(s);
                    case '[':
                        var listType = typeof(T);
                        Type objectType = null;
                        if (listType == typeof(Array))
                        {
                            break;
                        }
                        if (listType.IsGenericType)
                        {
                            foreach (var i in listType.GetInterfaces())
                            {
                                if (i == typeof(IList))
                                {
                                    objectType = listType.GetGenericArguments()[0];
                                    break;
                                }
                            }
                            //Generic Type
                        }
                        if (objectType != null)
                        {
                            IList list = (IList)TypeHelper.GetConstructor(listType)();
                            return (T)ConsumeArray(s, list, objectType);
                        }
                        throw new Exception("Cannot deserialize array into non-collection type");
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        continue;
                    default:
                        throw new Exception("Unexpected character");

                }
            }
            return default(T);
        }
        private object ConsumeArray(IEnumerator s, IList list, Type objectType)
        {
             s.MoveNext();
             char c = ' ';
             
             do
             {
                 c = (char)s.Current;
                 switch (c)
                 {
                     case ',':
                     case '\r':
                     case '\n':
                     case ' ':
                     case '\t':
                         continue;
                     case ']':
                         return list;
                     case '}':
                         return list;
                     default:

                         list.Add(ConsumeValue(s, objectType));
                         break;
                 }
             } while (s.MoveNext());
             throw new Exception("Unexpected character");
        }
        private int parseUnicode(char a, char b, char c, char d)
        {
            int decValue = ht[(byte)a];
            decValue = (decValue * 16) + ht[(byte)b];
            decValue = (decValue * 16) + ht[(byte)c];
            decValue = (decValue * 16) + ht[(byte)d];
            return decValue;
        }

        private object parseNumeric(IEnumerator s, bool single)
        {
            
            char c = ' ';
            bool fractal = false;
            bldr.Clear();

            do
            {
                c = (char)s.Current;
                switch (c)
                {
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                    case '+':
                        bldr.Append(c);
                        break;
                    case '.':
                    case 'e':
                    case 'E':
                        bldr.Append(c);
                        fractal = true;
                        break;
                    default:
                        goto parsed;
                }
            } while (s.MoveNext());
        parsed:
            if (fractal)
                if (single)
                    return float.Parse(bldr.ToString());
                else
                    return double.Parse(bldr.ToString());
            else
                return int.Parse(bldr.ToString());
        }
        private bool ParseBoolean(IEnumerator s)
        {
            bldr.Clear();
            char c = ' ';

            do
            {
                c = (char)s.Current;
                switch (c)
                {
                    case ',':
                    case '}':
                    case ']':
                        goto parsed;
                    default:
                        bldr.Append(c);
                        break;
                }
            } while (s.MoveNext());
        parsed:
            // TODO improve this
            try
            {
                return bool.Parse(bldr.ToString());
            }
            catch (Exception)
            {
                throw new Exception("Unable to parse boolean value");
            }
        }
        private string ParseString(IEnumerator s)
        {
            s.MoveNext();
            bldr.Clear();
            char c = ' ';

            do
            {
                c = (char)s.Current;
                if (c == '\\')
                {
                    if (!s.MoveNext())
                        throw new Exception("Invalid parse exception");
                    c = (char)s.Current;

                    switch (c)
                    {
                        case '"':
                            bldr.Append('"');
                            break;
                        case 'n':
                            bldr.Append('\n');
                            break;
                        case 'r':
                            bldr.Append('\r');
                            break;
                        case '\\':
                            bldr.Append('\\');
                            break;
                        case '/':
                            bldr.Append('/');
                            break;
                        case 't':
                            bldr.Append('\t');
                            break;
                        case 'b':
                            bldr.Append('\b');
                            break;
                        case 'f':
                            bldr.Append('\f');
                            break;
                        case 'u':
                            // TODO improve this
                            char u1,u2,u3,u4;
                            s.MoveNext();
                            u1 = (char)s.Current;
                            s.MoveNext();
                            u2 = (char)s.Current;
                            s.MoveNext();
                            u3 = (char)s.Current;
                            s.MoveNext();
                            u4 = (char)s.Current;
                            bldr.Append((char)parseUnicode(u1,u2,u3,u4));
                            break;
                    }
                }
                else if (c == '"')
                    goto parsed;
                else
                {
                    bldr.Append(c);
                } 
            } while (s.MoveNext());

            parsed:
            return bldr.ToString();
        }
        private string ParseNull(IEnumerator s)
        {
            // TODO Find a better way of doing this
            if ((char)s.Current == 'n' && s.MoveNext() 
                && (char)s.Current == 'u' && s.MoveNext()
               && (char)s.Current == 'l' && s.MoveNext()
              && (char)s.Current == 'l' && s.MoveNext())
            {
                return null;
            }
            throw new Exception("Unexpected character when reading value, expected null: " + s.Current);
        }
        private int ParseNullNumeric(IEnumerator s)
        {

            if ((char)s.Current == 'n' && s.MoveNext() 
                && (char)s.Current == 'u' && s.MoveNext()
               && (char)s.Current == 'l' && s.MoveNext()
              && (char)s.Current == 'l' && s.MoveNext())
            {
                return 0;
            }
            throw new Exception("Unexpected character when reading value, expected null: " + s.Current);
        }

        private T ConsumeObject<T>(IEnumerator s)
        {
            return (T)ConsumeObject(s, typeof(T));
        }
        private object ConsumeObject(IEnumerator s, Type instanceType)
        {
            s.MoveNext();

            TypeCache cache;
            if (!typeCache.TryGetValue(instanceType, out cache))
            {
                cache = new TypeCache(instanceType);
                typeCache.Add(instanceType, cache);
            }
            Object instance = cache.constructor();
            char c = ' ';
            do
            {
                c = (char)s.Current;
                switch (c)
                {
                    case '"':
                        string propertyName = ParseString(s);
                        PropertyAccessor accessor;
                        if (!cache.properties.TryGetValue(propertyName, out accessor))
                        {
                            foreach (var property in cache.properties)
                            {
                                if (property.Value.isSpecial)
                                {
                                    if (property.Value.prefixMatch != null)
                                    {
                                        if (propertyName.StartsWith(property.Value.prefixMatch))
                                        {
                                            accessor = property.Value;
                                        }
                                    }
                                    else if (property.Value.suffixMatch != null)
                                    {
                                        if (propertyName.EndsWith(property.Value.suffixMatch))
                                        {
                                            accessor = property.Value;
                                        }
                                    }
                                    else if (property.Value.ignoreCase != null)
                                    {
                                        if (propertyName.ToLower() == property.Key.ToLower())
                                        {
                                            accessor = property.Value;
                                        }
                                    }
                                }
                            }
                            if (accessor == null)
                                throw new Exception("No such field: " + propertyName);
                        }
                        s.MoveNext();
                        if (!ConsumeWhiteSpace(s))
                            throw new Exception("Parse Error");
                        if ((char)s.Current != ':')
                            throw new Exception("Parse Error");
                        s.MoveNext();
                        if (!ConsumeWhiteSpace(s))
                            throw new Exception("Parse Error");
                        


                         if (accessor.isDictionary)
                        {
                            IDictionary dict = (IDictionary)accessor.getter(instance);
                            if (dict == null)
                                dict = (IDictionary)TypeHelper.GetConstructor(accessor.type)();
                            accessor.setter(instance, ConsumeIntoDictionary(s, dict, accessor.genericType));
                            break;
                        }
                        accessor.setter(instance, ConsumeValue(s, accessor.type));
                        break;
                    case ',':
                        continue;
                    case '}':
                        return instance;
                }
            } while (s.MoveNext());

            return instance;
        }
        private object ConsumeObjectIntoDictionary(IEnumerator s, IDictionary dict, Type instanceType)
        {
            s.MoveNext();
            char c = ' ';
            do
            {

                c = (char)s.Current;
                switch (c)
                {
                    case '"':
                        string propertyName = ParseString(s);
                        /*PropertyAccessor accessor;
                        if (!cache.properties.TryGetValue(propertyName, out accessor))
                        {
                            throw new Exception("No such field");
                        }*/
                        s.MoveNext();
                        if (!ConsumeWhiteSpace(s))
                            throw new Exception("Parse Error");

                        if ((char)s.Current != ':')
                            throw new Exception("Parse Error");
                        s.MoveNext();
                        if (!ConsumeWhiteSpace(s))
                            throw new Exception("Parse Error");
                        // TODO nested dictionaries.
                        /*if (instanceType.isDictionary)
                        {
                            accessor.setter(instance, ConsumeIntoDictionary(s, ref index, accessor.type, accessor.genericType));
                            break;
                        }*/
                        //accessor.setter(instance, );
                        dict.Add(propertyName, ConsumeValue(s, instanceType));
                        break;
                    case ',':
                        continue;
                    case '}':
                        return dict;
                }
            } while (s.MoveNext());

            return dict;
        }

        private T ConsumeValue<T>(IEnumerator s)
        {
            return (T)ConsumeValue(s, typeof(T));
        }
        private object ConsumeValue(IEnumerator s, Type type)
        {
            char c = ' ';

            do
            {

                c = (char)s.Current;
                switch (c)
                {
                    case '"':
                        if (type == typeof(Guid))
                        {
                            return new Guid(ParseString(s));
                        }
                        else if (type == typeof(Guid?))
                        {
                            return new Guid(ParseString(s));
                        }
                        if (type != typeof(string) && type != typeof(object))
                            throw new Exception("Invalid field value type");
                        //return (object)ParseString(s, ref index);
                        return (object)ParseString(s);
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '.':
                    case '-':
                    case '+':
                    case 'e':
                    case 'E':
                        if (type == typeof(Single)
                            || type == typeof(float))
                        {
                            return (object)parseNumeric(s, true);
                        }
                        if (type != typeof(object)
                            && type != typeof(int)
                            && type != typeof(long)
                            && type != typeof(double)
                            && type != typeof(short)
                            )
                            throw new Exception("Invalid field value type");
                        return (object)parseNumeric(s,false);
                    case '[':
                        var listType = type;
                        Type objectType = null;
                        if (listType == typeof(Array) || listType.IsArray == true)
                        {
                            // Todo figure out a way to handle arrays
                            // Possible solution at the cost of performance would be to look ahead
                            // and count the number of items.
                            // Alternatively we could initialize a collection type and then call ToArray();
                            throw new Exception("Cannot deserialize array into non-collection type");
                        }
                        if (listType.IsGenericType)
                        {
                            foreach (var i in listType.GetInterfaces())
                            {
                                if (i == typeof(IList))
                                {
                                    objectType = listType.GetGenericArguments()[0];
                                    break;
                                }
                            }
                        }
                        if (objectType != null)
                        {
                            IList list = (IList)TypeHelper.GetConstructor(listType)();
                            return ConsumeArray(s, list, objectType);
                        }
                        throw new Exception("Cannot deserialize array into non-collection type");
                    case '{':
                        //return ConsumeObject(s, ref index, type);
                        return ConsumeObject(s, type);
                    default:
                        if (type == typeof(bool))
                        {
                            return ParseBoolean(s);
                        }
                        if (type == typeof(int)
                            || type == typeof(long)
                            || type == typeof(float)
                            || type == typeof(double)
                            || type == typeof(short)
                            || type == typeof(Single))
                        {
                            return ParseNullNumeric(s);
                        }

                        else if (IsNullableType(type))
                        {
                            return ParseNull(s);
                        }
                        else throw new Exception("Unexpected character when reading value");

                }
            } while (s.MoveNext());
            return null;
        }
        private object ConsumeIntoDictionary(IEnumerator s, IDictionary dict, Type type)
        {

            char c;
            do
            {
                c = (char)s.Current;
                switch (c)
                {

                    case '{':
                        return ConsumeObjectIntoDictionary(s, dict, type);
                    default:
                        throw new Exception("Unexpected character when reading dictionary value: " + c);
                }
            } while (s.MoveNext());
            return null;
        }
        private bool IsNullableType(Type type)
        {
            return (!type.IsValueType || (type.IsGenericType &&
            type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }

        public bool ConsumeWhiteSpace(IEnumerator s)
        {
            
            do
            {
                switch ((char)s.Current)
                {
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        continue;
                    default:
                        return true;
                }
            } while (s.MoveNext());
            return false;
        }
    }
}
