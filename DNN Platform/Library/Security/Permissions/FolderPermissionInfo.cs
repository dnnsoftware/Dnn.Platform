// Licensed to the .NET Foundation under one or more agreements.
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
    [Serializable]
    public class FolderPermissionInfo : PermissionInfoBase, IHydratable
    {
		#region "Private Members"
		
        //local property declarations
        private int _folderID;
        private string _folderPath;
        private int _folderPermissionID;
        private int _portalID;
		
		#endregion
		
		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new FolderPermissionInfo
        /// </summary>
        /// -----------------------------------------------------------------------------
        public FolderPermissionInfo()
        {
            this._folderPermissionID = Null.NullInteger;
            this._folderPath = Null.NullString;
            this._portalID = Null.NullInteger;
            this._folderID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new FolderPermissionInfo
        /// </summary>
        /// <param name="permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public FolderPermissionInfo(PermissionInfo permission) : this()
        {
            this.ModuleDefID = permission.ModuleDefID;
            this.PermissionCode = permission.PermissionCode;
            this.PermissionID = permission.PermissionID;
            this.PermissionKey = permission.PermissionKey;
            this.PermissionName = permission.PermissionName;
        }
		
		#endregion
		
		#region "Public Properties"

        [XmlIgnore]
        public int FolderPermissionID
        {
            get
            {
                return this._folderPermissionID;
            }
            set
            {
                this._folderPermissionID = value;
            }
        }

        [XmlIgnore]
        public int FolderID
        {
            get
            {
                return this._folderID;
            }
            set
            {
                this._folderID = value;
            }
        }

        [XmlIgnore]
        public int PortalID
        {
            get
            {
                return this._portalID;
            }
            set
            {
                this._portalID = value;
            }
        }

        [XmlElement("folderpath")]
        public string FolderPath
        {
            get
            {
                return this._folderPath;
            }
            set
            {
                this._folderPath = value;
            }
        }
		
		#endregion

        #region IHydratable Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fills a FolderPermissionInfo from a Data Reader
        /// </summary>
        /// <param name="dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            base.FillInternal(dr);
            this.FolderPermissionID = Null.SetNullInteger(dr["FolderPermissionID"]);
            this.FolderID = Null.SetNullInteger(dr["FolderID"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.FolderPath = Null.SetNullString(dr["FolderPath"]);
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
                return this.FolderPermissionID;
            }
            set
            {
                this.FolderPermissionID = value;
            }
        }

        #endregion
    }
}
