using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.DependencyInjection
{
    public interface IDnnStartup
    {
        void ConfigureServices(IServiceCollection services);
    }
}
