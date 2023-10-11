// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using Newtonsoft.Json;

    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : PermissionInfo
    /// <summary>PermissionInfo provides the Entity Layer for Permissions.</summary>
    [Serializable]
    public class PermissionInfo : BaseEntityInfo, IPermissionDefinitionInfo
    {
        /// <inheritdoc cref="IPermissionDefinitionInfo.ModuleDefId" />
        [XmlIgnore]
        [JsonIgnore]
        public int ModuleDefID { get; set; }

        /// <inheritdoc />
        [XmlElement("permissioncode")]
        public string PermissionCode { get; set; }

        /// <inheritdoc cref="IPermissionDefinitionInfo.PermissionID" />
        [XmlElement("permissionid")]
        public int PermissionID { get; set; }

        /// <inheritdoc />
        [XmlElement("permissionkey")]
        public string PermissionKey { get; set; }

        /// <inheritdoc />
        [XmlIgnore]
        [JsonIgnore]
        public string PermissionName { get; set; }

        /// <inheritdoc />
        [XmlIgnore]
        [JsonIgnore]
        int IPermissionDefinitionInfo.ModuleDefId
        {
            get => this.ModuleDefID;
            set => this.ModuleDefID = value;
        }

        /// <inheritdoc />
        [XmlIgnore]
        [JsonIgnore]
        int IPermissionDefinitionInfo.PermissionId
        {
            get => this.PermissionID;
            set => this.PermissionID = value;
        }

        /// <summary>FillInternal fills a PermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
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
