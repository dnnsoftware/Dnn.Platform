﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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