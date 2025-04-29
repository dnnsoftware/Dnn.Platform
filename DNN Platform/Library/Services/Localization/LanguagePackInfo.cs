// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization;

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

[Serializable]
public class LanguagePackInfo : BaseEntityInfo, IHydratable
{
    private int dependentPackageID = Null.NullInteger;
    private int languageID = Null.NullInteger;
    private int languagePackID = Null.NullInteger;
    private int packageID = Null.NullInteger;

    public LanguagePackType PackageType
    {
        get
        {
            if (this.DependentPackageID == -2)
            {
                return LanguagePackType.Core;
            }
            else
            {
                return LanguagePackType.Extension;
            }
        }
    }

    public int LanguagePackID
    {
        get
        {
            return this.languagePackID;
        }

        set
        {
            this.languagePackID = value;
        }
    }

    public int LanguageID
    {
        get
        {
            return this.languageID;
        }

        set
        {
            this.languageID = value;
        }
    }

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

    public int DependentPackageID
    {
        get
        {
            return this.dependentPackageID;
        }

        set
        {
            this.dependentPackageID = value;
        }
    }

    /// <inheritdoc/>
    public int KeyID
    {
        get
        {
            return this.LanguagePackID;
        }

        set
        {
            this.LanguagePackID = value;
        }
    }

    /// <inheritdoc/>
    public void Fill(IDataReader dr)
    {
        this.LanguagePackID = Null.SetNullInteger(dr["LanguagePackID"]);
        this.LanguageID = Null.SetNullInteger(dr["LanguageID"]);
        this.PackageID = Null.SetNullInteger(dr["PackageID"]);
        this.DependentPackageID = Null.SetNullInteger(dr["DependentPackageID"]);

        // Call the base classes fill method to populate base class proeprties
        this.FillInternal(dr);
    }
}
