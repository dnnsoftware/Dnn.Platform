// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : PermissionInfoBase
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PermissionInfoBase provides a base class for PermissionInfo classes
    /// </summary>
    /// <remarks>All Permission calsses have  a common set of properties
    ///   - AllowAccess
    ///   - RoleID
    ///   - RoleName
    ///   - UserID
    ///   - Username
    ///   - DisplayName
    /// 
    /// and these are implemented in this base class
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public abstract class PermissionInfoBase : PermissionInfo
    {
		#region "Private Members"

        private bool _AllowAccess;
        private string _DisplayName;
        private int _RoleID;
        private string _RoleName;
        private int _UserID;
        private string _Username;
		
		#endregion
		
		#region "Constructors"

        public PermissionInfoBase()
        {
            _RoleID = int.Parse(Globals.glbRoleNothing);
            _AllowAccess = false;
            _RoleName = Null.NullString;
            _UserID = Null.NullInteger;
            _Username = Null.NullString;
            _DisplayName = Null.NullString;
        }
		
		#endregion

		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets  aflag that indicates whether the user or role has permission
        /// </summary>
        /// <returns>A Boolean</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("allowaccess")]
        public bool AllowAccess
        {
            get
            {
                return _AllowAccess;
            }
            set
            {
                _AllowAccess = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User's DisplayName
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("displayname")]
        public string DisplayName
        {
            get
            {
                return _DisplayName;
            }
            set
            {
                _DisplayName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Role ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("roleid")]
        public int RoleID
        {
            get
            {
                return _RoleID;
            }
            set
            {
                _RoleID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Role Name
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("rolename")]
        public string RoleName
        {
            get
            {
                return _RoleName;
            }
            set
            {
                _RoleName = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("userid")]
        public int UserID
        {
            get
            {
                return _UserID;
            }
            set
            {
                _UserID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User Name
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("username")]
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                _Username = value;
            }
        }
		
		#endregion
		
		#region "Protected Methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillInternal fills the PermissionInfoBase from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        protected override void FillInternal(IDataReader dr)
        {
            //Call the base classes fill method to populate base class proeprties
            base.FillInternal(dr);

            UserID = Null.SetNullInteger(dr["UserID"]);
            Username = Null.SetNullString(dr["Username"]);
            DisplayName = Null.SetNullString(dr["DisplayName"]);
            if (UserID == Null.NullInteger)
            {
                RoleID = Null.SetNullInteger(dr["RoleID"]);
                RoleName = Null.SetNullString(dr["RoleName"]);
            }
            else
            {
                RoleID = int.Parse(Globals.glbRoleNothing);
                RoleName = "";
            }
            AllowAccess = Null.SetNullBoolean(dr["AllowAccess"]);
        }
		
		#endregion
    }
}
