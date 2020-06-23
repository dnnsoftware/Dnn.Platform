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
    [ExportMetadata("Name", "ViewPropertiesDigitalAssetsToolBarButton")]
    [ExportMetadata("Group", "Selection")]
    [ExportMetadata("Priority", 1)]
    public class ViewPropertiesSelectionToolBarButtonExtensionPoint : IToolBarButtonExtensionPoint
    {
        public string ButtonId
        {
            get { return "DigitalAssetsViewPropertiesSelectionBtnId"; }
        }

        public string CssClass
        {
            get { return "DigitalAssetsSelectionToolBar permission_READ singleItem"; }
        }

        public string Action
        {
            get { return "dnnModule.digitalAssets.showProperties()"; }
        }

        public string AltText
        {
            get { return LocalizationHelper.GetString("ViewPropertiesSelectionToolBarButtonExtensionPoint.AltText"); }
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
            get { return LocalizationHelper.GetString("ViewPropertiesSelectionToolBarButtonExtensionPoint.Text"); }
        }

        public string Icon
        {
            get
            {
                return IconController.IconURL("ViewProperties", "16x16", "ToolBar");
            }
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
