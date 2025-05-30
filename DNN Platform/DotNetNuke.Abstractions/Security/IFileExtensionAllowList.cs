// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Security;

using System.Collections.Generic;

/// <summary>A list of allowed file extensions.</summary>
public interface IFileExtensionAllowList
{
    /// <summary>Gets the list of extensions in the allow list.</summary>
    /// <remarks>All extensions are lowercase and prefixed with a '.'.</remarks>
    IEnumerable<string> AllowedExtensions { get; }

    /// <summary>Returns a string suitable for display to an end user.</summary>
    /// <returns>A String of the allow list extensions formatted for display to an end user.</returns>
    string ToDisplayString();

    /// <summary>Formats the extension allow list appropriate for display to an end user.</summary>
    /// <param name="additionalExtensions">A list of additionalExtensions to add to the current extensions.</param>
    /// <remarks><paramref name="additionalExtensions"/>case and '.' prefix will be corrected, and duplicates will be excluded from the string.</remarks>
    /// <returns>A String of the allow list extensions formatted for storage display to an end user.</returns>
    string ToDisplayString(IEnumerable<string> additionalExtensions);

    /// <summary>Indicates if the file extension is permitted by the Host allow list.</summary>
    /// <param name="extension">The file extension with or without preceding '.'.</param>
    /// <returns><see langword="true"/> if extension is in allow list or allow list is empty.  <see langword="false"/> otherwise.</returns>
    bool IsAllowedExtension(string extension);

    /// <summary>Indicates if the file extension is permitted by the Host allow list.</summary>
    /// <param name="extension">The file extension with or without preceding '.'.</param>
    /// <param name="additionalExtensions">Any other additional valid extensions or <see langword="null" />.</param>
    /// <returns><see langword="true"/> if extension is in allow list or allow list is empty.  <see langword="false"/> otherwise.</returns>
    bool IsAllowedExtension(string extension, IEnumerable<string> additionalExtensions);

    /// <summary>Formats the extension allow list appropriate for storage in the Host setting.</summary>
    /// <returns>A String of the allow list extensions formatted for storage as a Host setting.</returns>
    string ToStorageString();

    /// <summary>Formats the extension allow list appropriate for storage in the Host setting.</summary>
    /// <param name="additionalExtensions">A list of additionalExtensions to add to the current extensions.</param>
    /// <remarks><paramref name="additionalExtensions"/>case and '.' prefix will be corrected, and duplicates will be excluded from the string.</remarks>
    /// <returns>A String of the allow list extensions formatted for storage as a Host setting.</returns>
    string ToStorageString(IEnumerable<string> additionalExtensions);

    /// <summary>Create a new list which allows any of the extensions from this list that are also in <paramref name="parentList"/> (i.e. the intersection of this list and <paramref name="parentList"/>).</summary>
    /// <param name="parentList">The list to use to restrict this list.</param>
    /// <returns>A new <see cref="IFileExtensionAllowList"/>.</returns>
    IFileExtensionAllowList RestrictBy(IFileExtensionAllowList parentList);
}
