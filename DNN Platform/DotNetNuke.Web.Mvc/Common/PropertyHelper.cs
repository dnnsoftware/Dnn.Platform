#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DNN Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetNuke.Common;

namespace DotNetNuke.Web.Mvc.Common
{
    internal class PropertyHelper
    {
        private static readonly MethodInfo CallPropertyGetterByReferenceOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetterByReference", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo CallPropertyGetterOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetter", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly ConcurrentDictionary<Type, PropertyHelper[]> ReflectionCache = new ConcurrentDictionary<Type, PropertyHelper[]>();
        private readonly Func<object, object> _valueGetter;

        public PropertyHelper(PropertyInfo property)
        {
            Requires.NotNull("property", property);

            Name = property.Name;
            _valueGetter = MakeFastPropertyGetter(property);
        }

        // Implementation of the fast getter.
        private delegate TValue ByRefFunc<TDeclaringType, TValue>(ref TDeclaringType arg);

        public virtual string Name { get; protected set; }

        private static object CallPropertyGetter<TDeclaringType, TValue>(Func<TDeclaringType, TValue> getter, object @this)
        {
            return getter((TDeclaringType)@this);
        }

        private static object CallPropertyGetterByReference<TDeclaringType, TValue>(ByRefFunc<TDeclaringType, TValue> getter, object @this)
        {
            TDeclaringType unboxed = (TDeclaringType)@this;
            return getter(ref unboxed);
        }

        private static PropertyHelper CreateInstance(PropertyInfo property)
        {
            return new PropertyHelper(property);
        }

        /// <summary>
        /// Creates and caches fast property helpers that expose getters for every public get property on the underlying type.
        /// </summary>
        /// <param name="instance">the instance to extract property accessors for.</param>
        /// <returns>a cached array of all public property getters from the underlying type of this instance.</returns>
        public static PropertyHelper[] GetProperties(object instance)
        {
            return GetProperties(instance, CreateInstance, ReflectionCache);
        }

        protected static PropertyHelper[] GetProperties(object instance,
                                                Func<PropertyInfo, PropertyHelper> createPropertyHelper,
                                                ConcurrentDictionary<Type, PropertyHelper[]> cache)
        {
            // Using an array rather than IEnumerable, as this will be called on the hot path numerous times.
            PropertyHelper[] helpers;

            Type type = instance.GetType();

            if (!cache.TryGetValue(type, out helpers))
            {
                // We avoid loading indexed properties using the where statement.
                // Indexed properties are not useful (or valid) for grabbing properties off an anonymous object.
                IEnumerable<PropertyInfo> properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                           .Where(prop => prop.GetIndexParameters().Length == 0 &&
                                                                          prop.GetMethod != null);

                var newHelpers = new List<PropertyHelper>();

                foreach (PropertyInfo property in properties)
                {
                    PropertyHelper propertyHelper = createPropertyHelper(property);

                    newHelpers.Add(propertyHelper);
                }

                helpers = newHelpers.ToArray();
                cache.TryAdd(type, helpers);
            }

            return helpers;
        }

        public object GetValue(object instance)
        {
            //Contract.Assert(_valueGetter != null, "Must call Initialize before using this object");

            return _valueGetter(instance);
        }

        /// <summary>
        /// Creates a single fast property getter. The result is not cached.
        /// </summary>
        /// <param name="propertyInfo">propertyInfo to extract the getter for.</param>
        /// <returns>a fast getter.</returns>
        /// <remarks>This method is more memory efficient than a dynamically compiled lambda, and about the same speed.</remarks>
        public static Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            Requires.NotNull("property", propertyInfo);

            MethodInfo getMethod = propertyInfo.GetGetMethod();
            Guard.Against(getMethod == null, "Property must have a Get Method");
            Guard.Against(getMethod.IsStatic, "Property's Get method must not be static");
            Guard.Against(getMethod.GetParameters().Length != 0, "Property's Get method must not have parameters");

            // Instance methods in the CLR can be turned into static methods where the first parameter
            // is open over "this". This parameter is always passed by reference, so we have a code
            // path for value types and a code path for reference types.
            Type typeInput = getMethod.ReflectedType;
            Type typeOutput = getMethod.ReturnType;

            Delegate callPropertyGetterDelegate;
            if (typeInput.IsValueType)
            {
                // Create a delegate (ref TDeclaringType) -> TValue
                Delegate propertyGetterAsFunc = getMethod.CreateDelegate(typeof(ByRefFunc<,>).MakeGenericType(typeInput, typeOutput));
                MethodInfo callPropertyGetterClosedGenericMethod = CallPropertyGetterByReferenceOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }
            else
            {
                // Create a delegate TDeclaringType -> TValue
                Delegate propertyGetterAsFunc = getMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(typeInput, typeOutput));
                MethodInfo callPropertyGetterClosedGenericMethod = CallPropertyGetterOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }

            return (Func<object, object>)callPropertyGetterDelegate;
        }
    }
}
