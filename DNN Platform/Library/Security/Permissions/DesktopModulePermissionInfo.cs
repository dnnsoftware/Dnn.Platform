#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
