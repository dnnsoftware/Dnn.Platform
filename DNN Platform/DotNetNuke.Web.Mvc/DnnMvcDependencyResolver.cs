// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DotNetNuke.Services.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// The <see cref="IDependencyResolver"/> implementation used in the
    /// MVC Modules of DNN.
    /// </summary>
    internal class DnnMvcDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public DnnMvcDependencyResolver(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Returns the specified service from the scope.
        /// </summary>
        /// <param name="serviceType">
        /// The service to be retrieved.
        /// </param>
        /// <returns>
        /// The retrieved service.
        /// </returns>
        public object GetService(Type serviceType)
        {
            var accessor = this._serviceProvider.GetRequiredService<IScopeAccessor>();
            var scope = accessor.GetScope();
            if (scope != null)
            {
                return scope.ServiceProvider.GetService(serviceType);
            }

            throw new InvalidOperationException("IServiceScope not provided");
        }

        /// <summary>
        /// Returns the specified services from the scope.
        /// </summary>
        /// <param name="serviceType">
        /// The service to be retrieved.
        /// </param>
        /// <returns>
        /// The retrieved service.
        /// </returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            var accessor = this._serviceProvider.GetRequiredService<IScopeAccessor>();
            var scope = accessor.GetScope();
            if (scope != null)
            {
                return scope.ServiceProvider.GetServices(serviceType);
            }

            throw new InvalidOperationException("IServiceScope not provided");
        }
    }
}
