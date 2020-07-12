// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Dnn.EditBar.Library;
    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Web.Components.Controllers;

    [Serializable]
    public class AddModuleMenu : BaseMenuItem
    {
        public override string Name { get; } = "AddModule";

        public override string Text
        {
            get
            {
                return "Add Module";
            }
        }

        public override string CssClass
        {
            get
            {
                return string.Empty;
            }
        }

        public override string Template { get; } = string.Empty;

        public override string Parent { get; } = Constants.LeftMenu;

        public override string Loader { get; } = "AddModule";

        public override int Order { get; } = 5;

        public override bool Visible()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return false;
            }

            return portalSettings.UserMode == PortalSettings.Mode.Edit
                && ControlBarController.Instance.GetCategoryDesktopModules(portalSettings.PortalId, "All", string.Empty).Any();
        }
    }
}
