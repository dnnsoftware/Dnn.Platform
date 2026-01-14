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

    /// <summary>TabPermissionInfo provides the Entity Layer for Tab Permissions.</summary>
    [Serializable]
    [XmlRoot("permission")]
    public class TabPermissionInfo : PermissionInfoBase, IHydratable
    {
        private int tabID;

        // local property declarations
        private int tabPermissionID;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabPermissionInfo"/> class.
        /// Constructs a new TabPermissionInfo.
        /// </summary>
        public TabPermissionInfo()
        {
            this.tabPermissionID = Null.NullInteger;
            this.tabID = Null.NullInteger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TabPermissionInfo"/> class.
        /// Constructs a new TabPermissionInfo.
        /// </summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public TabPermissionInfo(PermissionInfo permission)
            : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }

        /// <summary>Gets or sets the Tab Permission ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("tabpermissionid")]
        public int TabPermissionID
        {
            get
            {
                return this.tabPermissionID;
            }

            set
            {
                this.tabPermissionID = value;
            }
        }

        /// <summary>Gets or sets the Tab ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("tabid")]
        public int TabID
        {
            get
            {
                return this.tabID;
            }

            set
            {
                this.tabID = value;
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
                return this.TabPermissionID;
            }

            set
            {
                this.TabPermissionID = value;
            }
        }

        /// <summary>Fills a TabPermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
            // Call the base classes fill method to populate base class properties
            this.FillInternal(dr);
            this.TabPermissionID = Null.SetNullInteger(dr["TabPermissionID"]);
            this.TabID = Null.SetNullInteger(dr["TabID"]);
        }
    }
}
