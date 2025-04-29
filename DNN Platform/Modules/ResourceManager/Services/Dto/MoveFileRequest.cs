// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Services.Dto;

/// <summary>Represents a request to move a file into a folder.</summary>
public class MoveFileRequest
{
    /// <summary>Gets or sets the id of the file to move.</summary>
    public int SourceFileId { get; set; }

    /// <summary>Gets or sets the id of the folder to move the file into.</summary>
    public int DestinationFolderId { get; set; }
}
