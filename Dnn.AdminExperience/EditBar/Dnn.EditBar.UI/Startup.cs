// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI;

using System.Linq;

using Dnn.EditBar.Library.Items;

using DotNetNuke.DependencyInjection;
using DotNetNuke.Framework.Reflections;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>Register types for the edit bar.</summary>
public class Startup : IDnnStartup
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services)
    {
        var menuTypes = new TypeLocator().GetAllMatchingTypes(
            t => t is { IsClass: true, IsAbstract: false, } && typeof(BaseMenuItem).IsAssignableFrom(t));
        services.TryAddEnumerable(menuTypes.Select(type => ServiceDescriptor.Transient(typeof(BaseMenuItem), type)));
    }
}
