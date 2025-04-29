// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.EditBar.Library;

using System;
using System.Linq;

/// <summary>Constants used by the Edit Bar.</summary>
public static class Constants
{
    /// <summary>The virtual-rooted path to the Edit Bar folder.</summary>
    public const string EditBarRelativePath = "~/DesktopModules/admin/Dnn.EditBar";

    /// <summary>The name of the parent for the menu items on the left.</summary>
    public const string LeftMenu = "LeftMenu";

    /// <summary>The name of the parent for the menu items on the right.</summary>
    public const string RightMenu = "RightMenu";

    /// <summary>The cache key for the Edit Bar menu items.</summary>
    public const string MenuItemsCacheKey = "Dnn.EditBar.MenuItems";
}
