﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class SearchBoxControl : PortalModuleBase
    {
        protected override void OnInit(EventArgs e)
        {
            ClientResourceManager.RegisterScript(Page, "~/DesktopModules/DigitalAssets/ClientScripts/dnn.DigitalAssets.SearchBox.js", FileOrder.Js.DefaultPriority + 10);
        }
    }
}
