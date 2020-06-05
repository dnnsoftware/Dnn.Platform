﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;
using System.Data;
using System.Xml.Serialization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace Dnn.PersonaBar.Library.Model
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.Security.Permissions
    /// Class	 : MenuPermissionInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// MenuPermissionInfo provides the Entity Layer for Module Permissions
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class PermissionInfo : BaseEntityInfo, IHydratable
    {
        #region "Public Properties"


        [XmlElement("permissionId")]
        public int PermissionId { get; set; }

        [XmlElement("menuId")]
        public int MenuId { get; set; }

        [XmlElement("permissionKey")]
        public string PermissionKey { get; set; }

        [XmlElement("permissionName")]
        public string PermissionName { get; set; }

        #endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a MenuPermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);

            
            this.PermissionId = Null.SetNullInteger(dr["PermissionId"]);
            this.MenuId = Null.SetNullInteger(dr["MenuId"]);
            this.PermissionKey = Null.SetNullString(dr["PermissionKey"]);
            this.PermissionName = Null.SetNullString(dr["PermissionName"]);
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
                return this.PermissionId;
            }
            set
            {
                this.PermissionId = value;
            }
        }

        #endregion
    }
}
