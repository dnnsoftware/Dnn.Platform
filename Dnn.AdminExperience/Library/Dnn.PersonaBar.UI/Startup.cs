// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI;

using Dnn.PersonaBar.UI.Components.Controllers;
using DotNetNuke.DependencyInjection;
using DotNetNuke.Services.FileSystem.Internal;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>The <see cref="IDnnStartup"/> instance for the Persona Bar UI project.</summary>
public class Startup : IDnnStartup
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
        services.TryAddTransient<IDirectory, DirectoryWrapper>();
        services.AddTransient<IAdminMenuController, AdminMenuController>();
    }
}
