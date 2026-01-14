// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;
    using System.Linq;

    using Dnn.EditBar.Library;
    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Web.Components.Controllers;

    /// <summary>
    /// Menu item to add an existing module to a page.
    /// </summary>
    [Serializable]
    public class AddExistingModuleMenu : BaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name => "AddExistingModule";

        /// <inheritdoc/>
        public override string Text => "Add Existing Module";

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template => string.Empty;

        /// <inheritdoc/>
        public override string Parent => Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader => "AddExistingModule";

        /// <inheritdoc/>
        public override int Order => 10;

        /// <inheritdoc/>
        public override bool Visible()
        {
            var portalSettings = PortalSettings.Current;
            if (portalSettings == null)
            {
                return false;
            }

            return Personalization.GetUserMode() == PortalSettings.Mode.Edit
                && ControlBarController.Instance.GetCategoryDesktopModules(portalSettings.PortalId, "All", string.Empty).Any();
        }
    }
}
