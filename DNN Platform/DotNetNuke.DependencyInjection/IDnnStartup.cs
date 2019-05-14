using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.DependencyInjection
{
    /// <summary>
    /// An interface for adding extension points to the DNN Startup Logic.
    /// </summary>
    public interface IDnnStartup
    {
        /// <summary>
        /// Configure additional services for the host or web application.
        /// This method will be called during the Application Startup phase
        /// and services will be available anywhere in the application.
        /// </summary>
        /// <param name="services">
        /// Service Collection used to registering services in the container.
        /// </param>
        void ConfigureServices(IServiceCollection services);
    }
}
