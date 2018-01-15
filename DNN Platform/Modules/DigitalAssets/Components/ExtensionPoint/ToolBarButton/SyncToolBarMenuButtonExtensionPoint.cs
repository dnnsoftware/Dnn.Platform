#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Collections.Generic;
using System.ComponentModel.Composition;

using DotNetNuke.ExtensionPoints;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
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
                    new DefaultMenuButtonItem("Refresh", "", "first permission_READ permission_BROWSE", LocalizationHelper.GetString("RefreshMenuItemExtensionPoint.Text"), "dnnModule.digitalAssets.refresFolderFromMenu()", "", 0, ""), 
                    new DefaultMenuButtonItem("Sync", "", "medium permission_MANAGE permission_WRITE", LocalizationHelper.GetString("SyncMenuItemExtensionPoint.Text"), "dnnModule.digitalAssets.syncFromMenu(false)", "", 0, ""), 
                    new DefaultMenuButtonItem("SyncRecursively", "", "last permission_MANAGE permission_WRITE", LocalizationHelper.GetString("SyncRecursivelyMenuItemExtensionPoint.Text"), "dnnModule.digitalAssets.syncFromMenu(true)", "", 0, "")
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