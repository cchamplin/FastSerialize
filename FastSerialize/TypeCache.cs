using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    public class TypeCache
    {
        public Dictionary<string, PropertyAccessor> properties;
        public TypeCache(Type t)
        {
            constructor = TypeHelper.GetConstructor(t);
            properties = new Dictionary<string, PropertyAccessor>();
            PropertyInfo[] typeProperties = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] typeFields = t.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo pi in typeProperties)
            {
                var p = new PropertyAccessor(t,pi);
                if (p.accessFor != null)
                {
                    properties.Add(p.accessFor, p);
                    continue;
                }
                properties.Add(pi.Name, p);
            }
            foreach (FieldInfo fi in typeFields)
            {
                var f = new PropertyAccessor(t, fi);
                if (f.accessFor != null)
                {
                    properties.Add(f.accessFor, f);
                    continue;
                }
                properties.Add(fi.Name, f);
            }
        }
        public TypeHelper.ConstructorDelegate constructor;
    }
}
