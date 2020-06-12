using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Web.Common
{
    public class LazyServiceProvider : IServiceProvider
    {
        private IServiceProvider _serviceProvider;

        public object GetService(Type serviceType)
        {
            if (this._serviceProvider is null)
            {
                throw new Exception("Cannot resolve services until the service provider is built.");
            }

            return this._serviceProvider.GetService(serviceType);
        }

        internal void SetProvider(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
    }
}
