// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.ExtensionPoints;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Modules.DigitalAssets.Components.ExtensionPoint.ToolBarButton
{
    public class DefaultMenuButtonItem : IMenuButtonItemExtensionPoint
    {
        #region Private fields

        #endregion

        public DefaultMenuButtonItem(string itemId, string itemType, string itemCssClass, string itemText, string itemAction, string itemIcon, int itemOrder, string itemAttributes)
        {
            ItemId = itemId;
            Attributes = itemAttributes;
            Type = itemType;
            Text = itemText;
            Icon = itemIcon;
            Order = itemOrder;
            CssClass = itemCssClass;
            Action = itemAction;
        }

        public string ItemId { get; private set; }

        public string Attributes { get; private set; }

        public string Type { get; private set; }

        public string Text { get; private set; }

        public string Icon { get; private set; }

        public int Order { get; private set; }

        public string Action { get; private set; }

        public string CssClass { get; private set; }

        public bool Visible
        {
            get { return true; }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
