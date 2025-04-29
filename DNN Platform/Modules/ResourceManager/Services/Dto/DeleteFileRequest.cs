// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

/// <summary>Represents a request to delete a file.</summary>
public class DeleteFileRequest
{
    /// <summary>Gets or sets the id of the file to delete.</summary>
    public int FileId { get; set; }
}
