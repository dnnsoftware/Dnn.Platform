// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

/// <summary>Represents a request to create a new folder.</summary>
public class CreateNewFolderRequest
{
    /// <summary>Gets or sets the new folder name.</summary>
    public string FolderName { get; set; }

    /// <summary>Gets or sets he parent folder id for the new folder.</summary>
    public int ParentFolderId { get; set; }

    /// <summary>Gets or sets the folder mapping id.</summary>
    public int FolderMappingId { get; set; }

    /// <summary>Gets or sets the mapped name.</summary>
    public string MappedName { get; set; }
}
