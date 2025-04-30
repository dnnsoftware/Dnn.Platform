// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;
    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;

    /// <summary>
    /// Menu item to edit the current page settings.
    /// </summary>
    [Serializable]
    public class PageSettingsMenu : BaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name => "PageSettings";

        /// <inheritdoc/>
        public override string Text => "PageSettings";

        /// <inheritdoc/>
        public override string Parent => Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader => "PageSettings";

        /// <inheritdoc/>
        public override int Order => 15;

        /// <inheritdoc/>
        public override bool Visible()
        {
            var isInEditMode = Personalization.GetUserMode() == PortalSettings.Mode.Edit;
            var isCurrentControlPanel = Host.ControlPanel.EndsWith("PersonaBarContainer.ascx", StringComparison.InvariantCultureIgnoreCase);
            var canEditPageSettings = this.CanEditPageSettings();

            return isInEditMode && isCurrentControlPanel && canEditPageSettings;
        }

        private bool CanEditPageSettings()
        {
            return
                TabPermissionController.CanAddPage() ||
                TabPermissionController.CanAdminPage() ||
                TabPermissionController.CanCopyPage() ||
                TabPermissionController.CanDeletePage() ||
                TabPermissionController.CanExportPage() ||
                TabPermissionController.CanImportPage() ||
                TabPermissionController.CanManagePage();
        }
    }
}
