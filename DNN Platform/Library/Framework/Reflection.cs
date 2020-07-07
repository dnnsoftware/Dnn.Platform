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

    /// -----------------------------------------------------------------------------
    /// Namespace: DotNetNuke.Framework
    /// Project  : DotNetNuke
    /// Class    : Reflection
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Library responsible for reflection.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class Reflection
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Reflection));

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="ObjectProviderType">The type of Object to create (data/navigation).</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider configured in web.config.</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string ObjectProviderType)
        {
            return CreateObject(ObjectProviderType, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="ObjectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="UseCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider configured in web.config.</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string ObjectProviderType, bool UseCache)
        {
            return CreateObject(ObjectProviderType, string.Empty, string.Empty, string.Empty, UseCache);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="ObjectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="ObjectNamespace">The namespace of the object to create.</param>
        /// <param name="ObjectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace and
        /// AssemblyName ( this allows derived providers to share the same config ).</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string ObjectProviderType, string ObjectNamespace, string ObjectAssemblyName)
        {
            return CreateObject(ObjectProviderType, string.Empty, ObjectNamespace, ObjectAssemblyName, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="ObjectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="ObjectNamespace">The namespace of the object to create.</param>
        /// <param name="ObjectAssemblyName">The assembly of the object to create.</param>
        /// <param name="UseCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace and
        /// AssemblyName ( this allows derived providers to share the same config ).</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string ObjectProviderType, string ObjectNamespace, string ObjectAssemblyName, bool UseCache)
        {
            return CreateObject(ObjectProviderType, string.Empty, ObjectNamespace, ObjectAssemblyName, UseCache);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="ObjectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="ObjectProviderName">The name of the Provider.</param>
        /// <param name="ObjectNamespace">The namespace of the object to create.</param>
        /// <param name="ObjectAssemblyName">The assembly of the object to create.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace,
        /// AssemblyName and ProviderName.</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string ObjectProviderType, string ObjectProviderName, string ObjectNamespace, string ObjectAssemblyName)
        {
            return CreateObject(ObjectProviderType, ObjectProviderName, ObjectNamespace, ObjectAssemblyName, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="ObjectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="ObjectProviderName">The name of the Provider.</param>
        /// <param name="ObjectNamespace">The namespace of the object to create.</param>
        /// <param name="ObjectAssemblyName">The assembly of the object to create.</param>
        /// <param name="UseCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace,
        /// AssemblyName and ProviderName.</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string ObjectProviderType, string ObjectProviderName, string ObjectNamespace, string ObjectAssemblyName, bool UseCache)
        {
            return CreateObject(ObjectProviderType, ObjectProviderName, ObjectNamespace, ObjectAssemblyName, UseCache, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="ObjectProviderType">The type of Object to create (data/navigation).</param>
        /// <param name="ObjectProviderName">The name of the Provider.</param>
        /// <param name="ObjectNamespace">The namespace of the object to create.</param>
        /// <param name="ObjectAssemblyName">The assembly of the object to create.</param>
        /// <param name="UseCache">Caching switch.</param>
        /// <param name="fixAssemblyName">Whether append provider name as part of the assembly name.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload for creating an object from a Provider including NameSpace,
        /// AssemblyName and ProviderName.</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string ObjectProviderType, string ObjectProviderName, string ObjectNamespace, string ObjectAssemblyName, bool UseCache, bool fixAssemblyName)
        {
            string TypeName = string.Empty;

            // get the provider configuration based on the type
            ProviderConfiguration objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration(ObjectProviderType);
            if (!string.IsNullOrEmpty(ObjectNamespace) && !string.IsNullOrEmpty(ObjectAssemblyName))
            {
                // if both the Namespace and AssemblyName are provided then we will construct an "assembly qualified typename" - ie. "NameSpace.ClassName, AssemblyName"
                if (string.IsNullOrEmpty(ObjectProviderName))
                {
                    // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                    TypeName = ObjectNamespace + "." + objProviderConfiguration.DefaultProvider + ", " + ObjectAssemblyName + (fixAssemblyName ? "." + objProviderConfiguration.DefaultProvider : string.Empty);
                }
                else
                {
                    // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                    TypeName = ObjectNamespace + "." + ObjectProviderName + ", " + ObjectAssemblyName + (fixAssemblyName ? "." + ObjectProviderName : string.Empty);
                }
            }
            else
            {
                // if only the Namespace is provided then we will construct an "full typename" - ie. "NameSpace.ClassName"
                if (!string.IsNullOrEmpty(ObjectNamespace))
                {
                    if (string.IsNullOrEmpty(ObjectProviderName))
                    {
                        // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                        TypeName = ObjectNamespace + "." + objProviderConfiguration.DefaultProvider;
                    }
                    else
                    {
                        // dynamically create the typename from the constants ( this enables private assemblies to share the same configuration as the base provider )
                        TypeName = ObjectNamespace + "." + ObjectProviderName;
                    }
                }
                else
                {
                    // if neither Namespace or AssemblyName are provided then we will get the typename from the default provider
                    if (string.IsNullOrEmpty(ObjectProviderName))
                    {
                        // get the typename of the default Provider from web.config
                        TypeName = ((Provider)objProviderConfiguration.Providers[objProviderConfiguration.DefaultProvider]).Type;
                    }
                    else
                    {
                        // get the typename of the specified ProviderName from web.config
                        TypeName = ((Provider)objProviderConfiguration.Providers[ObjectProviderName]).Type;
                    }
                }
            }

            return CreateObject(TypeName, TypeName, UseCache);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="TypeName">The fully qualified TypeName.</param>
        /// <param name="CacheKey">The Cache Key.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload that takes a fully-qualified typename and a Cache Key.</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string TypeName, string CacheKey)
        {
            return CreateObject(TypeName, CacheKey, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="TypeName">The fully qualified TypeName.</param>
        /// <param name="CacheKey">The Cache Key.</param>
        /// <param name="UseCache">Caching switch.</param>
        /// <returns>The created Object.</returns>
        /// <remarks>Overload that takes a fully-qualified typename and a Cache Key.</remarks>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(string TypeName, string CacheKey, bool UseCache)
        {
            return Activator.CreateInstance(CreateType(TypeName, CacheKey, UseCache));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <typeparam name="T">The type of object to create.</typeparam>
        /// <returns></returns>
        /// <remarks>Generic version.</remarks>
        /// -----------------------------------------------------------------------------
        public static T CreateObject<T>()
        {
            // dynamically create the object
            return Activator.CreateInstance<T>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates an object.
        /// </summary>
        /// <param name="type">The type of object to create.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static object CreateObject(Type type)
        {
            return Activator.CreateInstance(type);
        }

        public static Type CreateType(string TypeName)
        {
            return CreateType(TypeName, string.Empty, true, false);
        }

        public static Type CreateType(string TypeName, bool IgnoreErrors)
        {
            return CreateType(TypeName, string.Empty, true, IgnoreErrors);
        }

        public static Type CreateType(string TypeName, string CacheKey, bool UseCache)
        {
            return CreateType(TypeName, CacheKey, UseCache, false);
        }

        public static Type CreateType(string TypeName, string CacheKey, bool UseCache, bool IgnoreErrors)
        {
            if (string.IsNullOrEmpty(CacheKey))
            {
                CacheKey = TypeName;
            }

            Type type = null;

            // use the cache for performance
            if (UseCache)
            {
                type = (Type)DataCache.GetCache(CacheKey);
            }

            // is the type in the cache?
            if (type == null)
            {
                try
                {
                    // use reflection to get the type of the class
                    type = BuildManager.GetType(TypeName, true, true);
                    if (UseCache)
                    {
                        // insert the type into the cache
                        DataCache.SetCache(CacheKey, type);
                    }
                }
                catch (Exception exc)
                {
                    if (!IgnoreErrors)
                    {
                        Logger.Error(TypeName, exc);
                    }
                }
            }

            return type;
        }

        public static object CreateInstance(Type Type)
        {
            if (Type != null)
            {
                return Type.InvokeMember(string.Empty, BindingFlags.CreateInstance, null, null, null, null);
            }
            else
            {
                return null;
            }
        }

        public static object GetProperty(Type Type, string PropertyName, object Target)
        {
            if (Type != null)
            {
                return Type.InvokeMember(PropertyName, BindingFlags.GetProperty, null, Target, null);
            }
            else
            {
                return null;
            }
        }

        public static void SetProperty(Type Type, string PropertyName, object Target, object[] Args)
        {
            if (Type != null)
            {
                Type.InvokeMember(PropertyName, BindingFlags.SetProperty, null, Target, Args);
            }
        }

        public static void InvokeMethod(Type Type, string PropertyName, object Target, object[] Args)
        {
            if (Type != null)
            {
                Type.InvokeMember(PropertyName, BindingFlags.InvokeMethod, null, Target, Args);
            }
        }

        // dynamically create a default Provider from a ProviderType - this method was used by the CachingProvider to avoid a circular dependency
        [Obsolete("This method has been deprecated. Please use CreateObject(ByVal ObjectProviderType As String, ByVal UseCache As Boolean) As Object. Scheduled removal in v11.0.0.")]
        internal static object CreateObjectNotCached(string ObjectProviderType)
        {
            string TypeName = string.Empty;
            Type objType = null;

            // get the provider configuration based on the type
            ProviderConfiguration objProviderConfiguration = ProviderConfiguration.GetProviderConfiguration(ObjectProviderType);

            // get the typename of the Base DataProvider from web.config
            TypeName = ((Provider)objProviderConfiguration.Providers[objProviderConfiguration.DefaultProvider]).Type;
            try
            {
                // use reflection to get the type of the class
                objType = BuildManager.GetType(TypeName, true, true);
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
