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

    [Serializable]
    public class AddExistingModuleMenu : BaseMenuItem
    {
        public override string Name { get; } = "AddExistingModule";

        public override string Text
        {
            get
            {
                return "Add Existing Module";
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

        public override string Loader { get; } = "AddExistingModule";

        public override int Order { get; } = 10;

        public override bool Visible()
        {
            return PortalSettings.Current?.UserMode == PortalSettings.Mode.Edit;
        }
    }
}
