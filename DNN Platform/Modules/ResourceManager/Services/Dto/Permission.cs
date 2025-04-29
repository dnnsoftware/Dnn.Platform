// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services.Dto;

using System.Runtime.Serialization;

/// <summary>Defines one specific permission.</summary>
[DataContract]
public class Permission
{
    /// <summary>Gets or sets the id of the permission.</summary>
    [DataMember(Name = "permissionId")]
    public int PermissionId { get; set; }

    /// <summary>Gets or sets the permission name.</summary>
    [DataMember(Name = "permissionName")]
    public string PermissionName { get; set; }

    /// <summary>Gets or sets the permission key.</summary>
    [DataMember(Name = "permissionKey")]
    public string PermissionKey { get; set; }

    /// <summary>Gets or sets the permission code.</summary>
    [DataMember(Name = "permissionCode")]
    public string PermissionCode { get; set; }

    /// <summary>Gets or sets a value indicating whether the target of the permission has full control over the resource.</summary>
    [DataMember(Name = "fullControl")]
    public bool FullControl { get; set; }

    /// <summary>Gets or sets a value indicating whether the target of the permission can view the resource.</summary>
    [DataMember(Name = "view")]
    public bool View { get; set; }

    /// <summary>Gets or sets a value indicating whether the target of the permission has access to the resource.</summary>
    [DataMember(Name = "allowAccess")]
    public bool AllowAccess { get; set; }
}
