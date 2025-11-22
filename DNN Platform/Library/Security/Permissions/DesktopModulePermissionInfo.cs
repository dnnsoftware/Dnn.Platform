// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Permissions
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// DesktopModulePermissionInfo provides the Entity Layer for DesktopModulePermissionInfo
    /// Permissions.
    /// </summary>
    [Serializable]
    public class DesktopModulePermissionInfo : PermissionInfoBase, IHydratable
    {
        // local property declarations
        private int desktopModulePermissionID;
        private int portalDesktopModuleID;

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopModulePermissionInfo"/> class.
        /// Constructs a new DesktopModulePermissionInfo.
        /// </summary>
        public DesktopModulePermissionInfo()
        {
            this.desktopModulePermissionID = Null.NullInteger;
            this.portalDesktopModuleID = Null.NullInteger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DesktopModulePermissionInfo"/> class.
        /// Constructs a new DesktopModulePermissionInfo.
        /// </summary>
        /// <param name="permission">A PermissionInfo object.</param>
        public DesktopModulePermissionInfo(PermissionInfo permission)
            : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }

        /// <summary>Gets or sets the DesktopModule Permission ID.</summary>
        /// <returns>An Integer.</returns>
        public int DesktopModulePermissionID
        {
            get
            {
                return this.desktopModulePermissionID;
            }

            set
            {
                this.desktopModulePermissionID = value;
            }
        }

        /// <summary>Gets or sets the PortalDesktopModule ID.</summary>
        /// <returns>An Integer.</returns>
        public int PortalDesktopModuleID
        {
            get
            {
                return this.portalDesktopModuleID;
            }

            set
            {
                this.portalDesktopModuleID = value;
            }
        }

        /// <summary>Gets or sets the Key ID.</summary>
        /// <returns>An Integer.</returns>
        public int KeyID
        {
            get
            {
                return this.DesktopModulePermissionID;
            }

            set
            {
                this.DesktopModulePermissionID = value;
            }
        }

        /// <summary>Fills a DesktopModulePermissionInfo from a Data Reader.</summary>
        /// <param name="dr">The Data Reader to use.</param>
        public void Fill(IDataReader dr)
        {
            this.FillInternal(dr);
            this.DesktopModulePermissionID = Null.SetNullInteger(dr["DesktopModulePermissionID"]);
            this.PortalDesktopModuleID = Null.SetNullInteger(dr["PortalDesktopModuleID"]);
        }

        /// <summary>Compares if two DesktopModulePermissionInfo objects are equivalent/equal.</summary>
        /// <param name="other">a DesktopModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the DesktopModulePermissionCollection.
        /// DesktopModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
        public bool Equals(DesktopModulePermissionInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return (this.AllowAccess == other.AllowAccess) && (this.PortalDesktopModuleID == other.PortalDesktopModuleID) && (this.RoleID == other.RoleID) && (this.PermissionID == other.PermissionID);
        }

        /// <summary>Compares if two DesktopModulePermissionInfo objects are equivalent/equal.</summary>
        /// <param name="obj">a DesktopModulePermissionObject.</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object.
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the DesktopModulePermissionCollection.
        /// DesktopModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
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

            if (obj.GetType() != typeof(DesktopModulePermissionInfo))
            {
                return false;
            }

            return this.Equals((DesktopModulePermissionInfo)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.desktopModulePermissionID * 397) ^ this.portalDesktopModuleID;
            }
        }
    }
}
