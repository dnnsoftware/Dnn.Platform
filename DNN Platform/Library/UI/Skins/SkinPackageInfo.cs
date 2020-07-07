// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : SkinPackageInfo
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///     Handles the Business Object for Skins.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class SkinPackageInfo : BaseEntityInfo, IHydratable
    {
        private int _PackageID = Null.NullInteger;
        private int _PortalID = Null.NullInteger;
        private string _SkinName;
        private int _SkinPackageID = Null.NullInteger;
        private string _SkinType;
        private Dictionary<int, string> _Skins = new Dictionary<int, string>();

        public int PackageID
        {
            get
            {
                return this._PackageID;
            }

            set
            {
                this._PackageID = value;
            }
        }

        public int SkinPackageID
        {
            get
            {
                return this._SkinPackageID;
            }

            set
            {
                this._SkinPackageID = value;
            }
        }

        public int PortalID
        {
            get
            {
                return this._PortalID;
            }

            set
            {
                this._PortalID = value;
            }
        }

        public string SkinName
        {
            get
            {
                return this._SkinName;
            }

            set
            {
                this._SkinName = value;
            }
        }

        [XmlIgnore]
        public Dictionary<int, string> Skins
        {
            get
            {
                return this._Skins;
            }

            set
            {
                this._Skins = value;
            }
        }

        public string SkinType
        {
            get
            {
                return this._SkinType;
            }

            set
            {
                this._SkinType = value;
            }
        }

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
                        this._Skins[skinID] = Null.SetNullString(dr["SkinSrc"]);
                    }
                }
            }
        }
    }
}
