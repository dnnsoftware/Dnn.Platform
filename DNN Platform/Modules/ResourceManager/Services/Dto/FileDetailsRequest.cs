// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

using System.Runtime.Serialization;

/// <summary>Represents a request to get file details.</summary>
public class FileDetailsRequest
{
    /// <summary>Gets or sets the id of the file.</summary>
    [DataMember(Name = "fileId")]
    public int FileId { get; set; }

    /// <summary>Gets or sets the file name.</summary>
    [DataMember(Name = "fileName")]
    public string FileName { get; set; }

    /// <summary>Gets or sets the file title.</summary>
    [DataMember(Name = "title")]
    public string Title { get; set; }

    /// <summary>Gets or sets the description of the file.</summary>
    [DataMember(Name = "description")]
    public string Description { get; set; }
}
