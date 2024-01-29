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
        private int moduleDefId;
        private int permissionId;

        /// <inheritdoc cref="ModuleDefId" />
        [XmlIgnore]
        [JsonIgnore]
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(ModuleDefId)} instead. Scheduled for removal in v11.0.0.")]
        [CLSCompliant(false)]
        public int ModuleDefID
        {
            get => this.moduleDefId;
            set => this.moduleDefId = value;
        }

        /// <inheritdoc />
        [XmlElement("permissioncode")]
        public string PermissionCode { get; set; }

        /// <inheritdoc cref="PermissionId" />
        [XmlIgnore]
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(PermissionId)} instead. Scheduled for removal in v11.0.0.")]
        [CLSCompliant(false)]
        public int PermissionID
        {
            get => this.permissionId;
            set => this.permissionId = value;
        }

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
        public int ModuleDefId
        {
            get => this.moduleDefId;
            set => this.moduleDefId = value;
        }

        /// <inheritdoc />
        [XmlElement("permissionid")]
        public int PermissionId
        {
            get => this.permissionId;
            set => this.permissionId = value;
        }

        /// <summary>FillInternal fills a PermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        protected override void FillInternal(IDataReader dr)
        {
            base.FillInternal(dr);

            this.permissionId = Null.SetNullInteger(dr["PermissionID"]);
            this.moduleDefId = Null.SetNullInteger(dr["ModuleDefID"]);
            this.PermissionCode = Null.SetNullString(dr["PermissionCode"]);
            this.PermissionKey = Null.SetNullString(dr["PermissionKey"]);
            this.PermissionName = Null.SetNullString(dr["PermissionName"]);
        }
    }
}
