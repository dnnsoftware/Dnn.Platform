// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System;

    public interface IMenuButtonItemExtensionPoint : IExtensionPoint
    {
        string ItemId { get; }

        string Attributes { get; }

        string Type { get; }

        string CssClass { get; }

        string Action { get; }
    }
}
