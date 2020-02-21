// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.UI.Modules;

namespace DotNetNuke.ExtensionPoints
{
    public interface IToolBarButtonExtensionPoint : IExtensionPoint
    {
        string ButtonId { get; }

        string CssClass { get; }

        string Action { get; }

        string AltText { get; }

        bool ShowText { get; }

        bool ShowIcon { get; }

        bool Enabled { get; }

        ModuleInstanceContext ModuleContext { get; set; }
    }
}
