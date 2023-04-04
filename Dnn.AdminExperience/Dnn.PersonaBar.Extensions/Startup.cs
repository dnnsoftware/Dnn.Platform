// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions;

using Dnn.PersonaBar.Prompt.Components.Repositories;

using DotNetNuke.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>Registers services for the Persona Bar Extensions.</summary>
public class Startup : IDnnStartup
{
    /// <inheritdoc/>
    public void ConfigureServices(IServiceCollection services)
    {
#pragma warning disable CS0618
        services.AddTransient<ICommandRepository, CommandRepository>();
#pragma warning restore CS0618
    }
}
