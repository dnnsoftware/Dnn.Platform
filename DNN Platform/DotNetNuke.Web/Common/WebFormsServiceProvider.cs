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
    internal class WebFormsServiceProvider : IServiceProvider
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

            var service = provider?.GetService(serviceType);

            if (service != null)
            {
                return service;
            }

            // "ActivatorUtilities.CreateInstance" will throw an exception if there are only constructors that are not public,
            // which is the case for some of the types in ASP.NET WebForms. So we need to check if there are any public constructors.
            if (provider != null && serviceType.GetConstructors().Length > 0)
            {
                return ActivatorUtilities.CreateInstance(provider, serviceType);
            }

            return Activator.CreateInstance(serviceType, ActivatorFlags, null, null, null);
        }
    }
}
