// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.UI.WebControls;

namespace DotNetNuke.Web.DDRMenu.Localisation
{
    public interface ILocalisation
    {
        bool HaveApi();
        TabInfo LocaliseTab(TabInfo tab, int portalId);
        DNNNodeCollection LocaliseNodes(DNNNodeCollection nodes);
    }
}
