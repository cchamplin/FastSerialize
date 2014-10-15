using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastSerializeTest
{
    public class SimpleType
    {
        public string Foo;
        public int Bar;
        public String FooBar;
        public int FooProperty
        {
            get;
            set;
        }
        public List<SimpleType> initList()
        {
            return new List<SimpleType>();
        }
        
        public void setFoo(object type, object value)
        {
            ((SimpleType)type).Foo = (string)value;
        }
        public void setFooProperty(object type, object value)
        {
            ((SimpleType)type).FooProperty = (int)value;
        }

        public string getFoo(object type)
        {
            return ((SimpleType)type).Foo;
        }
        public int getFooProperty(object type)
        {
            return ((SimpleType)type).FooProperty;
        }
    }
}
