using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace FastSerialize
{
    internal static class TypeHelper
    {
        public static ConcurrentDictionary<Type, ConstructorDelegate> _constructorCache;
        public static ConcurrentDictionary<String, Type> _resolvedTypeCache;
        static TypeHelper()
        {
            _constructorCache = new ConcurrentDictionary<Type, ConstructorDelegate>();
            _resolvedTypeCache = new ConcurrentDictionary<string, Type>();
        }
        public delegate object ConstructorDelegate();
        public static Type ResolveType(string lookup) {
            
                Type t;
                if (_resolvedTypeCache.TryGetValue(lookup, out t))
                    return t;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                var name = lookup.Substring(0, lookup.IndexOf('#'));
                var ns = lookup.Substring(lookup.IndexOf('#')+1);
                t = assemblies.SelectMany(a => a.GetTypes())
                                        .Single(ty => (ty.Name == name && ty.Namespace == ns));
                _resolvedTypeCache.TryAdd(lookup, t);
                return t;
            
        }
        public static ConstructorDelegate GetConstructor(Type type)
        {
            
                ConstructorDelegate tDel;
                if (_constructorCache.TryGetValue(type, out tDel))
                    return tDel;

                ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
                if (ci == null)
                {
                    ci = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                }
                if (ci != null)
                {
                    DynamicMethod method = new System.Reflection.Emit.DynamicMethod("__Ctor", type, Type.EmptyTypes, typeof(TypeHelper).Module, true);
                    ILGenerator gen = method.GetILGenerator();
                    gen.Emit(System.Reflection.Emit.OpCodes.Newobj, ci);
                    gen.Emit(System.Reflection.Emit.OpCodes.Ret);
                    tDel = (ConstructorDelegate)method.CreateDelegate(typeof(ConstructorDelegate));
                    _constructorCache.TryAdd(type, tDel);
                    return tDel;
                }
                return () => Activator.CreateInstance(type);
            
        }
        public static void Cast(ILGenerator il, Type type, LocalBuilder addr)
        {
            if (type == typeof(object)) { }
            else if (type.IsValueType)
            {
                il.Emit(OpCodes.Unbox_Any, type);
                if (addr != null)
                {
                    il.Emit(OpCodes.Stloc, addr);
                    il.Emit(OpCodes.Ldloca_S, addr);
                }
            }
            else
            {
                il.Emit(OpCodes.Castclass, type);
            }
        }
    }
    // Todo dynamic members
    internal class DynanmicInstance : DynamicObject
    {
        public DynanmicInstance()
        {
            
        }
        private Dictionary<string, object> _dictionary = new Dictionary<string, object>();
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return base.TryGetMember(binder, out result);
        }
    }
    public class PropertyAccessor
    {
        public delegate void PropertySetter(object instance, object value);
        public delegate object PropertyGetter(object instance);
        public Type type;
        public PropertySetter setter;
        public PropertyGetter getter;
        public bool isSpecial;
        public string accessFor;
        public Type genericType;
        public bool isList;
        public bool isDictionary;
        public string prefixMatch;
        public string suffixMatch;
        public bool ignoreCase;

        public PropertyAccessor(Type t, PropertyInfo pi)
        {
            this.type = pi.PropertyType;

            foreach (Attribute attr in pi.GetCustomAttributes(false)) {
                if (attr is NamedProperty)
                {
                    
                    this.accessFor = ((NamedProperty)attr).PropertyName;
                }
                if (attr is PrefixConsumer)
                {
                    this.isSpecial = true;
                    this.prefixMatch = ((PrefixConsumer)attr).PropertyPrefix;
                }
                if (attr is IgnoreCase)
                {
                    this.isSpecial = true;
                    this.ignoreCase = true;
                }
                if (attr is SuffixConsumer)
                {
                    this.isSpecial = true;
                    this.suffixMatch = ((SuffixConsumer)attr).PropertySuffix;
                }
            }
            if (this.type.IsGenericType)
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i == typeof(IList))
                    {
                        this.isList = true;
                        this.genericType = this.type.GetGenericArguments()[0];
                        break;
                    }
                    if (i == typeof(IDictionary))
                    {
                        this.isDictionary = true;
                        this.genericType = this.type.GetGenericArguments()[1];
                    }
                }
            }
            ILGenerator gen;
            DynamicMethod method;
            LocalBuilder loc;
            if (pi.CanWrite)
            {
                method = new System.Reflection.Emit.DynamicMethod("__setter" + pi.Name, typeof(void), new Type[] { typeof(Object), typeof(Object) }, t, true);


                gen = method.GetILGenerator();

                loc = t.IsValueType ? gen.DeclareLocal(t) : null;
                gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                TypeHelper.Cast(gen, t, loc);
                gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
                TypeHelper.Cast(gen, pi.PropertyType, null);
                gen.EmitCall(t.IsValueType ? OpCodes.Call : OpCodes.Callvirt, pi.GetSetMethod(), null);
                gen.Emit(System.Reflection.Emit.OpCodes.Ret);
                setter = (PropertySetter)method.CreateDelegate(typeof(PropertySetter));
            }


            if (pi.CanRead)
            {
                method = new System.Reflection.Emit.DynamicMethod("__getter" + pi.Name, typeof(object), new Type[] { typeof(object) }, t, true);

                gen = method.GetILGenerator();
                loc = t.IsValueType ? gen.DeclareLocal(t) : null;
                gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
                TypeHelper.Cast(gen, t, loc);
                gen.EmitCall(t.IsValueType ? OpCodes.Call : OpCodes.Callvirt, pi.GetGetMethod(), null);
                if (pi.PropertyType.IsValueType)
                {
                    gen.Emit(OpCodes.Box, pi.PropertyType);
                }
                //TypeHelper.Cast(gen, typeof(object), null);
                gen.Emit(System.Reflection.Emit.OpCodes.Ret);
                getter = (PropertyGetter)method.CreateDelegate(typeof(PropertyGetter));
            }
           
        }
        public PropertyAccessor(Type t, FieldInfo fi)
        {
            this.type = fi.FieldType;
            foreach (Attribute attr in fi.GetCustomAttributes(false))
            {
                if (attr is NamedProperty)
                {

                    this.accessFor = ((NamedProperty)attr).PropertyName;
                }
                if (attr is PrefixConsumer)
                {
                    this.isSpecial = true;
                    this.prefixMatch = ((PrefixConsumer)attr).PropertyPrefix;
                }
                if (attr is IgnoreCase)
                {
                    this.isSpecial = true;
                    this.ignoreCase = true;
                }
                if (attr is SuffixConsumer)
                {
                    this.isSpecial = true;
                    this.suffixMatch = ((SuffixConsumer)attr).PropertySuffix;
                }
            }
            if (this.type.IsGenericType)
            {
                foreach (var i in type.GetInterfaces())
                {
                    if (i == typeof(IList))
                    {
                        this.isList = true;
                        this.genericType = this.type.GetGenericArguments()[0];
                        break;
                    }
                    if (i == typeof(IDictionary))
                    {
                        this.isDictionary = true;
                        this.genericType = this.type.GetGenericArguments()[1];
                    }
                }
            }
            DynamicMethod method = new System.Reflection.Emit.DynamicMethod("__setter" + fi.Name, typeof(void), new Type[] { typeof(Object), typeof(Object) }, t, true);

            ILGenerator gen = method.GetILGenerator();
            LocalBuilder loc = t.IsValueType ? gen.DeclareLocal(t) : null;
            gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            TypeHelper.Cast(gen, t, loc);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_1);
            TypeHelper.Cast(gen, fi.FieldType, null);
            gen.Emit(System.Reflection.Emit.OpCodes.Stfld, fi);
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);
            setter = (PropertySetter)method.CreateDelegate(typeof(PropertySetter));
            
            
            method = new System.Reflection.Emit.DynamicMethod("__getter" + fi.Name, typeof(object), new Type[] { typeof(object) }, t, true);

            gen = method.GetILGenerator();
            loc = t.IsValueType ? gen.DeclareLocal(t) : null;
            //gen.Emit(System.Reflection.Emit.OpCodes.Nop);
            gen.Emit(System.Reflection.Emit.OpCodes.Ldarg_0);
            TypeHelper.Cast(gen, t, loc);
            gen.Emit(OpCodes.Ldfld, fi);
            if (fi.FieldType.IsValueType)
            {
                gen.Emit(OpCodes.Box, fi.FieldType);
            }
            gen.Emit(System.Reflection.Emit.OpCodes.Ret);
            getter = (PropertyGetter)method.CreateDelegate(typeof(PropertyGetter));


        }
    }
}
