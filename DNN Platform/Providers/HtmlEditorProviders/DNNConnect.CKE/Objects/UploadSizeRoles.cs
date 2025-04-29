// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Objects;

/// <summary>Upload Size for a Role.</summary>
public class UploadSizeRoles
{
    /// <summary>Gets or sets a value indicating Role ID.</summary>
    public int RoleId { get; set; }

    /// <summary>Gets or sets a value for Role Name.</summary>
    public string RoleName { get; set; }

    /// <summary>Gets or sets a value indicating max upload file size.</summary>
    public int UploadFileLimit { get; set; }
}
