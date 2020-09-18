// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Service provider implementation for <see cref="HttpRuntime.WebObjectActivator"/>.
    /// </summary>
    internal class RequestScopeServiceProvider : IServiceProvider
    {
        private const int TypesCannotResolveCacheCap = 100000;
        private readonly ConcurrentDictionary<Type, bool> typesCannotResolve = new ConcurrentDictionary<Type, bool>();

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            if (this.typesCannotResolve.ContainsKey(serviceType))
            {
                return DefaultCreateInstance(serviceType);
            }

            var provider = HttpContextSource.Current?.GetScope()?.ServiceProvider;

            if (provider == null)
            {
                return DefaultCreateInstance(serviceType);
            }

            var instance = provider.GetService(serviceType);

            if (instance != null)
            {
                return instance;
            }

            try
            {
                return ActivatorUtilities.CreateInstance(provider, serviceType);
            }
            catch (InvalidOperationException)
            {
                // Thrown by "ActivatorUtilities.CreateInstance" when there is no public constructor available.
                // Some ASP.NET modules have private constructors, so need to use the default activator instead.
                if (this.typesCannotResolve.Count < TypesCannotResolveCacheCap)
                {
                    this.typesCannotResolve.TryAdd(serviceType, true);
                }

                return DefaultCreateInstance(serviceType);
            }
        }

        /// <summary>
        /// Create a instance of the given type with <see cref="Activator"/>
        /// Used when there is no service provider or valid constructor available.
        /// </summary>
        /// <param name="serviceType">Requested service type.</param>
        /// <returns>New instance of the service type.</returns>
        private static object DefaultCreateInstance(Type serviceType)
        {
            return Activator.CreateInstance(
                serviceType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.CreateInstance,
                null,
                null,
                null);
        }
    }
}
