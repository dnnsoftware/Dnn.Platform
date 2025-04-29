// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

using System.Runtime.Serialization;

/// <summary>Represents a request for folder details.</summary>
public class FolderDetailsRequest
{
    /// <summary>Gets or sets the id of the folder.</summary>
    [DataMember(Name = "folderId")]
    public int FolderId { get; set; }

    /// <summary>Gets or sets the name of the folder.</summary>
    [DataMember(Name = "folderName")]
    public string FolderName { get; set; }

    /// <summary>Gets or sets the <see cref="Permissions"/>.</summary>
    [DataMember(Name = "permissions")]
    public Permissions Permissions { get; set; }
}
