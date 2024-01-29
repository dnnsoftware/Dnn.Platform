// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;

    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : PermissionInfoBase
    /// <summary>PermissionInfoBase provides a base class for PermissionInfo classes.</summary>
    /// <remarks>All Permission classes have  a common set of properties
    ///   - AllowAccess
    ///   - RoleID
    ///   - RoleName
    ///   - UserID
    ///   - Username
    ///   - DisplayName
    ///
    /// and these are implemented in this base class.
    /// </remarks>
    [Serializable]
    public abstract class PermissionInfoBase : PermissionInfo, IPermissionInfo
    {
        private bool allowAccess;
        private string displayName;
        private int roleId;
        private string roleName;
        private int userId;
        private string username;

        /// <summary>Initializes a new instance of the <see cref="PermissionInfoBase"/> class.</summary>
        public PermissionInfoBase()
        {
            this.roleId = int.Parse(Globals.glbRoleNothing);
            this.allowAccess = false;
            this.roleName = Null.NullString;
            this.userId = Null.NullInteger;
            this.username = Null.NullString;
            this.displayName = Null.NullString;
        }

        /// <inheritdoc />
        [XmlElement("allowaccess")]
        public bool AllowAccess
        {
            get
            {
                return this.allowAccess;
            }

            set
            {
                this.allowAccess = value;
            }
        }

        /// <inheritdoc />
        [XmlElement("displayname")]
        public string DisplayName
        {
            get
            {
                return this.displayName;
            }

            set
            {
                this.displayName = value;
            }
        }

        /// <inheritdoc cref="RoleId" />
        [XmlIgnore]
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(RoleId)} instead. Scheduled for removal in v11.0.0.")]
        [CLSCompliant(false)]
        public int RoleID
        {
            get
            {
                return this.roleId;
            }

            set
            {
                this.roleId = value;
            }
        }

        /// <inheritdoc />
        [XmlElement("rolename")]
        public string RoleName
        {
            get
            {
                return this.roleName;
            }

            set
            {
                this.roleName = value;
            }
        }

        /// <inheritdoc cref="UserId"/>
        [XmlIgnore]
        [Obsolete($"Deprecated in DotNetNuke 9.13.1. Use {nameof(UserId)} instead. Scheduled for removal in v11.0.0.")]
        [CLSCompliant(false)]
        public int UserID
        {
            get
            {
                return this.userId;
            }

            set
            {
                this.userId = value;
            }
        }

        /// <inheritdoc />
        [XmlElement("username")]
        public string Username
        {
            get
            {
                return this.username;
            }

            set
            {
                this.username = value;
            }
        }

        /// <inheritdoc />
        [XmlElement("roleid")]
        public int RoleId
        {
            get => this.roleId;
            set => this.roleId = value;
        }

        /// <inheritdoc />
        [XmlElement("userid")]
        public int UserId
        {
            get => this.userId;
            set => this.userId = value;
        }

        /// <summary>FillInternal fills the PermissionInfoBase from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        protected override void FillInternal(IDataReader dr)
        {
            // Call the base classes fill method to populate base class properties
            base.FillInternal(dr);

            this.userId = Null.SetNullInteger(dr["UserID"]);
            this.username = Null.SetNullString(dr["Username"]);
            this.displayName = Null.SetNullString(dr["DisplayName"]);
            if (this.userId == Null.NullInteger)
            {
                this.roleId = Null.SetNullInteger(dr["RoleID"]);
                this.roleName = Null.SetNullString(dr["RoleName"]);
            }
            else
            {
                this.roleId = int.Parse(Globals.glbRoleNothing);
                this.roleName = string.Empty;
            }

            this.allowAccess = Null.SetNullBoolean(dr["AllowAccess"]);
        }
    }
}
