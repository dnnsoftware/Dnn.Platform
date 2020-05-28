// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
