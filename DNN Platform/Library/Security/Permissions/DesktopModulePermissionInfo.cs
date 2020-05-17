﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : DesktopModulePermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// DesktopModulePermissionInfo provides the Entity Layer for DesktopModulePermissionInfo
    /// Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class DesktopModulePermissionInfo : PermissionInfoBase, IHydratable
    {
		#region "Private Members"

        //local property declarations
        private int _desktopModulePermissionID;
        private int _portalDesktopModuleID;
		
		#endregion
		
		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new DesktopModulePermissionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
		public DesktopModulePermissionInfo()
        {
            _desktopModulePermissionID = Null.NullInteger;
            _portalDesktopModuleID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new DesktopModulePermissionInfo
        /// </summary>
        /// <param name="permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public DesktopModulePermissionInfo(PermissionInfo permission) : this()
        {
            ModuleDefID = permission.ModuleDefID;
            PermissionCode = permission.PermissionCode;
            PermissionID = permission.PermissionID;
            PermissionKey = permission.PermissionKey;
            PermissionName = permission.PermissionName;
        }
		
		#endregion
		
		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the DesktopModule Permission ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int DesktopModulePermissionID
        {
            get
            {
                return _desktopModulePermissionID;
            }
            set
            {
                _desktopModulePermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the PortalDesktopModule ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int PortalDesktopModuleID
        {
            get
            {
                return _portalDesktopModuleID;
            }
            set
            {
                _portalDesktopModuleID = value;
            }
        }
		
		#endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a DesktopModulePermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);
            DesktopModulePermissionID = Null.SetNullInteger(dr["DesktopModulePermissionID"]);
            PortalDesktopModuleID = Null.SetNullInteger(dr["PortalDesktopModuleID"]);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        public int KeyID
        {
            get
            {
                return DesktopModulePermissionID;
            }
            set
            {
                DesktopModulePermissionID = value;
            }
        }

        #endregion

		#region "Public Methods"
				
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two DesktopModulePermissionInfo objects are equivalent/equal
        /// </summary>
        /// <param name="other">a DesktopModulePermissionObject</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the DesktopModulePermissionCollection.
        /// DesktopModulePermissionCollection.Contains will use this method to check if a given permission
        /// is already included in the collection.
        /// </remarks>
        /// -----------------------------------------------------------------------------
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
            return (AllowAccess == other.AllowAccess) && (PortalDesktopModuleID == other.PortalDesktopModuleID) && (RoleID == other.RoleID) && (PermissionID == other.PermissionID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two DesktopModulePermissionInfo objects are equivalent/equal
        /// </summary>
        /// <param name="obj">a DesktopModulePermissionObject</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object
        /// </returns>
        /// <remarks>
        /// This function is needed to prevent adding duplicates to the DesktopModulePermissionCollection.
        /// DesktopModulePermissionCollection.Contains will use this method to check if a given permission
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
            if (obj.GetType() != typeof (DesktopModulePermissionInfo))
            {
                return false;
            }
            return Equals((DesktopModulePermissionInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_desktopModulePermissionID*397) ^ _portalDesktopModuleID;
            }
        }
		
		#endregion
    }
}
