// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
    using System;

    using DotNetNuke.ExtensionPoints;
    using DotNetNuke.UI.Modules;

    public class DefaultMenuButtonItem : IMenuButtonItemExtensionPoint
    {
        public DefaultMenuButtonItem(string itemId, string itemType, string itemCssClass, string itemText, string itemAction, string itemIcon, int itemOrder, string itemAttributes)
        {
            this.ItemId = itemId;
            this.Attributes = itemAttributes;
            this.Type = itemType;
            this.Text = itemText;
            this.Icon = itemIcon;
            this.Order = itemOrder;
            this.CssClass = itemCssClass;
            this.Action = itemAction;
        }

        public bool Visible
        {
            get { return true; }
        }

        public string ItemId { get; private set; }

        public string Attributes { get; private set; }

        public string Type { get; private set; }

        public string Text { get; private set; }

        public string Icon { get; private set; }

        public int Order { get; private set; }

        public string Action { get; private set; }

        public string CssClass { get; private set; }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
