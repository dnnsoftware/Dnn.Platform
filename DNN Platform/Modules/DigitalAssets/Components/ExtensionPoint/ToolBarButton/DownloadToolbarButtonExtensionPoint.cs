// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.ComponentModel.Composition;

using DotNetNuke.Entities.Icons;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
    //TODO Create the Custom IToolBarButtonExtensionPoint Export attribute
    [Export(typeof(IToolBarButtonExtensionPoint))]
    [ExportMetadata("Module", "DigitalAssets")]
    [ExportMetadata("Name", "DigitalAssetsToolBarButton")]
    [ExportMetadata("Group", "Selection")]
    [ExportMetadata("Priority", 1)]
    public class DownloadToolBarButtonExtensionPoint : IToolBarButtonExtensionPoint
    {
        public string ButtonId
        {
            get { return "DigitalAssetsDownloadBtnId"; }
        }

        public string CssClass
        {
            get { return "DigitalAssetsSelectionToolBar DigitalAssetsDownload permission_READ onlyFiles"; }
        }

        public string Action
        {
            get { return "dnnModule.digitalAssets.download()"; }
        }

        public string AltText
        {
            get { return LocalizationHelper.GetString("DownloadToolBarButtonExtensionPoint.AltText"); }
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
            get { return LocalizationHelper.GetString("DownloadToolBarButtonExtensionPoint.Text"); }
        }

        public string Icon
        {
            get { return IconController.IconURL("FileDownload", "16x16", "Gray"); }
        }

        public int Order
        {
            get { return 1; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
