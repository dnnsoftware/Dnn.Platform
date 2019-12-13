// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.ExtensionPoints
{
    public interface IMenuButtonItemExtensionPoint : IExtensionPoint
    {
        string ItemId { get; }

        string Attributes { get; }

        string Type { get; }

        string CssClass { get; }

        string Action { get; }
    }
}
