// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Services.Dto;

/// <summary>Represents a request to move a folder to another folder.</summary>
public class MoveFolderRequest
{
    /// <summary>Gets or sets the id of the folder to move.</summary>
    public int SourceFolderId { get; set; }

    /// <summary>Gets or sets the id of the folder into which to move the source folder.</summary>
    public int DestinationFolderId { get; set; }
}
