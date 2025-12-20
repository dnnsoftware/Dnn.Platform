// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library
{
    using Dnn.PersonaBar.Library.Containers;
    using Dnn.PersonaBar.Library.Controllers;

    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Users;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Register services used by the Persona Bar library.</summary>
    internal sealed class Startup : IDnnStartup
    {
        /// <inheritdoc/>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IPersonaBarContainer, PersonaBarContainer>();
            services.AddSingleton<IPersonaBarController, PersonaBarController>();
            services.AddSingleton<IPersonaBarUserSettingsController, PersonaBarUserSettingsController>();
        }
    }
}
