// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components;

using System.Drawing;

using Dnn.Modules.ResourceManager.Components.Models;

using DotNetNuke.Services.FileSystem;

/// <summary>Manager for Thumbnails on Resource Manager.</summary>
public interface IThumbnailsManager
{
    /// <summary>Checks if a file has an available thumbnail.</summary>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>A value indicating wheter the file is available to get a thumbnail.</returns>
    bool ThumbnailAvailable(string fileName);

    /// <summary>Gets the url of the thumbnail of a file.</summary>
    /// <param name="moduleId">Id of the module.</param>
    /// <param name="fileId">Id of the file.</param>
    /// <param name="width">Width of the thumbnail to generate.</param>
    /// <param name="height">Height of the thumbnail to generate.</param>
    /// <returns>A string containing the url of the requested thumbnail.</returns>
    string ThumbnailUrl(int moduleId, int fileId, int width, int height);

    /// <summary>Gets the url of the thumbnail of a file.</summary>
    /// <param name="moduleId">Id of the module.</param>
    /// <param name="fileId">Id of the file.</param>
    /// <param name="width">Width of the thumbnail to generate.</param>
    /// <param name="height">Height of the thumbnail to generate.</param>
    /// <param name="timestamp">A timestamp string to append to the url for cachebusting.</param>
    /// <returns>A string containing the url of the requested thumbnail.</returns>
    string ThumbnailUrl(int moduleId, int fileId, int width, int height, string timestamp);

    /// <summary>Gets the url of the thumbnail of a file.</summary>
    /// <param name="moduleId">Id of the module.</param>
    /// <param name="fileId">Id of the file.</param>
    /// <param name="width">Width of the thumbnail to generate.</param>
    /// <param name="height">Height of the thumbnail to generate.</param>
    /// <param name="version">The version number of the file.</param>
    /// <returns>A string containing the url of the requested thumbnail.</returns>
    string ThumbnailUrl(int moduleId, int fileId, int width, int height, int version);

    /// <summary>Get the thumbnail from an image.</summary>
    /// <param name="imageUrl">Url of the image.</param>
    /// <param name="width">Width of the thumbnail to generate.</param>
    /// <param name="height">Height of the thumbnail to generate.</param>
    /// <returns>The thumbnail of the image, <see cref="ThumbnailContent"/>.</returns>
    ThumbnailContent GetThumbnailContentFromImageUrl(string imageUrl, int width, int height);

    /// <summary>Get the thumbnail from an image.</summary>
    /// <param name="image">The original image from which to get a thumbnail.</param>
    /// <param name="width">Width of the thumbnail to generate.</param>
    /// <param name="height">Height of the thumbnail to generate.</param>
    /// <param name="crop">If true, will crop the thumbnail image.</param>
    /// <returns>The thumbnail of the image, <see cref="ThumbnailContent"/>.</returns>
    ThumbnailContent GetThumbnailContentFromImage(Image image, int width, int height, bool crop = false);

    /// <summary>Get the thumbnail from a file.</summary>
    /// <param name="item">The file from which to get a thumbnail.</param>
    /// <param name="width">Width of the thumbnail to generate.</param>
    /// <param name="height">Height of the thumbnail to generate.</param>
    /// <param name="crop">If true, will crop the thumbnail image.</param>
    /// <returns>The thumbnail of the file, <see cref="ThumbnailContent"/>.</returns>
    ThumbnailContent GetThumbnailContent(IFileInfo item, int width, int height, bool crop = false);
}
