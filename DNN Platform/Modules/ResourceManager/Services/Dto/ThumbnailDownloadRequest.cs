// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

/// <summary>Represents a request to download a thumbnail.</summary>
public class ThumbnailDownloadRequest
{
    /// <summary>Gets or sets the id of the file.</summary>
    public int FileId { get; set; }

    /// <summary>Gets or sets the width of the thumbnail.</summary>
    public int Width { get; set; }

    /// <summary>Gets or sets the height of the thumbnail.</summary>
    public int Height { get; set; }

    /// <summary>Gets or sets an optional version number.</summary>
    public int? Version { get; set; }
}
