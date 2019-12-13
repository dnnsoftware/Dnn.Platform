// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : ModulePermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ModulePermissionInfo provides the Entity Layer for Module Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class ModulePermissionInfo : PermissionInfoBase, IHydratable
    {
		#region "Private Members"
		
        private int _moduleID;

        //local property declarations
        private int _modulePermissionID;
		
		#endregion
		
		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new ModulePermissionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public ModulePermissionInfo()
        {
            _modulePermissionID = Null.NullInteger;
            _moduleID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new ModulePermissionInfo
        /// </summary>
        /// <param name="permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public ModulePermissionInfo(PermissionInfo permission) : this()
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
        /// Gets and sets the Module Permission ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("modulepermissionid")]
        public int ModulePermissionID
        {
            get
            {
                return _modulePermissionID;
            }
            set
            {
                _modulePermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Module ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("moduleid")]
        public int ModuleID
        {
            get
            {
                return _moduleID;
            }
            set
            {
                _moduleID = value;
            }
        }
		
		#endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a ModulePermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);
            ModulePermissionID = Null.SetNullInteger(dr["ModulePermissionID"]);
            ModuleID = Null.SetNullInteger(dr["ModuleID"]);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return ModulePermissionID;
            }
            set
            {
                ModulePermissionID = value;
            }
        }

        #endregion
		
		#region "Public Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two ModulePermissionInfo objects are equivalent/equal
        /// </summary>
        /// <param name="other">a ModulePermissionObject</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object
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
            return (AllowAccess == other.AllowAccess) && (ModuleID == other.ModuleID) && (RoleID == other.RoleID) && (PermissionID == other.PermissionID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Compares if two ModulePermissionInfo objects are equivalent/equal
        /// </summary>
        /// <param name="obj">a ModulePermissionObject</param>
        /// <returns>true if the permissions being passed represents the same permission
        /// in the current object
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
            if (obj.GetType() != typeof (ModulePermissionInfo))
            {
                return false;
            }
            return Equals((ModulePermissionInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_moduleID*397) ^ _modulePermissionID;
            }
        }
		
		#endregion
    }
}
