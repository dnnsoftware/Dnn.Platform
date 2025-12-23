// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Commons
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Common;

    /// <summary>
    /// Provides fast reflection-based access to property values using cached delegates.
    /// </summary>
    internal class PropertyHelper
    {
        private static readonly MethodInfo CallPropertyGetterByReferenceOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetterByReference", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo CallPropertyGetterOpenGenericMethod = typeof(PropertyHelper).GetMethod("CallPropertyGetter", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly ConcurrentDictionary<Type, PropertyHelper[]> ReflectionCache = new ConcurrentDictionary<Type, PropertyHelper[]>();
        private readonly Func<object, object> valueGetter;

        /// <summary>Initializes a new instance of the <see cref="PropertyHelper"/> class.</summary>
        /// <param name="property">The property.</param>
        public PropertyHelper(PropertyInfo property)
        {
            Requires.NotNull("property", property);

            this.Name = property.Name;
            this.valueGetter = MakeFastPropertyGetter(property);
        }

        // Implementation of the fast getter.
        private delegate TValue ByRefFunc<TDeclaringType, TValue>(ref TDeclaringType arg);

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>Creates and caches fast property helpers that expose getters for every public get property on the underlying type.</summary>
        /// <param name="instance">The instance to extract property accessors for.</param>
        /// <returns>A cached array of all public property getters from the underlying type of this instance.</returns>
        public static PropertyHelper[] GetProperties(object instance)
        {
            return GetProperties(instance, CreateInstance, ReflectionCache);
        }

        /// <summary>Creates a single fast property getter. The result is not cached.</summary>
        /// <param name="propertyInfo">Property information to extract the getter for.</param>
        /// <returns>A fast property getter delegate.</returns>
        /// <remarks>This method is more memory efficient than a dynamically compiled lambda, and about the same speed.</remarks>
        public static Func<object, object> MakeFastPropertyGetter(PropertyInfo propertyInfo)
        {
            Requires.NotNull("property", propertyInfo);

            var getMethod = propertyInfo.GetGetMethod();
            Guard.Against(getMethod == null, "Property must have a Get Method");
            Guard.Against(getMethod.IsStatic, "Property's Get method must not be static");
            Guard.Against(getMethod.GetParameters().Length != 0, "Property's Get method must not have parameters");

            // Instance methods in the CLR can be turned into static methods where the first parameter
            // is open over "this". This parameter is always passed by reference, so we have a code
            // path for value types and a code path for reference types.
            var typeInput = getMethod.ReflectedType;
            var typeOutput = getMethod.ReturnType;

            Delegate callPropertyGetterDelegate;
            if (typeInput.IsValueType)
            {
                // Create a delegate (ref TDeclaringType) -> TValue
                var propertyGetterAsFunc = getMethod.CreateDelegate(typeof(ByRefFunc<,>).MakeGenericType(typeInput, typeOutput));
                var callPropertyGetterClosedGenericMethod = CallPropertyGetterByReferenceOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }
            else
            {
                // Create a delegate TDeclaringType -> TValue
                var propertyGetterAsFunc = getMethod.CreateDelegate(typeof(Func<,>).MakeGenericType(typeInput, typeOutput));
                var callPropertyGetterClosedGenericMethod = CallPropertyGetterOpenGenericMethod.MakeGenericMethod(typeInput, typeOutput);
                callPropertyGetterDelegate = Delegate.CreateDelegate(typeof(Func<object, object>), propertyGetterAsFunc, callPropertyGetterClosedGenericMethod);
            }

            return (Func<object, object>)callPropertyGetterDelegate;
        }

        /// <summary>
        /// Gets the value of the property for the specified instance.
        /// </summary>
        /// <param name="instance">The instance to read the property value from.</param>
        /// <returns>The property value.</returns>
        public object GetValue(object instance)
        {
            // Contract.Assert(valueGetter != null, "Must call Initialize before using this object");
            return this.valueGetter(instance);
        }

        /// <summary>
        /// Gets or creates cached <see cref="PropertyHelper"/> instances for the specified object's type.
        /// </summary>
        /// <param name="instance">The instance whose type is used to discover properties.</param>
        /// <param name="createPropertyHelper">A factory used to create new <see cref="PropertyHelper"/> instances.</param>
        /// <param name="cache">The cache used to store helpers per type.</param>
        /// <returns>An array of <see cref="PropertyHelper"/> objects describing the type's public properties.</returns>
        protected static PropertyHelper[] GetProperties(
            object instance,
            Func<PropertyInfo, PropertyHelper> createPropertyHelper,
            ConcurrentDictionary<Type, PropertyHelper[]> cache)
        {
            // Using an array rather than IEnumerable, as this will be called on the hot path numerous times.
            PropertyHelper[] helpers;

            var type = instance.GetType();

            if (!cache.TryGetValue(type, out helpers))
            {
                // We avoid loading indexed properties using the where statement.
                // Indexed properties are not useful (or valid) for grabbing properties off an anonymous object.
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                                           .Where(prop => prop.GetIndexParameters().Length == 0 &&
                                                                          prop.GetMethod != null);

                var newHelpers = new List<PropertyHelper>();

                foreach (var property in properties)
                {
                    var propertyHelper = createPropertyHelper(property);

                    newHelpers.Add(propertyHelper);
                }

                helpers = newHelpers.ToArray();
                cache.TryAdd(type, helpers);
            }

            return helpers;
        }

        private static object CallPropertyGetter<TDeclaringType, TValue>(Func<TDeclaringType, TValue> getter, object @this)
        {
            return getter((TDeclaringType)@this);
        }

        private static object CallPropertyGetterByReference<TDeclaringType, TValue>(ByRefFunc<TDeclaringType, TValue> getter, object @this)
        {
            var unboxed = (TDeclaringType)@this;
            return getter(ref unboxed);
        }

        private static PropertyHelper CreateInstance(PropertyInfo property)
        {
            return new PropertyHelper(property);
        }
    }
}
