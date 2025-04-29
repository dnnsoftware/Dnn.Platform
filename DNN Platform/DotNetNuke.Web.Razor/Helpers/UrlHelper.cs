// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor.Helpers;

using System;

using DotNetNuke.Abstractions;
using DotNetNuke.Common;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.UI.Modules;
using Microsoft.Extensions.DependencyInjection;

[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public partial class UrlHelper
{
    private readonly ModuleInstanceContext context;

    /// <summary>Initializes a new instance of the <see cref="UrlHelper"/> class.</summary>
    /// <param name="context">The module context.</param>
    public UrlHelper(ModuleInstanceContext context)
    {
        this.context = context;
        this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
    }

    protected INavigationManager NavigationManager { get; }

    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial string NavigateToControl()
    {
        return this.NavigationManager.NavigateURL(this.context.TabId);
    }

    [DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
    public partial string NavigateToControl(string controlKey)
    {
        return this.NavigationManager.NavigateURL(this.context.TabId, controlKey, "mid=" + this.context.ModuleId);
    }
}
