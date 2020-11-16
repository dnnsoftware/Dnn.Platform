// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Library.Model
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security.Permissions;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : MenuPermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// MenuPermissionInfo provides the Entity Layer for Module Permissions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class MenuPermissionInfo : PermissionInfoBase, IHydratable
    {
        private int _menuId;

        // local property declarations
        private int _menuPermissionId;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuPermissionInfo"/> class.
        /// Constructs a new MenuPermissionInfo.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public MenuPermissionInfo()
        {
            this._menuPermissionId = Null.NullInteger;
            this._menuId = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuPermissionInfo"/> class.
        /// Constructs a new MenuPermissionInfo.
        /// </summary>
        /// <param name="permission">A PermissionInfo object.</param>
        /// -----------------------------------------------------------------------------
        public MenuPermissionInfo(PermissionInfo permission)
            : this()
        {
            this.ModuleDefID = Null.NullInteger;
            this.PermissionCode = "PERSONABAR_MENU";
            this.PermissionID = permission.PermissionId;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module Permission ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("menupermissionid")]
        public int MenuPermissionId
        {
            get
            {
                return this._menuPermissionId;
            }

            set
            {
                this._menuPermissionId = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("menuid")]
        public int MenuId
        {
            get
            {
                return this._menuId;
            }

            set
            {
                this._menuId = value;
            }
        }

        [XmlElement("portalid")]
        public int PortalId { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.MenuPermissionId;
            }

            set
            {
                this.MenuPermissionId = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a MenuPermissionInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.FillInternal(dr);
            this.MenuPermissionId = Null.SetNullInteger(dr["MenuPermissionId"]);
            this.MenuId = Null.SetNullInteger(dr["MenuId"]);
            this.PortalId = Null.SetNullInteger(dr["PortalId"]);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two MenuPermissionInfo objects are equivalent/equal.
        /// </summary>
        /// <param name="other">a ModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the ModulePermissionCollection.
        /// ModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool Equals(MenuPermissionInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return (this.AllowAccess == other.AllowAccess) && (this.MenuId == other.MenuId) && (this.RoleID == other.RoleID) && (this.PermissionID == other.PermissionID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two MenuPermissionInfo objects are equivalent/equal.
        /// </summary>
        /// <param name="obj">a ModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the ModulePermissionCollection.
        /// ModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(MenuPermissionInfo))
            {
                return false;
            }

            return this.Equals((MenuPermissionInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this._menuId * 397) ^ this._menuPermissionId;
            }
        }
    }
}
