﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
    /// Class	 : TabPermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// TabPermissionInfo provides the Entity Layer for Tab Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    [XmlRoot("permission")]
    public class TabPermissionInfo : PermissionInfoBase, IHydratable
    {
		#region "Private Members"
		
        private int _TabID;
        //local property declarations
		private int _TabPermissionID;
		
		#endregion
		
		#region "Constructors"

         /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new TabPermissionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
       public TabPermissionInfo()
        {
            this._TabPermissionID = Null.NullInteger;
            this._TabID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new TabPermissionInfo
        /// </summary>
        /// <param name="permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public TabPermissionInfo(PermissionInfo permission) : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }
		
		#endregion
		
		#region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Tab Permission ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("tabpermissionid")]
        public int TabPermissionID
        {
            get
            {
                return this._TabPermissionID;
            }
            set
            {
                this._TabPermissionID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Tab ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlElement("tabid")]
        public int TabID
        {
            get
            {
                return this._TabID;
            }
            set
            {
                this._TabID = value;
            }
        }
		
		#endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a TabPermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            //Call the base classes fill method to ppoulate base class proeprties
			base.FillInternal(dr);
            this.TabPermissionID = Null.SetNullInteger(dr["TabPermissionID"]);
            this.TabID = Null.SetNullInteger(dr["TabID"]);
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
                return this.TabPermissionID;
            }
            set
            {
                this.TabPermissionID = value;
            }
        }

        #endregion
    }
}
