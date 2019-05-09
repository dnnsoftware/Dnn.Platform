using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.DependencyInjection
{
    public interface IServiceRegistration
    {
        void ConfigureServices(IServiceCollection services);
    }
}
