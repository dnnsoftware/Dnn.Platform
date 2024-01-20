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
        private int packageId = Null.NullInteger;
        private int portalId = Null.NullInteger;
        private string skinName;
        private int skinPackageId = Null.NullInteger;
        private string skinType;
        private Dictionary<int, string> skins = new Dictionary<int, string>();
        private List<SkinInfo> skinsList = new List<SkinInfo>();
        private AbstractionList<ISkinInfo, SkinInfo> abstractSkins;

        /// <inheritdoc cref="ISkinPackageInfo.PackageId"/>
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(ISkinPackageInfo)}.{nameof(ISkinPackageInfo.PackageId)} instead. Scheduled for removal in v11.0.0.")]
        public int PackageID
        {
            get
            {
                return ((ISkinPackageInfo)this).PackageId;
            }

            set
            {
                ((ISkinPackageInfo)this).PackageId = value;
            }
        }

        /// <inheritdoc cref="ISkinPackageInfo.SkinPackageId"/>
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(ISkinPackageInfo)}.{nameof(ISkinPackageInfo.SkinPackageId)} instead. Scheduled for removal in v11.0.0.")]
        public int SkinPackageID
        {
            get
            {
                return ((ISkinPackageInfo)this).SkinPackageId;
            }

            set
            {
                ((ISkinPackageInfo)this).SkinPackageId = value;
            }
        }

        /// <inheritdoc cref="ISkinPackageInfo.PortalId"/>
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(ISkinPackageInfo)}.{nameof(ISkinPackageInfo.PortalId)} instead. Scheduled for removal in v11.0.0.")]
        public int PortalID
        {
            get
            {
                return ((ISkinPackageInfo)this).PortalId;
            }

            set
            {
                ((ISkinPackageInfo)this).PortalId = value;
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

        /// <summary>Gets or sets a dictionary mapping from <see cref="SkinInfo.SkinId"/> to <see cref="SkinInfo.SkinSrc"/>.</summary>
        [XmlIgnore]
        [JsonIgnore]
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(ISkinPackageInfo)}.{nameof(ISkinPackageInfo.Skins)} instead. Scheduled for removal in v11.0.0.")]
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

        /// <inheritdoc cref="ISkinPackageInfo.Skins"/>
        [XmlIgnore]
        [JsonIgnore]
        public List<SkinInfo> SkinsList
        {
            get
            {
                return this.skinsList;
            }

            set
            {
                this.skinsList = value;
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
                return ((ISkinPackageInfo)this).SkinPackageId;
            }

            set
            {
                ((ISkinPackageInfo)this).SkinPackageId = value;
            }
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        int ISkinPackageInfo.PackageId
        {
            get => this.packageId;
            set => this.packageId = value;
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        int ISkinPackageInfo.SkinPackageId
        {
            get => this.skinPackageId;
            set => this.skinPackageId = value;
        }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        IObjectList<ISkinInfo> ISkinPackageInfo.Skins
        {
            get
            {
                return this.abstractSkins ??= new AbstractionList<ISkinInfo, SkinInfo>(this.SkinsList);
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
            get => this.portalId;
            set => this.portalId = value;
        }

        /// <inheritdoc/>
        public void Fill(IDataReader dr)
        {
            var @this = (ISkinPackageInfo)this;
            @this.SkinPackageId = Null.SetNullInteger(dr["SkinPackageID"]);
            @this.PackageId = Null.SetNullInteger(dr["PackageID"]);
            @this.SkinName = Null.SetNullString(dr["SkinName"]);
            this.SkinType = Null.SetNullString(dr["SkinType"]);

            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);

            if (dr.NextResult())
            {
                while (dr.Read())
                {
                    int skinId = Null.SetNullInteger(dr["SkinID"]);
                    if (skinId > Null.NullInteger)
                    {
                        var skinSrc = Null.SetNullString(dr["SkinSrc"]);
                        this.skins[skinId] = skinSrc;
                        this.skinsList.Add(new SkinInfo
                        {
                            SkinId = skinId,
                            SkinSrc = skinSrc,
                            SkinPackageId = @this.SkinPackageId,
                            PortalId = @this.PortalId,
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
