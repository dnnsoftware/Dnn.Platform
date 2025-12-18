// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Reflection;
    using System.Web.Compilation;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Library responsible for reflection.</summary>
    public partial class Reflection
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Reflection));

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider configured in web.config.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string objectProviderType)
        {
            return CreateObject(objectProviderType, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string objectProviderType)
        {
            return CreateObject(serviceProvider, objectProviderType, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider configured in web.config.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string objectProviderType, bool useCache)
        {
            return CreateObject(objectProviderType, string.Empty, string.Empty, string.Empty, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string objectProviderType, bool useCache)
        {
            return CreateObject(serviceProvider, objectProviderType, string.Empty, string.Empty, string.Empty, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace and AssemblyName ( this allows derived providers to share the same config ).</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string objectProviderType, string objectNamespace, string objectAssemblyName)
        {
            return CreateObject(objectProviderType, string.Empty, objectNamespace, objectAssemblyName, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string objectProviderType, string objectNamespace, string objectAssemblyName)
        {
            return CreateObject(serviceProvider, objectProviderType, string.Empty, objectNamespace, objectAssemblyName, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace and AssemblyName ( this allows derived providers to share the same config ).</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string objectProviderType, string objectNamespace, string objectAssemblyName, bool useCache)
        {
            return CreateObject(objectProviderType, string.Empty, objectNamespace, objectAssemblyName, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string objectProviderType, string objectNamespace, string objectAssemblyName, bool useCache)
        {
            return CreateObject(serviceProvider, objectProviderType, string.Empty, objectNamespace, objectAssemblyName, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace, AssemblyName and ProviderName.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName)
        {
            return CreateObject(objectProviderType, objectProviderName, objectNamespace, objectAssemblyName, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName)
        {
            return CreateObject(serviceProvider, objectProviderType, objectProviderName, objectNamespace, objectAssemblyName, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace, AssemblyName and ProviderName.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName, bool useCache)
        {
            return CreateObject(objectProviderType, objectProviderName, objectNamespace, objectAssemblyName, useCache, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName, bool useCache)
        {
            return CreateObject(serviceProvider, objectProviderType, objectProviderName, objectNamespace, objectAssemblyName, useCache, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <param name="fixAssemblyName">Whether append provider name as part of the assembly name.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace, AssemblyName and ProviderName.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(
            string objectProviderType,
            string objectProviderName,
            string objectNamespace,
            string objectAssemblyName,
            bool useCache,
            bool fixAssemblyName)
        {
            return CreateObject(
                Globals.GetCurrentServiceProvider(),
                objectProviderType,
                objectProviderName,
                objectNamespace,
                objectAssemblyName,
                useCache,
                fixAssemblyName);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <param name="fixAssemblyName">Whether append provider name as part of the assembly name.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace, AssemblyName and ProviderName.</remarks>
        public static object CreateObject(IServiceProvider serviceProvider, string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName, bool useCache, bool fixAssemblyName)
        {
            string typeName;

            // get the provider configuration based on the type
            ProviderConfiguration objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration(objectProviderType);
            if (!string.IsNullOrEmpty(objectNamespace) && !string.IsNullOrEmpty(objectAssemblyName))
            {
                // if both the Namespace and AssemblyName are provided then we will construct an "assembly qualified typename" - ie. "NameSpace.ClassName, AssemblyName"
                if (string.IsNullOrEmpty(objectProviderName))
                {
                    // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                    typeName = objectNamespace + "." + objProviderConfiguration.DefaultProvider + ", " + objectAssemblyName + (fixAssemblyName ? "." + objProviderConfiguration.DefaultProvider : string.Empty);
                }
                else
                {
                    // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                    typeName = objectNamespace + "." + objectProviderName + ", " + objectAssemblyName + (fixAssemblyName ? "." + objectProviderName : string.Empty);
                }
            }
            else
            {
                // if only the Namespace is provided then we will construct an "full typename" - ie. "NameSpace.ClassName"
                if (!string.IsNullOrEmpty(objectNamespace))
                {
                    if (string.IsNullOrEmpty(objectProviderName))
                    {
                        // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                        typeName = objectNamespace + "." + objProviderConfiguration.DefaultProvider;
                    }
                    else
                    {
                        // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                        typeName = objectNamespace + "." + objectProviderName;
                    }
                }
                else
                {
                    // if neither Namespace or AssemblyName are provided then we will get the typename from the default provider
                    if (string.IsNullOrEmpty(objectProviderName))
                    {
                        // get the typename of the default Provider from web.config
                        typeName = ((Provider)objProviderConfiguration.Providers[objProviderConfiguration.DefaultProvider]).Type;
                    }
                    else
                    {
                        // get the typename of the specified ProviderName from web.config
                        typeName = ((Provider)objProviderConfiguration.Providers[objectProviderName]).Type;
                    }
                }
            }

            return CreateObject(serviceProvider, typeName, typeName, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="typeName">The fully qualified TypeName.</param>
        /// <param name="cacheKey">The Cache Key.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload that takes a fully-qualified typename and a Cache Key.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string typeName, string cacheKey)
        {
            return CreateObject(typeName, cacheKey, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="typeName">The fully qualified TypeName.</param>
        /// <param name="cacheKey">The Cache Key.</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string typeName, string cacheKey)
        {
            return CreateObject(serviceProvider, typeName, cacheKey, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="typeName">The fully qualified TypeName.</param>
        /// <param name="cacheKey">The Cache Key.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload that takes a fully-qualified typename and a Cache Key.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(string typeName, string cacheKey, bool useCache)
        {
            return CreateObject(Globals.GetCurrentServiceProvider(), typeName, cacheKey, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="typeName">The fully qualified TypeName.</param>
        /// <param name="cacheKey">The Cache Key.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, string typeName, string cacheKey, bool useCache)
        {
            var type = CreateType(typeName, cacheKey, useCache);
            return CreateObject(serviceProvider, type);
        }

        /// <summary>Creates an object.</summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <returns>The created object.</returns>
        /// <remarks>Generic version.</remarks>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial T CreateObject<T>()
        {
            return CreateObject<T>(Globals.GetCurrentServiceProvider());
        }

        /// <summary>Creates an object.</summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <param name="serviceProvider">The DI container.</param>
        /// <returns>The created object.</returns>
        public static T CreateObject<T>(IServiceProvider serviceProvider)
        {
            try
            {
                return ActivatorUtilities.GetServiceOrCreateInstance<T>(serviceProvider);
            }
            catch (InvalidOperationException exception)
            {
                Logger.Warn($"Unable to create type via service provider: {typeof(T)}", exception);
                return Activator.CreateInstance<T>();
            }
        }

        /// <summary>Creates an object.</summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns>The created object.</returns>
        [DnnDeprecated(9, 11, 3, "Please use overload with IServiceProvider")]
        public static partial object CreateObject(Type type)
        {
            return CreateObject(Globals.GetCurrentServiceProvider(), type);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="type">The type of object to create.</param>
        /// <returns>The created object.</returns>
        public static object CreateObject(IServiceProvider serviceProvider, Type type)
        {
            try
            {
                return ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, type);
            }
            catch (InvalidOperationException exception)
            {
                Logger.Warn($"Unable to create type via service provider: {type}", exception);
                return Activator.CreateInstance(type);
            }
        }

        /// <summary>Creates a type. Caches type creation for performance. Errors creating the type are logged.</summary>
        /// <param name="typeName">The name of the type to create.</param>
        /// <returns>The <see cref="Type"/> instance or <see langword="null"/>.</returns>
        public static Type CreateType(string typeName)
        {
            return CreateType(typeName, string.Empty, true, false);
        }

        /// <summary>Creates a type. Caches type creation for performance.</summary>
        /// <param name="typeName">The name of the type to create.</param>
        /// <param name="ignoreErrors">Whether to log exceptions.</param>
        /// <returns>The <see cref="Type"/> instance or <see langword="null"/>.</returns>
        public static Type CreateType(string typeName, bool ignoreErrors)
        {
            return CreateType(typeName, string.Empty, true, ignoreErrors);
        }

        /// <summary>Creates a type. Errors creating the type are logged.</summary>
        /// <param name="typeName">The name of the type to create.</param>
        /// <param name="cacheKey">A custom cache key; otherwise <paramref name="typeName"/> is used as the key.</param>
        /// <param name="useCache">Whether to store the type in the cache or bypass the cache.</param>
        /// <returns>The <see cref="Type"/> instance or <see langword="null"/>.</returns>
        public static Type CreateType(string typeName, string cacheKey, bool useCache)
        {
            return CreateType(typeName, cacheKey, useCache, false);
        }

        /// <summary>Creates a type.</summary>
        /// <param name="typeName">The name of the type to create.</param>
        /// <param name="cacheKey">A custom cache key; otherwise <paramref name="typeName"/> is used as the key.</param>
        /// <param name="useCache">Whether to store the type in the cache or bypass the cache.</param>
        /// <param name="ignoreErrors">Whether to log exceptions.</param>
        /// <returns>The <see cref="Type"/> instance or <see langword="null"/>.</returns>
        public static Type CreateType(string typeName, string cacheKey, bool useCache, bool ignoreErrors)
        {
            if (string.IsNullOrEmpty(cacheKey))
            {
                cacheKey = typeName;
            }

            Type type = null;

            // use the cache for performance
            if (useCache)
            {
                type = (Type)DataCache.GetCache(cacheKey);
            }

            // is the type in the cache?
            if (type == null)
            {
                try
                {
                    // use reflection to get the type of the class
                    type = BuildManager.GetType(typeName, true, true);
                    if (useCache)
                    {
                        // insert the type into the cache
                        DataCache.SetCache(cacheKey, type);
                    }
                }
                catch (Exception exc)
                {
                    if (!ignoreErrors)
                    {
                        Logger.Error(typeName, exc);
                    }
                }
            }

            return type;
        }

        /// <summary>Dynamically call the empty constructor for a <paramref name="type"/>.</summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns>An instance of the object, or <see langword="null"/> if <paramref name="type"/> is <see langword="null"/>.</returns>
        public static object CreateInstance(Type type)
        {
            return CreateInstance(type, null);
        }

        /// <summary>Dynamically call a constructor for a <paramref name="type"/>.</summary>
        /// <param name="type">The type of object to create.</param>
        /// <param name="args">The constructor arguments.</param>
        /// <returns>An instance of the object, or <see langword="null"/> if <paramref name="type"/> is <see langword="null"/>.</returns>
        public static object CreateInstance(Type type, object[] args)
        {
            if (type != null)
            {
                return type.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, args, null);
            }
            else
            {
                return null;
            }
        }

        /// <summary>Dynamically get the value of a property.</summary>
        /// <param name="type">The type of <paramref name="target"/>.</param>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="target">The object from which to read the property.</param>
        /// <returns>The property's value, or <see langword="null"/> if <paramref name="type"/> is <see langword="null"/>.</returns>
        public static object GetProperty(Type type, string propertyName, object target)
        {
            if (type != null)
            {
                return type.InvokeMember(propertyName, BindingFlags.GetProperty, null, target, null);
            }
            else
            {
                return null;
            }
        }

        /// <summary>Dynamically set the value of a property.</summary>
        /// <param name="type">The type of <paramref name="target"/>.</param>
        /// <param name="propertyName">The name of the property to set.</param>
        /// <param name="target">The object on which to set the property.</param>
        /// <param name="args">The input to the property.</param>
        public static void SetProperty(Type type, string propertyName, object target, object[] args)
        {
            if (type != null)
            {
                type.InvokeMember(propertyName, BindingFlags.SetProperty, null, target, args);
            }
        }

        /// <summary>Dynamically invoke a method on an object.</summary>
        /// <param name="type">The type of <paramref name="target"/>.</param>
        /// <param name="propertyName">The name of the method to invoke.</param>
        /// <param name="target">The object on which to invoke the method.</param>
        /// <param name="args">The input to the method.</param>
        public static void InvokeMethod(Type type, string propertyName, object target, object[] args)
        {
            if (type != null)
            {
                type.InvokeMember(propertyName, BindingFlags.InvokeMethod, null, target, args);
            }
        }

        /// <summary>Dynamically create a default Provider from a ProviderType.</summary>
        /// <param name="objectProviderType">The name of the <see cref="Type"/>.</param>
        /// <returns>The provider instance.</returns>
        [DnnDeprecated(7, 0, 0, "Please use CreateObject(string objectProviderType, bool useCache)", RemovalVersion = 11)]
        internal static partial object CreateObjectNotCached(string objectProviderType)
        {
            string typeName = string.Empty;
            Type objType = null;

            // get the provider configuration based on the type
            ProviderConfiguration objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration(objectProviderType);

            // get the typename of the Base DataProvider from web.config
            typeName = ((Provider)objProviderConfiguration.Providers[objProviderConfiguration.DefaultProvider]).Type;
            try
            {
                // use reflection to get the type of the class
                objType = BuildManager.GetType(typeName, true, true);
            }
            catch (Exception exc)
            {
                // could not load the type
                Exceptions.LogException(exc);
            }

            // dynamically create the object
            return Activator.CreateInstance(objType);
        }
    }
}
