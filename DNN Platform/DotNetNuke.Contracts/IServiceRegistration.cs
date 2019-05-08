using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace DotNetNuke.Contracts
{
    // make internal
    public interface IInternalServiceRegistration
    {
        IEnumerable<KeyValuePair<Type, Type>> GetServiceRegistrations();
    }
    public interface IServiceRegistration
    {
        void ConfigureServices(IServiceCollection services);
    }
}
