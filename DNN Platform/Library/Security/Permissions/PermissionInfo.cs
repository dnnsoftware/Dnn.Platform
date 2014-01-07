#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;

#endregion

namespace DotNetNuke.Security.Permissions
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : PermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PermissionInfo provides the Entity Layer for Permissions
    /// </summary>
    /// <history>
    /// 	[cnurse]	01/14/2008   Documented
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class PermissionInfo : BaseEntityInfo
    {
		#region "Private Members"

        private int _ModuleDefID;
        private string _PermissionCode;
        private int _PermissionID;
        private string _PermissionKey;
        private string _PermissionName;
		
		#endregion
		
		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Mdoule Definition ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// <history>
        /// 	[cnurse]	01/14/2008   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int ModuleDefID
        {
            get
            {
                return _ModuleDefID;
            }
            set
            {
                _ModuleDefID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission Code
        /// </summary>
        /// <returns>A String</returns>
        /// <history>
        /// 	[cnurse]	01/14/2008   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissioncode")]
        public string PermissionCode
        {
            get
            {
                return _PermissionCode;
            }
            set
            {
                _PermissionCode = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// <history>
        /// 	[cnurse]	01/14/2008   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissionid")]
        public int PermissionID
        {
            get
            {
                return _PermissionID;
            }
            set
            {
                _PermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission Key
        /// </summary>
        /// <returns>A String</returns>
        /// <history>
        /// 	[cnurse]	01/14/2008   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissionkey")]
        public string PermissionKey
        {
            get
            {
                return _PermissionKey;
            }
            set
            {
                _PermissionKey = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission Name
        /// </summary>
        /// <returns>A String</returns>
        /// <history>
        /// 	[cnurse]	01/14/2008   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public string PermissionName
        {
            get
            {
                return _PermissionName;
            }
            set
            {
                _PermissionName = value;
            }
        }
		
		#endregion
		
		#region "Protected methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillInternal fills a PermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// <history>
        /// 	[cnurse]	01/14/2008   Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void FillInternal(IDataReader dr)
        {
            base.FillInternal(dr);
            PermissionID = Null.SetNullInteger(dr["PermissionID"]);
            ModuleDefID = Null.SetNullInteger(dr["ModuleDefID"]);
            PermissionCode = Null.SetNullString(dr["PermissionCode"]);
            PermissionKey = Null.SetNullString(dr["PermissionKey"]);
            PermissionName = Null.SetNullString(dr["PermissionName"]);
		}

		#endregion
	}
}
