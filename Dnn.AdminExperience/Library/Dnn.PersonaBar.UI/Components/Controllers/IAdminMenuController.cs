// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Components.Controllers
{
    using DotNetNuke.Entities.Tabs;

    public interface IAdminMenuController
    {
        void CreateLinkMenu(TabInfo tab);

        void DeleteLinkMenu(TabInfo tab);
    }
}
