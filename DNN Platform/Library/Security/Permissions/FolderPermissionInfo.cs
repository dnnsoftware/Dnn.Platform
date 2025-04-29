// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions;

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Abstractions.Security.Permissions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using Newtonsoft.Json;

[Serializable]
public class FolderPermissionInfo : PermissionInfoBase, IHydratable, IFolderPermissionInfo
{
    // local property declarations
    private int folderId;
    private string folderPath;
    private int folderPermissionId;
    private int portalId;

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderPermissionInfo"/> class.
    /// Constructs a new FolderPermissionInfo.
    /// </summary>
    public FolderPermissionInfo()
    {
        this.folderPermissionId = Null.NullInteger;
        this.folderPath = Null.NullString;
        this.portalId = Null.NullInteger;
        this.folderId = Null.NullInteger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderPermissionInfo"/> class.
    /// Constructs a new FolderPermissionInfo.
    /// </summary>
    /// <param name="permission">A PermissionInfo object.</param>
    public FolderPermissionInfo(PermissionInfo permission)
        : this((IPermissionDefinitionInfo)permission)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FolderPermissionInfo"/> class.
    /// Constructs a new FolderPermissionInfo.
    /// </summary>
    /// <param name="permission">A PermissionInfo object.</param>
    public FolderPermissionInfo(IPermissionDefinitionInfo permission)
        : this()
    {
        var @this = (IPermissionDefinitionInfo)this;
        @this.ModuleDefId = permission.ModuleDefId;
        @this.PermissionCode = permission.PermissionCode;
        @this.PermissionId = permission.PermissionId;
        @this.PermissionKey = permission.PermissionKey;
        @this.PermissionName = permission.PermissionName;
    }

    [XmlIgnore]
    [JsonIgnore]
    [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(IFolderPermissionInfo)}.{nameof(IFolderPermissionInfo.FolderPermissionId)} instead. Scheduled for removal in v11.0.0.")]
    public int FolderPermissionID
    {
        get
        {
            return ((IFolderPermissionInfo)this).FolderPermissionId;
        }

        set
        {
            ((IFolderPermissionInfo)this).FolderPermissionId = value;
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    int IFolderPermissionInfo.FolderPermissionId
    {
        get
        {
            return this.folderPermissionId;
        }

        set
        {
            this.folderPermissionId = value;
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(IFolderPermissionInfo)}.{nameof(IFolderPermissionInfo.FolderId)} instead. Scheduled for removal in v11.0.0.")]
    public int FolderID
    {
        get
        {
            return ((IFolderPermissionInfo)this).FolderId;
        }

        set
        {
            ((IFolderPermissionInfo)this).FolderId = value;
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(IFolderPermissionInfo)}.{nameof(IFolderPermissionInfo.FolderId)} instead. Scheduled for removal in v11.0.0.")]
    int IFolderPermissionInfo.FolderId
    {
        get
        {
            return this.folderId;
        }

        set
        {
            this.folderId = value;
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(IFolderPermissionInfo)}.{nameof(IFolderPermissionInfo.PortalId)} instead. Scheduled for removal in v11.0.0.")]
    public int PortalID
    {
        get
        {
            return ((IFolderPermissionInfo)this).PortalId;
        }

        set
        {
            ((IFolderPermissionInfo)this).PortalId = value;
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(IFolderPermissionInfo)}.{nameof(IFolderPermissionInfo.PortalId)} instead. Scheduled for removal in v11.0.0.")]
    int IFolderPermissionInfo.PortalId
    {
        get
        {
            return this.portalId;
        }

        set
        {
            this.portalId = value;
        }
    }

    [XmlElement("folderpath")]
    public string FolderPath
    {
        get
        {
            return this.folderPath;
        }

        set
        {
            this.folderPath = value;
        }
    }

    /// <summary>Gets or sets the Key ID.</summary>
    /// <returns>An Integer.</returns>
    [XmlIgnore]
    [JsonIgnore]
    public int KeyID
    {
        get
        {
            return ((IFolderPermissionInfo)this).FolderPermissionId;
        }

        set
        {
            ((IFolderPermissionInfo)this).FolderPermissionId = value;
        }
    }

    /// <summary>Fills a FolderPermissionInfo from a Data Reader.</summary>
    /// <param name="dr">The Data Reader to use.</param>
    public void Fill(IDataReader dr)
    {
        this.FillInternal(dr);

        var @this = (IFolderPermissionInfo)this;
        @this.FolderPermissionId = Null.SetNullInteger(dr["FolderPermissionID"]);
        @this.FolderId = Null.SetNullInteger(dr["FolderID"]);
        @this.PortalId = Null.SetNullInteger(dr["PortalID"]);
        @this.FolderPath = Null.SetNullString(dr["FolderPath"]);
    }
}
