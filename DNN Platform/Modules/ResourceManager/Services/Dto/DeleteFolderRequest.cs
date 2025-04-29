// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

/// <summary>Represents a request to delete a folder.</summary>
public class DeleteFolderRequest
{
    /// <summary>Gets or sets the id of the folder to delete.</summary>
    public int FolderId { get; set; }

    /// <summary>Gets or sets a value indicating whether an unlink is allowed.</summary>
    public bool UnlinkAllowedStatus { get; set; }
}
