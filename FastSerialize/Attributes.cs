using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    [System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
    public class DictionarySet : Attribute
    {
    }
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class NamedProperty : Attribute
    {
        public string PropertyName;
        public NamedProperty(string name)
        {
            this.PropertyName = name;
        }
    }
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class IgnoreCase : Attribute
    {
        public IgnoreCase()
        {
        }
    }
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class PrefixConsumer : Attribute
    {
        public string PropertyPrefix;
        public PrefixConsumer(string name)
        {
            this.PropertyPrefix = name;
        }
    }
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Parameter, AllowMultiple = false)]
    public class SuffixConsumer : Attribute
    {
        public string PropertySuffix;
        public SuffixConsumer(string name)
        {
            this.PropertySuffix = name;
        }
    }
}
