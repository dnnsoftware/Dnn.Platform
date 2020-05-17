// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Tabs;

namespace Dnn.PersonaBar.UI.Components.Controllers
{
    public interface IAdminMenuController
    {
        void CreateLinkMenu(TabInfo tab);
        void DeleteLinkMenu(TabInfo tab);
    }
}
