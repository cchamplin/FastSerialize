using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    public class JsonSerializerString : ISerializer
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

        internal JsonSerializerString()
        {
            typeCache = new Dictionary<Type, TypeCache>();
        }
        public string Serialize(object o, bool outputNull = false, bool typeHints = true)
        {
            StringBuilder result = new StringBuilder();
            SerializeObject(o, result, outputNull, typeHints);
            return result.ToString();
        }
        private string EscapeString(String o)
        {
            o = o.Replace("\\", "\\\\");
            o = o.Replace("\"", "\\\"");
            o = o.Replace("\t", "\\t");
            o = o.Replace("\r", "\\r");
            o = o.Replace("\n", "\\n");
            return o;

        }
        private void SerializeObject(object o, StringBuilder result, bool outputNull, bool typeHints)
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
                            SerializeObject(value,result, outputNull, typeHints);
                        }
                        result.Append(']');
                        result.Append("\r\n");
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
                            SerializeObject(value.Value, result, outputNull, typeHints);
                        }
                        result[result.Length] = '}';
                        result.Append("\r\n");
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
                        SerializeObject(pObj, result, outputNull, typeHints);
                        if (count < cache.properties.Keys.Count - 1)
                        {
                            result.Append(',');
                            result.Append("\r\n");
                        }
                    }
                    else if (outputNull) {
                        result.Append('"');
                        result.Append(key);
                        result.Append('"');
                        result.Append(':');
                        result.Append("null");
                        if (count < cache.properties.Keys.Count - 1)
                        {

                            result.Append(',');
                            result.Append("\r\n");
                        }
                    }
                    count++;
                }
                result.Append('}');
                result.Append("\r\n");
            }
        }

        public T Deserialize<T>(Stream s,bool @explicit = true)
        {
            throw new NotImplementedException();
        }
        public T Deserialize<T>(string s, bool @explicit = true)
        {

            char c;
            for (int x = 0; x < s.Length; x++)
            {
                c = s[x];
                switch (c)
                {
                    case '{':
                        return ConsumeObject<T>(s, ref x);
                    case '[':
                        var listType = typeof(T);
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
                            // TODO look into handling other types of collections
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
                            return (T)ConsumeArray(s, list, objectType, ref x);
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
        private object ConsumeArray(string s, IList list, Type objectType, ref int index)
        {
            index++;
            char c;

            for (; index < s.Length; index++)
            {
                c = s[index];
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
                    // TODO check this logic and flow
                    case '}':
                        return list;
                    default:
                        list.Add(ConsumeValue(s, ref index, objectType));
                        if (s[index] == ']')
                            index--;
                        break;
                }
            }
            return list;
            //throw new Exception("Unexpected character");
        }

        

        private int parseUnicode(char a, char b, char c, char d)
        {
            int decValue = ht[(byte)a];
            decValue = (decValue * 16) + ht[(byte)b];
            decValue = (decValue * 16) + ht[(byte)c];
            decValue = (decValue * 16) + ht[(byte)d];
            return decValue;
        }

        private object parseNumeric(string s, ref int index, bool single)
        {
            char c;
            bool fractal = false;
            bldr.Clear();
            for (; index < s.Length; index++)
            {
                c = s[index];
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
            }
        parsed:
            if (fractal)
                 if (single)
                    return float.Parse(bldr.ToString());
                else
                    return double.Parse(bldr.ToString());
            else
                return int.Parse(bldr.ToString());
        }
        private bool ParseBoolean(string s, ref int index)
        {
            // TODO improve this
            char c;
            bldr.Clear();
            for (; index < s.Length; index++)
            {
                c = s[index];
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
            }
        parsed:
            try
            {
                return bool.Parse(bldr.ToString());
            }
            catch (Exception)
            {
                throw new Exception("Unable to parse boolean value");
            }
        }

        private string ParseString(string s, ref int index)
        {
            index++;
            bldr.Clear();
            char c;
            for (; index < s.Length; index++)
            {
                c = s[index];
                if (c == '\\')
                {
                    if (++index == s.Length)
                        throw new Exception("Invalid parse exception");

                    c = s[index];
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
                            bldr.Append((char)parseUnicode(s[++index],s[++index],s[++index],s[++index]));
                            break;
                    }
                }
                else if (c == '"')
                    goto parsed;
                else
                    bldr.Append(c);
            }

        parsed:
            return bldr.ToString();
        }
        private string ParseNull(string s, ref int index)
        {

            if (s[index++] == 'n' && s[index++] == 'u' && s[index++] == 'l' && s[index++] == 'l')
            {
                return null;
            }
            throw new Exception("Unexpected character when reading value, expected null: " + s[index]);
        }
        private int ParseNullNumeric(string s, ref int index)
        {

            if (s[index++] == 'n' && s[index++] == 'u' && s[index++] == 'l' && s[index++] == 'l')
            {
                return 0;
            }
            throw new Exception("Unexpected character when reading value, expected null: " + s[index]);
        }
        private T ConsumeObject<T>(string s, ref int index)
        {
            return (T)ConsumeObject(s, typeof(T), ref index);
        }

        private object ConsumeObject(string s, Type instanceType, ref int index)
        {
            index++;
            TypeCache cache;
            if (!typeCache.TryGetValue(instanceType, out cache))
            {
                cache = new TypeCache(instanceType);
                typeCache.Add(instanceType, cache);
            }
            Object instance = cache.constructor();
            char c;
            for (; index < s.Length; index++)
            {

                c = s[index];
                switch (c)
                {
                    case '"':
                        string propertyName = ParseString(s, ref index);
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
                        ++index;
                        if (!ConsumeWhiteSpace(s, ref index))
                            throw new Exception("Parse Error");
                        if (s[index++] != ':')
                            throw new Exception("Parse Error");
                        if (!ConsumeWhiteSpace(s, ref index))
                            throw new Exception("Parse Error");
                        if (accessor.isDictionary)
                        {
                            IDictionary dict = (IDictionary)accessor.getter(instance);
                            if (dict == null)
                                dict = (IDictionary)TypeHelper.GetConstructor(accessor.type)();
                            accessor.setter(instance, ConsumeIntoDictionary(s, ref index, dict, accessor.genericType));
                            break;
                        }
                        accessor.setter(instance, ConsumeValue(s, ref index, accessor.type));
                        if (s[index] == '}')
                            index--;
                        break;
                    case ',':
                        continue;
                    case '}':
                        index++;
                        return instance;

                }
            }

            return instance;
        }
        private object ConsumeObjectIntoDictionary(string s, IDictionary dict, Type instanceType, ref int index)
        {
            index++;
            char c;
            for (; index < s.Length; index++)
            {

                c = s[index];
                switch (c)
                {
                    case '"':
                        string propertyName = ParseString(s, ref index);
                        /*PropertyAccessor accessor;
                        if (!cache.properties.TryGetValue(propertyName, out accessor))
                        {
                            throw new Exception("No such field");
                        }*/
                        ++index;
                        if (!ConsumeWhiteSpace(s, ref index))
                            throw new Exception("Parse Error");
                        if (s[index++] != ':')
                            throw new Exception("Parse Error");
                        if (!ConsumeWhiteSpace(s, ref index))
                            throw new Exception("Parse Error");
                        // TODO nested dictionaries.
                        /*if (instanceType.isDictionary)
                        {
                            accessor.setter(instance, ConsumeIntoDictionary(s, ref index, accessor.type, accessor.genericType));
                            break;
                        }*/
                        //accessor.setter(instance, );
                        dict.Add(propertyName, ConsumeValue(s, ref index, instanceType));
                        break;
                    case ',':
                        continue;
                    case '}':
                        return dict;
                }
            }

            return dict;
        }
        private T ConsumeValue<T>(string s, ref int index)
        {
            return (T)ConsumeValue(s, ref index, typeof(T));
        }
        private object ConsumeValue(string s, ref int index, Type type)
        {
            char c;
            for (; index < s.Length; index++)
            {
                c = s[index];
                switch (c)
                {
                    case '"':
                        if (type == typeof(Guid))
                        {
                            return new Guid(ParseString(s, ref index));
                        }
                        else if (type == typeof(Guid?))
                        {
                            return new Guid(ParseString(s, ref index));
                        }
                        if (type != typeof(string) && type != typeof(object))
                            throw new Exception("Invalid field value type");
                        return (object)ParseString(s, ref index);
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
                            return (object)parseNumeric(s, ref index, true);
                        }
                        if (type != typeof(object)
                            && type != typeof(int)
                            && type != typeof(long)
                            && type != typeof(double)
                            && type != typeof(short))
                            throw new Exception("Invalid field value type");
                        return (object)parseNumeric(s, ref index, false);
                    case '[':
                        var listType = type;
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
                        }
                        if (objectType != null)
                        {
                            IList list = (IList)TypeHelper.GetConstructor(listType)();
                            return ConsumeArray(s, list, objectType, ref index);
                        }
                        throw new Exception("Cannot deserialize array into non-collection type");
                    case '{':
                        return ConsumeObject(s, type, ref index);
                    default:
                        if (type == typeof(bool))
                        {
                            return ParseBoolean(s, ref index);
                        }
                        if (type == typeof(int)
                            || type == typeof(long)
                            || type == typeof(float)
                            || type == typeof(double)
                            || type == typeof(short)
                            || type == typeof(Single))
                        {
                            return ParseNullNumeric(s, ref index);
                        }
                        else if (IsNullableType(type))
                        {
                            return ParseNull(s, ref index);
                        }
                        else throw new Exception("Unexpected character when reading value: " + c);
                }
            }
            return null;
        }
        private object ConsumeIntoDictionary(string s, ref int index, IDictionary dict, Type type)
        {
            
            char c;
            for (; index < s.Length; index++)
            {
                c = s[index];
                switch (c)
                {
                    
                    case '{':
                        return ConsumeObjectIntoDictionary(s, dict, type, ref index);
                    default:
                        throw new Exception("Unexpected character when reading dictionary value: " + c);
                }
            }
            return null;
        }
        private bool IsNullableType(Type type)
        {
            return (!type.IsValueType || (type.IsGenericType &&
            type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))));
        }
        private bool ConsumeWhiteSpace(string s, ref int index)
        {
            for (; index < s.Length; index++)
            {
                switch (s[index])
                {
                    case ' ':
                    case '\t':
                    case '\n':
                    case '\r':
                        continue;
                    default:
                        return true;
                }
            }
            return false;
        }
    }
}
