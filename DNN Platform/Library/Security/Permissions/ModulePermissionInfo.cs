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

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class    : ModulePermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModulePermissionInfo provides the Entity Layer for Module Permissions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ModulePermissionInfo : PermissionInfoBase, IHydratable
    {
        private int _moduleID;

        // local property declarations
        private int _modulePermissionID;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ModulePermissionInfo"/> class.
        /// Constructs a new ModulePermissionInfo.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModulePermissionInfo()
        {
            this._modulePermissionID = Null.NullInteger;
            this._moduleID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ModulePermissionInfo"/> class.
        /// Constructs a new ModulePermissionInfo.
        /// </summary>
        /// <param name="permission">A PermissionInfo object.</param>
        /// -----------------------------------------------------------------------------
        public ModulePermissionInfo(PermissionInfo permission)
            : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module Permission ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("modulepermissionid")]
        public int ModulePermissionID
        {
            get
            {
                return this._modulePermissionID;
            }

            set
            {
                this._modulePermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Module ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("moduleid")]
        public int ModuleID
        {
            get
            {
                return this._moduleID;
            }

            set
            {
                this._moduleID = value;
            }
        }

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
                return this.ModulePermissionID;
            }

            set
            {
                this.ModulePermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModulePermissionInfo from a Data Reader.
        /// </summary>
        /// <param name="dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.FillInternal(dr);
            this.ModulePermissionID = Null.SetNullInteger(dr["ModulePermissionID"]);
            this.ModuleID = Null.SetNullInteger(dr["ModuleID"]);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two ModulePermissionInfo objects are equivalent/equal.
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
        public bool Equals(ModulePermissionInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return (this.AllowAccess == other.AllowAccess) && (this.ModuleID == other.ModuleID) && (this.RoleID == other.RoleID) && (this.PermissionID == other.PermissionID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two ModulePermissionInfo objects are equivalent/equal.
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

            if (obj.GetType() != typeof(ModulePermissionInfo))
            {
                return false;
            }

            return this.Equals((ModulePermissionInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this._moduleID * 397) ^ this._modulePermissionID;
            }
        }
    }
}
