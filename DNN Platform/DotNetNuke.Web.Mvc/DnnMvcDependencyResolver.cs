// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DotNetNuke.Web.Mvc
{
    /// <summary>
    /// The <see cref="IDependencyResolver"/> implementation used in the
    /// MVC Modules of DNN.
    /// </summary>
    internal class DnnMvcDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDependencyResolver _resolver;

        /// <summary>
        /// Instantiate a new instance of the <see cref="DnnDependencyResolver"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> to be used in the <see cref="DnnDependencyResolver"/>
        /// </param>
        public DnnMvcDependencyResolver(IServiceProvider serviceProvider, IDependencyResolver resolver)
        {
            _serviceProvider = serviceProvider;
            _resolver = resolver;
        }

        /// <summary>
        /// Returns the specified service from the scope
        /// </summary>
        /// <param name="serviceType">
        /// The service to be retrieved
        /// </param>
        /// <returns>
        /// The retrieved service
        /// </returns>
        public object GetService(Type serviceType)
        {
            try
            {
                return _serviceProvider.GetService(serviceType);
            }
            catch
            {
                return _resolver.GetService(serviceType);
            }
        }

        /// <summary>
        /// Returns the specified services from the scope
        /// </summary>
        /// <param name="serviceType">
        /// The service to be retrieved
        /// </param>
        /// <returns>
        /// The retrieved service
        /// </returns>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            try
            {
                return _serviceProvider.GetServices(serviceType);
            }
            catch
            {
                return _resolver.GetServices(serviceType);
            }
        }
    }
}
