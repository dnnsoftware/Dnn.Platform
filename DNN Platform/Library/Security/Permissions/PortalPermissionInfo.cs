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

    /// <summary>PortalPermissionInfo provides the Entity Layer for Portal Permissions.</summary>
    [Serializable]
    [XmlRoot("permission")]
    public class PortalPermissionInfo : PermissionInfoBase, IHydratable
    {
        private int portalID;

        // local property declarations
        private int portalPermissionID;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalPermissionInfo"/> class.
        /// Constructs a new PortalPermissionInfo.
        /// </summary>
        public PortalPermissionInfo()
        {
            this.portalPermissionID = Null.NullInteger;
            this.portalID = Null.NullInteger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalPermissionInfo"/> class.
        /// Constructs a new PortalPermissionInfo.
        /// </summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public PortalPermissionInfo(PermissionInfo permission)
            : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }

        /// <summary>Gets or sets the Portal Permission ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("portalpermissionid")]
        public int PortalPermissionID
        {
            get
            {
                return this.portalPermissionID;
            }

            set
            {
                this.portalPermissionID = value;
            }
        }

        /// <summary>Gets or sets the Portal ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("portalid")]
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

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.PortalPermissionID;
            }

            set
            {
                this.PortalPermissionID = value;
            }
        }

        /// <summary>Fills a PortalPermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);
            this.PortalPermissionID = Null.SetNullInteger(dr["PortalPermissionID"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
        }
    }
}
