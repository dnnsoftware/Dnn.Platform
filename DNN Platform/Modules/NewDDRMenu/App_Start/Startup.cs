// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.NewDDRMenu
{
    using DotNetNuke.DependencyInjection;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>Registers DDR Menu services.</summary>
    public class Startup : IDnnStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddScoped<ILocaliser, Localiser>();
            // services.TryAddEnumerable(ServiceDescriptor.Scoped<ILocalisation, Generic>());
        }
    }
}
