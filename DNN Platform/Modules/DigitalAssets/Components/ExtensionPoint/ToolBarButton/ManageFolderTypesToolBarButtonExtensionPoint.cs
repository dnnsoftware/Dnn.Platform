// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.ComponentModel.Composition;

using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.Security.Permissions;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
    //TODO Create the Custom IToolBarButtonExtensionPoint Export attribute
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
                if (ModuleContext == null)
                {
                    return string.Empty;
                }

	            if (PortalSettings.Current.EnablePopUps)
	            {
		            return ModuleContext.EditUrl("FolderMappings");
	            }
	            else
	            {
		            return string.Format("location.href = '{0}';", ModuleContext.EditUrl("FolderMappings"));
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
            get { return ModuleContext != null
                            && ModulePermissionController.CanManageModule(ModuleContext.Configuration); }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
