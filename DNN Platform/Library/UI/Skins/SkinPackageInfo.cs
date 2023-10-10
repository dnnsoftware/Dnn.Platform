// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Collections;
    using DotNetNuke.Abstractions.Skins;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;
    using Newtonsoft.Json;

    /// Project  : DotNetNuke
    /// Class    : SkinPackageInfo
    ///
    /// <summary>    Handles the Business Object for Skins.</summary>
    [Serializable]
    public class SkinPackageInfo : BaseEntityInfo, IHydratable, ISkinPackageInfo
    {
        private int packageID = Null.NullInteger;
        private int portalID = Null.NullInteger;
        private string skinName;
        private int skinPackageID = Null.NullInteger;
        private string skinType;
        private List<SkinInfo> skins = new List<SkinInfo>();
        private AbstractionList<ISkinInfo, SkinInfo> abstractSkins;

        /// <inheritdoc cref="ISkinPackageInfo.PackageId"/>
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

        /// <inheritdoc cref="ISkinPackageInfo.SkinPackageId"/>
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

        /// <inheritdoc cref="ISkinPackageInfo.PortalId"/>
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

        /// <inheritdoc/>
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

        /// <inheritdoc cref="ISkinPackageInfo.Skins"/>
        [XmlIgnore]
        [JsonIgnore]
        public List<SkinInfo> Skins
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

        /// <inheritdoc cref="ISkinPackageInfo.SkinType"/>
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
        [XmlIgnore]
        [JsonIgnore]
        int ISkinPackageInfo.PackageId
        {
            get => this.PackageID;
            set => this.PackageID = value;
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        int ISkinPackageInfo.SkinPackageId
        {
            get => this.SkinPackageID;
            set => this.SkinPackageID = value;
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        IObjectList<ISkinInfo> ISkinPackageInfo.Skins
        {
            get
            {
                return this.abstractSkins ??= new AbstractionList<ISkinInfo, SkinInfo>(this.Skins);
            }
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        SkinPackageType ISkinPackageInfo.SkinType
        {
            get => SkinUtils.FromDatabaseName(this.SkinType);
            set => this.SkinType = SkinUtils.ToDatabaseName(value);
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        int ISkinPackageInfo.PortalId
        {
            get => this.PortalID;
            set => this.PortalID = value;
        }

        /// <inheritdoc/>
        public void Fill(IDataReader dr)
        {
            this.SkinPackageID = Null.SetNullInteger(dr["SkinPackageID"]);
            this.PackageID = Null.SetNullInteger(dr["PackageID"]);
            this.SkinName = Null.SetNullString(dr["SkinName"]);
            this.SkinType = Null.SetNullString(dr["SkinType"]);

            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);

            if (dr.NextResult())
            {
                while (dr.Read())
                {
                    int skinID = Null.SetNullInteger(dr["SkinID"]);
                    if (skinID > Null.NullInteger)
                    {
                        this.skins.Add(new SkinInfo
                        {
                            SkinId = skinID,
                            SkinSrc = Null.SetNullString(dr["SkinSrc"]),
                            SkinPackageId = this.SkinPackageID,
                            PortalId = this.PortalID,
                            SkinRoot = SkinUtils.FromDatabaseName(this.SkinType) switch
                            {
                                SkinPackageType.Container => SkinController.RootContainer,
                                SkinPackageType.Skin => SkinController.RootSkin,
                                _ => throw new ArgumentOutOfRangeException(nameof(this.SkinType), this.SkinType, "Invalid skin type."),
                            },
                        });
                    }
                }
            }
        }
    }
}
