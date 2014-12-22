using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FastSerialize;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using Mono.Cecil.Cil;
namespace FastSerializeTest
{
    [TestClass]
    public class FastSerializeTest
    {

        [TestMethod]
        public void TestTypeCache()
        {

            

            TypeCache tc = new TypeCache(typeof(SimpleType));
            SimpleType instance = (SimpleType)tc.constructor();

   
            tc.properties["FooProperty"].setter(instance, (object)15);

            if (instance.FooProperty != 15)
                throw new ArgumentException("Invalid Value");
        }

        [TestMethod]
        public void TestSimpleSerialize()
        {
            Serializer s = new Serializer(typeof(JsonSerializerGeneric));
            var o = new { Test="Mer" };
            String result = s.Serialize(o);
            result.ToString();


            SimpleType o2 = new SimpleType { Bar = 55, FooBar = "Value3" };
            result = s.Serialize(o2);
            result.ToString();
        }

        [TestMethod]
        public void TestComplexSerialize()
        {
            StreamReader sr = new StreamReader("TestCases/complex_01.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            List<ComplexType> data = s.Deserialize<List<ComplexType>>(json);
            string result = s.Serialize(data);
            result.ToString();
            System.Diagnostics.Debug.Write(result.ToString());
        }

        [TestMethod]
        public void TestDeserialize_Mising01_String()
        {
            StreamReader sr = new StreamReader("TestCases/missing_01.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            SimpleType result = s.Deserialize<SimpleType>(json,false);

            if (result == null || result.Foo != "Test" || result.Bar != 55 || result.FooBar != "Value1")
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Mising02_String()
        {
            StreamReader sr = new StreamReader("TestCases/missing_02.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            SimpleType result = s.Deserialize<SimpleType>(json, false);

            if (result == null || result.Foo != "Test" || result.Bar != 55 || result.FooBar != "Value1")
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Mising03_String()
        {
            StreamReader sr = new StreamReader("TestCases/missing_03.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            SimpleType result = s.Deserialize<SimpleType>(json, false);

            if (result == null || result.Foo != "Test" || result.Bar != 55 || result.FooBar != "Value1")
                throw new Exception("Data mismatch");

        }


        [TestMethod]
        public void TestDeserialize_Simple01_String()
        {
            StreamReader sr = new StreamReader("TestCases/simple_01.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerString));

            List<SimpleType> result = s.Deserialize<List<SimpleType>>(json);

            if (result == null || result.Count != 0)
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Simple01_Generic()
        {
            StreamReader sr = new StreamReader("TestCases/simple_01.json");

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            List<SimpleType> result = s.Deserialize<List<SimpleType>>(sr.BaseStream);

            if (result == null || result.Count != 0)
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Simple02_String()
        {
            StreamReader sr = new StreamReader("TestCases/simple_02.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerString));

            SimpleType result = s.Deserialize<SimpleType>(json);

            if (result == null)
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Simple02_Generic()
        {
            StreamReader sr = new StreamReader("TestCases/simple_02.json");

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            SimpleType result = s.Deserialize<SimpleType>(sr.BaseStream);

            if (result == null)
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Simple03_String()
        {
            StreamReader sr = new StreamReader("TestCases/simple_03.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerString));

            SimpleType result = s.Deserialize<SimpleType>(json);

            if (result == null)
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Simple03_Generic()
        {
            StreamReader sr = new StreamReader("TestCases/simple_03.json");

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            SimpleType result = s.Deserialize<SimpleType>(sr.BaseStream);

            if (result == null)
                throw new Exception("Data mismatch");

        }


        [TestMethod]
        public void TestDeserialize_Complex01_String()
        {
            StreamReader sr = new StreamReader("TestCases/complex_01.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerString));

            List<ComplexType> result = s.Deserialize<List<ComplexType>>(json);

            if (result == null || result.Count != 5)
                throw new Exception("Data mismatch");
           
        }

        [TestMethod]
        public void TestDeserialize_Complex01_Generic()
        {
            StreamReader sr = new StreamReader("TestCases/complex_01.json");

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            List<ComplexType> result = s.Deserialize<List<ComplexType>>(sr.BaseStream);

            if (result == null || result.Count != 5)
                throw new Exception("Data mismatch");

        }
        [TestMethod]
        public void TestDeserialize_Complex02_String()
        {
            StreamReader sr = new StreamReader("TestCases/complex_02.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerString));

            List<ComplexType> result = s.Deserialize<List<ComplexType>>(json);

            if (result == null || result.Count != 7)
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Complex02_Generic()
        {
            StreamReader sr = new StreamReader("TestCases/complex_02.json");

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            List<ComplexType> result = s.Deserialize<List<ComplexType>>(sr.BaseStream);

            if (result == null || result.Count != 7)
                throw new Exception("Data mismatch");

        }
        [TestMethod]
        public void TestDeserialize_Complex03_String()
        {
            StreamReader sr = new StreamReader("TestCases/complex_03.json");
            var json = sr.ReadToEnd();

            Serializer s = new Serializer(typeof(JsonSerializerString));

            ComplexType result = s.Deserialize<ComplexType>(json);

            if (result == null)
                throw new Exception("Data mismatch");

        }

        [TestMethod]
        public void TestDeserialize_Complex03_Generic()
        {
            StreamReader sr = new StreamReader("TestCases/complex_03.json");

            Serializer s = new Serializer(typeof(JsonSerializerGeneric));

            ComplexType result = s.Deserialize<ComplexType>(sr.BaseStream);

            if (result == null)
                throw new Exception("Data mismatch");

        }
    }
}
