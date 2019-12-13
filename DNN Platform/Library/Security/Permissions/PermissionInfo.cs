// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class PermissionInfo : BaseEntityInfo
    {
		#region Private Members

        #endregion
		
		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Mdoule Definition ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int ModuleDefID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission Code
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissioncode")]
        public string PermissionCode { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissionid")]
        public int PermissionID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission Key
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("permissionkey")]
        public string PermissionKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Permission Name
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public string PermissionName { get; set; }

        #endregion
		
		#region "Protected methods"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// FillInternal fills a PermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
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
