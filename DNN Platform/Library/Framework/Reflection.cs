// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Reflection;
    using System.Web.Compilation;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;

    /// <summary>Library responsible for reflection.</summary>
    public class Reflection
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Reflection));

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider configured in web.config.</remarks>
        public static object CreateObject(string objectProviderType)
        {
            return CreateObject(objectProviderType, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider configured in web.config.</remarks>
        public static object CreateObject(string objectProviderType, bool useCache)
        {
            return CreateObject(objectProviderType, string.Empty, string.Empty, string.Empty, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace and AssemblyName ( this allows derived providers to share the same config ).</remarks>
        public static object CreateObject(string objectProviderType, string objectNamespace, string objectAssemblyName)
        {
            return CreateObject(objectProviderType, string.Empty, objectNamespace, objectAssemblyName, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace and AssemblyName ( this allows derived providers to share the same config ).</remarks>
        public static object CreateObject(string objectProviderType, string objectNamespace, string objectAssemblyName, bool useCache)
        {
            return CreateObject(objectProviderType, string.Empty, objectNamespace, objectAssemblyName, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace, AssemblyName and ProviderName.</remarks>
        public static object CreateObject(string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName)
        {
            return CreateObject(objectProviderType, objectProviderName, objectNamespace, objectAssemblyName, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="objectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="objectProviderName">The name of the Provider.</param>
        /// <param name="objectNamespace">The namespace of the object to create.</param>
        /// <param name="objectAssemblyName">The assembly of the object to create.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace, AssemblyName and ProviderName.</remarks>
        public static object CreateObject(string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName, bool useCache)
        {
            return CreateObject(objectProviderType, objectProviderName, objectNamespace, objectAssemblyName, useCache, true);
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
        public static object CreateObject(string objectProviderType, string objectProviderName, string objectNamespace, string objectAssemblyName, bool useCache, bool fixAssemblyName)
        {
            string typeName = string.Empty;

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

            return CreateObject(typeName, typeName, useCache);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="typeName">The fully qualified TypeName.</param>
        /// <param name="cacheKey">The Cache Key.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload that takes a fully-qualified typename and a Cache Key.</remarks>
        public static object CreateObject(string typeName, string cacheKey)
        {
            return CreateObject(typeName, cacheKey, true);
        }

        /// <summary>Creates an object.</summary>
        /// <param name="typeName">The fully qualified TypeName.</param>
        /// <param name="cacheKey">The Cache Key.</param>
        /// <param name="useCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload that takes a fully-qualified typename and a Cache Key.</remarks>
        public static object CreateObject(string typeName, string cacheKey, bool useCache)
        {
            return Activator.CreateInstance(CreateType(typeName, cacheKey, useCache));
        }

        /// <summary>Creates an object.</summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <returns>The created object.</returns>
        /// <remarks>Generic version.</remarks>
        public static T CreateObject<T>()
        {
            // dynamically create the object
            return Activator.CreateInstance<T>();
        }

        /// <summary>Creates an object.</summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns>The created object.</returns>
        public static object CreateObject(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static Type CreateType(string typeName)
        {
            return CreateType(typeName, string.Empty, true, false);
        }

        public static Type CreateType(string typeName, bool ignoreErrors)
        {
            return CreateType(typeName, string.Empty, true, ignoreErrors);
        }

        public static Type CreateType(string typeName, string cacheKey, bool useCache)
        {
            return CreateType(typeName, cacheKey, useCache, false);
        }

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

        public static object CreateInstance(Type type)
        {
            return CreateInstance(type, null);
        }

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

        public static void SetProperty(Type type, string propertyName, object target, object[] args)
        {
            if (type != null)
            {
                type.InvokeMember(propertyName, BindingFlags.SetProperty, null, target, args);
            }
        }

        public static void InvokeMethod(Type type, string propertyName, object target, object[] args)
        {
            if (type != null)
            {
                type.InvokeMember(propertyName, BindingFlags.InvokeMethod, null, target, args);
            }
        }

        // dynamically create a default Provider from a ProviderType - this method was used by the CachingProvider to avoid a circular dependency
        [Obsolete("This method has been deprecated. Please use CreateObject(ByVal ObjectProviderType As String, ByVal UseCache As Boolean) As Object. Scheduled removal in v11.0.0.")]
        internal static object CreateObjectNotCached(string objectProviderType)
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
