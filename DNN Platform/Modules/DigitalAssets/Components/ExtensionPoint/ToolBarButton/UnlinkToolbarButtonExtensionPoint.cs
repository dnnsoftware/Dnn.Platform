// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
    using System;
    using System.ComponentModel.Composition;

    using DotNetNuke.Entities.Icons;
    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.UI.Modules;

    // TODO Create the Custom IToolBarButtonExtensionPoint Export attribute
    [Export(typeof(IToolBarButtonExtensionPoint))]
    [ExportMetadata("Module", "DigitalAssets")]
    [ExportMetadata("Name", "DigitalAssetsUnlinkToolBarButton")]
    [ExportMetadata("Group", "Selection")]
    [ExportMetadata("Priority", 1)]
    public class UnlinkToolBarButtonExtensionPoint : IToolBarButtonExtensionPoint
    {
        public string ButtonId
        {
            get { return "DigitalAssetsUnlinkBtnId"; }
        }

        public string CssClass
        {
            get { return "DigitalAssetsSelectionToolBar DigitalAssetsUnlink permission_DELETE onlyFolders singleItem"; }
        }

        public string Action
        {
            get { return "dnnModule.digitalAssets.unlinkSelectedItems()"; }
        }

        public string AltText
        {
            get { return LocalizationHelper.GetString("UnlinkToolBarButtonExtensionPoint.AltText"); }
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
            get { return LocalizationHelper.GetString("UnlinkToolBarButtonExtensionPoint.Text"); }
        }

        public string Icon
        {
            get { return IconController.IconURL("UnLink", "16x16", "Gray"); }
        }

        public int Order
        {
            get { return 6; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
