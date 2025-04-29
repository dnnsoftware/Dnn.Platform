// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Razor.Helpers;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.UI.Modules;

[DnnDeprecated(9, 3, 2, "Use Razor Pages instead")]
public partial class DnnHelper
{
    private readonly ModuleInstanceContext context;

    public DnnHelper(ModuleInstanceContext context)
    {
        this.context = context;
    }

    public ModuleInfo Module
    {
        get
        {
            return this.context.Configuration;
        }
    }

    public TabInfo Tab
    {
        get
        {
            return this.context.PortalSettings.ActiveTab;
        }
    }

    public PortalSettings Portal
    {
        get
        {
            return this.context.PortalSettings;
        }
    }

    public UserInfo User
    {
        get
        {
            return this.context.PortalSettings.UserInfo;
        }
    }
}
