// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;

    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : PermissionInfoBase
    /// <summary>PermissionInfoBase provides a base class for PermissionInfo classes.</summary>
    /// <remarks>All Permission calsses have  a common set of properties
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
    public abstract class PermissionInfoBase : PermissionInfo
    {
        private bool allowAccess;
        private string displayName;
        private int roleID;
        private string roleName;
        private int userID;
        private string username;

        /// <summary>Initializes a new instance of the <see cref="PermissionInfoBase"/> class.</summary>
        public PermissionInfoBase()
        {
            this.roleID = int.Parse(Globals.glbRoleNothing);
            this.allowAccess = false;
            this.roleName = Null.NullString;
            this.userID = Null.NullInteger;
            this.username = Null.NullString;
            this.displayName = Null.NullString;
        }

        /// <summary>Gets or sets a value indicating whether gets and sets  aflag that indicates whether the user or role has permission.</summary>
        /// <returns>A Boolean.</returns>
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

        /// <summary>Gets or sets the User's DisplayName.</summary>
        /// <returns>A String.</returns>
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

        /// <summary>Gets or sets the Role ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("roleid")]
        public int RoleID
        {
            get
            {
                return this.roleID;
            }

            set
            {
                this.roleID = value;
            }
        }

        /// <summary>Gets or sets the Role Name.</summary>
        /// <returns>A String.</returns>
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

        /// <summary>Gets or sets the User ID.</summary>
        /// <returns>An Integer.</returns>
        [XmlElement("userid")]
        public int UserID
        {
            get
            {
                return this.userID;
            }

            set
            {
                this.userID = value;
            }
        }

        /// <summary>Gets or sets the User Name.</summary>
        /// <returns>A String.</returns>
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

        /// <summary>FillInternal fills the PermissionInfoBase from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        protected override void FillInternal(IDataReader dr)
        {
            // Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);

            this.UserID = Null.SetNullInteger(dr["UserID"]);
            this.Username = Null.SetNullString(dr["Username"]);
            this.DisplayName = Null.SetNullString(dr["DisplayName"]);
            if (this.UserID == Null.NullInteger)
            {
                this.RoleID = Null.SetNullInteger(dr["RoleID"]);
                this.RoleName = Null.SetNullString(dr["RoleName"]);
            }
            else
            {
                this.RoleID = int.Parse(Globals.glbRoleNothing);
                this.RoleName = string.Empty;
            }

            this.AllowAccess = Null.SetNullBoolean(dr["AllowAccess"]);
        }
    }
}
