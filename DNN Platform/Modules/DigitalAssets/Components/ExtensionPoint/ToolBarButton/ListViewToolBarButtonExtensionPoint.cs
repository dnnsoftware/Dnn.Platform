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

    [Export(typeof(IToolBarButtonExtensionPoint))]
    [ExportMetadata("Module", "DigitalAssets")]
    [ExportMetadata("Name", "DigitalAssetsToolBarButton")]
    [ExportMetadata("Group", "Main")]
    [ExportMetadata("Priority", 1)]
    public class ListViewToolBarButtonExtensionPoint : IToolBarButtonExtensionPoint
    {
        public string ButtonId
        {
            get { return "DigitalAssetsListViewBtnId"; }
        }

        public string CssClass
        {
            get { return "DigitalAssetsListView middleButton split leftAligned"; }
        }

        public string Action
        {
            get { return @"dnnModule.digitalAssets.setView(""listview"")"; }
        }

        public string AltText
        {
            get { return LocalizationHelper.GetString("ListViewToolBarButtonExtensionPoint.AltText"); }
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
            get { return LocalizationHelper.GetString("ListViewToolBarButtonExtensionPoint.Text"); }
        }

        public string Icon
        {
            get { return IconController.IconURL("ThumbView", "16x16", "Gray"); }
        }

        public int Order
        {
            get { return 2; }
        }

        public bool Enabled
        {
            get { return true; }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
