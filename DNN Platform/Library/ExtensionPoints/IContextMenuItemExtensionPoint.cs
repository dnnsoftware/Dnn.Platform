// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.ExtensionPoints
{
    public interface IContextMenuItemExtensionPoint : IExtensionPoint
    {
        string CtxMenuItemId { get; }

        string CssClass { get; }

        string Action { get; }

        string AltText { get; }

        bool ShowText { get; }

        bool ShowIcon { get; }
    }
}
