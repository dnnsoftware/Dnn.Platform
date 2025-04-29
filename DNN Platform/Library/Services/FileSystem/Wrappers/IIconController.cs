// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal;

/// <summary>A contract specifying the ability to get the URL of icons.</summary>
internal interface IIconController
{
    /// <summary>Gets the URL of the icon with the given <paramref name="key"/>.</summary>
    /// <param name="key">The icon key.</param>
    /// <returns>The URL.</returns>
    string IconURL(string key);

    /// <summary>Gets the URL of the icon with the given <paramref name="key"/>.</summary>
    /// <param name="key">The icon key.</param>
    /// <param name="size">The icon size, e.g. <see cref="DotNetNuke.Entities.Icons.IconController.DefaultIconSize"/>.</param>
    /// <returns>The URL.</returns>
    string IconURL(string key, string size);
}
