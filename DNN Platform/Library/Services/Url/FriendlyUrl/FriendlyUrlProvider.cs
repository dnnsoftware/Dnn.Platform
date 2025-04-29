// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Url.FriendlyUrl;

using System;

using DotNetNuke.Abstractions.Portals;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Internal.SourceGenerators;

public abstract partial class FriendlyUrlProvider
{
    // return the provider
    public static FriendlyUrlProvider Instance()
    {
        return ComponentFactory.GetComponent<FriendlyUrlProvider>();
    }

    public abstract string FriendlyUrl(TabInfo tab, string path);

    public abstract string FriendlyUrl(TabInfo tab, string path, string pageName);

    [DnnDeprecated(9, 4, 3, "Use the IPortalSettings overload")]
    public virtual partial string FriendlyUrl(TabInfo tab, string path, string pageName, PortalSettings settings)
    {
        return this.FriendlyUrl(tab, path, pageName, (IPortalSettings)settings);
    }

    public abstract string FriendlyUrl(TabInfo tab, string path, string pageName, IPortalSettings settings);

    public abstract string FriendlyUrl(TabInfo tab, string path, string pageName, string portalAlias);
}
