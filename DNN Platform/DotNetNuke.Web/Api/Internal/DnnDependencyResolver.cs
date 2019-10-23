using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;

namespace DotNetNuke.Web.Api.Internal
{
    internal class DnnDependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;
        public DnnDependencyResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IDependencyScope BeginScope()
        {
            return new DnnDependencyResolver(_serviceProvider.CreateScope().ServiceProvider);
        }

        public object GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _serviceProvider.GetServices(serviceType);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            // I don't think we should dispose anything
        }
    }
}
