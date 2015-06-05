using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{

    internal class TypeCache
    {
        public ConcurrentDictionary<string, PropertyAccessor> properties;
        public TypeCache(Type t)
        {
            constructor = TypeHelper.GetConstructor(t);
            properties = new ConcurrentDictionary<string, PropertyAccessor>();
            PropertyInfo[] typeProperties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] typeFields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo pi in typeProperties)
            {
                var p = new PropertyAccessor(t,pi);
                if (p.accessFor != null)
                {
                    properties.TryAdd(p.accessFor, p);
                    continue;
                }
                properties.TryAdd(pi.Name, p);
            }
            foreach (FieldInfo fi in typeFields)
            {
                var f = new PropertyAccessor(t, fi);
                if (f.accessFor != null)
                {
                    properties.TryAdd(f.accessFor, f);
                    continue;
                }
                properties.TryAdd(fi.Name, f);
            }
        }
        public TypeHelper.ConstructorDelegate constructor;
    }
}
