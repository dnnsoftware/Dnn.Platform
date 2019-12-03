using Dnn.PersonaBar.Library.Containers;
using DotNetNuke.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Dnn.PersonaBar.Library
{
    class Startup : IDnnStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPersonaBarContainer, PersonaBarContainer>();
        }
    }
}
