// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : PermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PermissionInfo provides the Entity Layer for Permissions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class PermissionInfo : BaseEntityInfo
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Mdoule Definition ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int ModuleDefID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Permission Code.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissioncode")]
        public string PermissionCode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Permission ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissionid")]
        public int PermissionID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Permission Key.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissionkey")]
        public string PermissionKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Permission Name.
        /// </summary>
        /// <returns>A String.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public string PermissionName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillInternal fills a PermissionInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        protected override void FillInternal(IDataReader dr)
        {
            base.FillInternal(dr);
            this.PermissionID = Null.SetNullInteger(dr["PermissionID"]);
            this.ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            this.PermissionCode = Null.SetNullString(dr["PermissionCode"]);
            this.PermissionKey = Null.SetNullString(dr["PermissionKey"]);
            this.PermissionName = Null.SetNullString(dr["PermissionName"]);
        }
    }
}
