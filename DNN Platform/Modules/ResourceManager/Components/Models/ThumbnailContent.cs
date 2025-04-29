// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Components.Models;

using System.Net.Http;

/// <summary>Represents the content of a thumbnail.</summary>
public class ThumbnailContent
{
    /// <summary>Gets or sets the thumbnail content as a byte array.</summary>
    public ByteArrayContent Content { get; set; }

    /// <summary>Gets or sets the content type of the thumbnail.</summary>
    public string ContentType { get; set; }

    /// <summary>Gets or sets the thumbnail height.</summary>
    public int Height { get; set; }

    /// <summary>Gets or sets the thumbnail name.</summary>
    public string ThumbnailName { get; set; }

    /// <summary>Gets or sets the thumbnail width.</summary>
    public int Width { get; set; }
}
