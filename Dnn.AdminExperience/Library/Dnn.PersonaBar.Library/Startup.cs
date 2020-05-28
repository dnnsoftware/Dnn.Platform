// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
