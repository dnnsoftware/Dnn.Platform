// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins;

using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;
using Newtonsoft.Json;

/// Project  : DotNetNuke
/// Class    : SkinPackageInfo
///
/// <summary>    Handles the Business Object for Skins.</summary>
[Serializable]
public class SkinPackageInfo : BaseEntityInfo, IHydratable
{
    private int packageID = Null.NullInteger;
    private int portalID = Null.NullInteger;
    private string skinName;
    private int skinPackageID = Null.NullInteger;
    private string skinType;
    private Dictionary<int, string> skins = new Dictionary<int, string>();

    public int PackageID
    {
        get
        {
            return this.packageID;
        }

        set
        {
            this.packageID = value;
        }
    }

    public int SkinPackageID
    {
        get
        {
            return this.skinPackageID;
        }

        set
        {
            this.skinPackageID = value;
        }
    }

    public int PortalID
    {
        get
        {
            return this.portalID;
        }

        set
        {
            this.portalID = value;
        }
    }

    public string SkinName
    {
        get
        {
            return this.skinName;
        }

        set
        {
            this.skinName = value;
        }
    }

    [XmlIgnore]
    [JsonIgnore]
    public Dictionary<int, string> Skins
    {
        get
        {
            return this.skins;
        }

        set
        {
            this.skins = value;
        }
    }

    public string SkinType
    {
        get
        {
            return this.skinType;
        }

        set
        {
            this.skinType = value;
        }
    }

    /// <inheritdoc/>
    public int KeyID
    {
        get
        {
            return this.SkinPackageID;
        }

        set
        {
            this.SkinPackageID = value;
        }
    }

    /// <inheritdoc/>
    public void Fill(IDataReader dr)
    {
        this.SkinPackageID = Null.SetNullInteger(dr["SkinPackageID"]);
        this.PackageID = Null.SetNullInteger(dr["PackageID"]);
        this.SkinName = Null.SetNullString(dr["SkinName"]);
        this.SkinType = Null.SetNullString(dr["SkinType"]);

        // Call the base classes fill method to populate base class proeprties
        this.FillInternal(dr);

        if (dr.NextResult())
        {
            while (dr.Read())
            {
                int skinID = Null.SetNullInteger(dr["SkinID"]);
                if (skinID > Null.NullInteger)
                {
                    this.skins[skinID] = Null.SetNullString(dr["SkinSrc"]);
                }
            }
        }
    }
}
