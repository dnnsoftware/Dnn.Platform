// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
    using System;
    using System.ComponentModel.Composition;

    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.UI.Modules;

    // TODO Create the Custom IToolBarButtonExtensionPoint Export attribute
    [Export(typeof(IToolBarButtonExtensionPoint))]
    [ExportMetadata("Module", "DigitalAssets")]
    [ExportMetadata("Name", "DigitalAssetsToolBarButton")]
    [ExportMetadata("Group", "Main")]
    [ExportMetadata("Priority", 1)]
    public class ManageFolderTypesToolBarButtonExtensionPoint : IToolBarButtonExtensionPoint
    {
        public string ButtonId
        {
            get { return "DigitalAssetsManageFolderTypesBtnId"; }
        }

        public string CssClass
        {
            get { return "rightButton split leftAligned folderRequired permission_ADD"; }
        }

        public string Action
        {
            get
            {
                if (this.ModuleContext == null)
                {
                    return string.Empty;
                }

                if (PortalSettings.Current.EnablePopUps)
                {
                    return this.ModuleContext.EditUrl("FolderMappings");
                }
                else
                {
                    return string.Format("location.href = '{0}';", this.ModuleContext.EditUrl("FolderMappings"));
                }
            }
        }

        public string AltText
        {
            get { return LocalizationHelper.GetString("ManageFolderTypesToolBarButtonExtensionPoint.AltText"); }
        }

        public bool ShowText
        {
            get { return false; }
        }

        public bool ShowIcon
        {
            get { return true; }
        }

        public string Text
        {
            get { return LocalizationHelper.GetString("ManageFolderTypesToolBarButtonExtensionPoint.Text"); }
        }

        public string Icon
        {
            get { return "~/DesktopModules/DigitalAssets/Images/manageFolderTypes.png"; }
        }

        public int Order
        {
            get { return 8; }
        }

        public bool Enabled
        {
            get
            {
                return this.ModuleContext != null
                            && ModulePermissionController.CanManageModule(this.ModuleContext.Configuration);
            }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
