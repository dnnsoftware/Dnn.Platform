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
            _folderPermissionID = Null.NullInteger;
            _folderPath = Null.NullString;
            _portalID = Null.NullInteger;
            _folderID = Null.NullInteger;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a new FolderPermissionInfo
        /// </summary>
        /// <param name="permission">A PermissionInfo object</param>
        /// -----------------------------------------------------------------------------
        public FolderPermissionInfo(PermissionInfo permission) : this()
        {
            ModuleDefID = permission.ModuleDefID;
            PermissionCode = permission.PermissionCode;
            PermissionID = permission.PermissionID;
            PermissionKey = permission.PermissionKey;
            PermissionName = permission.PermissionName;
        }
		
		#endregion
		
		#region "Public Properties"

        [XmlIgnore]
        public int FolderPermissionID
        {
            get
            {
                return _folderPermissionID;
            }
            set
            {
                _folderPermissionID = value;
            }
        }

        [XmlIgnore]
        public int FolderID
        {
            get
            {
                return _folderID;
            }
            set
            {
                _folderID = value;
            }
        }

        [XmlIgnore]
        public int PortalID
        {
            get
            {
                return _portalID;
            }
            set
            {
                _portalID = value;
            }
        }

        [XmlElement("folderpath")]
        public string FolderPath
        {
            get
            {
                return _folderPath;
            }
            set
            {
                _folderPath = value;
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
            FolderPermissionID = Null.SetNullInteger(dr["FolderPermissionID"]);
            FolderID = Null.SetNullInteger(dr["FolderID"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            FolderPath = Null.SetNullString(dr["FolderPath"]);
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
                return FolderPermissionID;
            }
            set
            {
                FolderPermissionID = value;
            }
        }

        #endregion
    }
}
