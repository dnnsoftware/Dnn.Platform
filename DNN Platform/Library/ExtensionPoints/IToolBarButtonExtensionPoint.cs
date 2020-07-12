// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using DotNetNuke.UI.Modules;

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
