// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Common
{
    using System;
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
        private const BindingFlags ActivatorFlags =
            BindingFlags.Instance |
            BindingFlags.NonPublic |
            BindingFlags.Public |
            BindingFlags.CreateInstance;

        /// <inheritdoc />
        public object GetService(Type serviceType)
        {
            var provider = HttpContextSource.Current?.GetScope()?.ServiceProvider;

            return provider != null && (serviceType.IsInterface || serviceType.GetConstructors().Length > 0)
                ? ActivatorUtilities.GetServiceOrCreateInstance(provider, serviceType)
                : Activator.CreateInstance(serviceType, ActivatorFlags, null, null, null);
        }
    }
}
