// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;

    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.UI.Modules;

    [Export(typeof(IToolBarButtonExtensionPoint))]
    [ExportMetadata("Module", "DigitalAssets")]
    [ExportMetadata("Name", "DigitalAssetsToolBarSyncMenuButton")]
    [ExportMetadata("Group", "Main")]
    [ExportMetadata("Priority", 1)]
    public class SyncToolBarMenuButtonExtensionPoint : IToolBarMenuButtonExtensionPoint
    {
        public List<IMenuButtonItemExtensionPoint> Items
        {
            get
            {
                return new List<IMenuButtonItemExtensionPoint>
                {
                    new DefaultMenuButtonItem("Refresh", string.Empty, "first permission_READ permission_BROWSE", LocalizationHelper.GetString("RefreshMenuItemExtensionPoint.Text"), "dnnModule.digitalAssets.refresFolderFromMenu()", string.Empty, 0, string.Empty),
                    new DefaultMenuButtonItem("Sync", string.Empty, "medium permission_MANAGE permission_WRITE", LocalizationHelper.GetString("SyncMenuItemExtensionPoint.Text"), "dnnModule.digitalAssets.syncFromMenu(false)", string.Empty, 0, string.Empty),
                    new DefaultMenuButtonItem("SyncRecursively", string.Empty, "last permission_MANAGE permission_WRITE", LocalizationHelper.GetString("SyncRecursivelyMenuItemExtensionPoint.Text"), "dnnModule.digitalAssets.syncFromMenu(true)", string.Empty, 0, string.Empty),
                };
            }
        }

        public string ButtonId
        {
            get
            {
                return "DigitalAssetsSyncFolderMenuBtnId";
            }
        }

        public string CssClass
        {
            get
            {
                return "rightButton leftAligned permission_READ permission_BROWSE";
            }
        }

        public string MenuCssClass
        {
            get
            {
                return "DigitalAssetsMenuButton";
            }
        }

        public string Action
        {
            get
            {
                return "dnnModule.digitalAssets.onOpeningRefreshMenu()";
            }
        }

        public string AltText
        {
            get
            {
                return LocalizationHelper.GetString("SyncToolBarMenuButtonExtensionPoint.AltText");
            }
        }

        public bool ShowText
        {
            get
            {
                return false;
            }
        }

        public bool ShowIcon
        {
            get
            {
                return true;
            }
        }

        public string Text
        {
            get
            {
                return LocalizationHelper.GetString("SyncToolBarMenuButtonExtensionPoint.Text");
            }
        }

        public string Icon
        {
            get
            {
                return "/DesktopModules/DigitalAssets/Images/down.png";
            }
        }

        public int Order
        {
            get
            {
                return 5;
            }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
