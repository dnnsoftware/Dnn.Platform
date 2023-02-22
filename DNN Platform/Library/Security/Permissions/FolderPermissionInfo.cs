// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using Newtonsoft.Json;

    [Serializable]
    public class FolderPermissionInfo : PermissionInfoBase, IHydratable
    {
        // local property declarations
        private int folderID;
        private string folderPath;
        private int folderPermissionID;
        private int portalID;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderPermissionInfo"/> class.
        /// Constructs a new FolderPermissionInfo.
        /// </summary>
        public FolderPermissionInfo()
        {
            this.folderPermissionID = Null.NullInteger;
            this.folderPath = Null.NullString;
            this.portalID = Null.NullInteger;
            this.folderID = Null.NullInteger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderPermissionInfo"/> class.
        /// Constructs a new FolderPermissionInfo.
        /// </summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public FolderPermissionInfo(PermissionInfo permission)
            : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }

        [XmlIgnore]
        [JsonIgnore]
        public int FolderPermissionID
        {
            get
            {
                return this.folderPermissionID;
            }

            set
            {
                this.folderPermissionID = value;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
        public int FolderID
        {
            get
            {
                return this.folderID;
            }

            set
            {
                this.folderID = value;
            }
        }

        [XmlIgnore]
        [JsonIgnore]
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
                return this.FolderPermissionID;
            }

            set
            {
                this.FolderPermissionID = value;
            }
        }

        /// <summary>Fills a FolderPermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
            this.FillInternal(dr);
            this.FolderPermissionID = Null.SetNullInteger(dr["FolderPermissionID"]);
            this.FolderID = Null.SetNullInteger(dr["FolderID"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.FolderPath = Null.SetNullString(dr["FolderPath"]);
        }
    }
}
