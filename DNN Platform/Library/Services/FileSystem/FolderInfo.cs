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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    [XmlRoot("folder", IsNullable = false)]
    [Serializable]
    public class FolderInfo : BaseEntityInfo, IHydratable, IFolderInfo
    {
        // local property declarations
        private string _displayName;
        private string _displayPath;
        private FolderPermissionCollection _folderPermissions;
        private int _folderMappingId;

        #region Constructors

        public FolderInfo(): this(false)
        {            
        }

        internal FolderInfo(bool initialiseEmptyPermissions)
        {
            FolderID = Null.NullInteger;
            UniqueId = Guid.NewGuid();
            VersionGuid = Guid.NewGuid();
            WorkflowID = Null.NullInteger;            
            if (initialiseEmptyPermissions)
            {
                _folderPermissions = new FolderPermissionCollection();   
            }            
        }
        #endregion

        #region Public Properties

        [XmlElement("folderid")]
        public int FolderID { get; set; }

        [XmlElement("uniqueid")]
        public Guid UniqueId { get; set; }

        [XmlElement("versionguid")]
        public Guid VersionGuid { get; set; }

        [XmlElement("foldername")]
        public string FolderName
        {
            get
            {
                string folderName = PathUtils.Instance.RemoveTrailingSlash(FolderPath);
                if (folderName.Length > 0 && folderName.LastIndexOf("/", StringComparison.Ordinal) > -1)
                {
                    folderName = folderName.Substring(folderName.LastIndexOf("/", StringComparison.Ordinal) + 1);
                }
                return folderName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the folder has any child subfolder
        /// </summary>
        [XmlElement("haschildren")]
        public bool HasChildren
        {
            get
            {
                return FolderManager.Instance.GetFolders(this).Any();
            }
        }

        [XmlElement("displayname")]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    _displayName = FolderName;
                }
                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }

        [XmlElement("folderpath")]
        public string FolderPath { get; set; }

        [XmlElement("displaypath")]
        public string DisplayPath
        {
            get
            {
                if (string.IsNullOrEmpty(_displayPath))
                {
                    _displayPath = FolderPath;
                }
                return _displayPath;
            }
            set
            {
                _displayPath = value;
            }
        }

        [XmlElement("iscached")]
        public bool IsCached { get; set; }

        [XmlElement("isprotected")]
        public bool IsProtected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether file versions are active for the folder
        /// </summary>
        [XmlElement("isversioned")]
        public bool IsVersioned { get; set; }

        /// <summary>
        /// Gets or sets the path this folder is mapped on its provider file system
        /// </summary>
        [XmlElement("mappedpath")]
        public string MappedPath { get; set; }

        /// <summary>
        /// Gets or sets a reference to the active Workflow for the folder
        /// </summary>
        [XmlElement("workflowid")]
        public int WorkflowID { get; set; }

        [XmlIgnore]
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Gets or sets a reference to the parent folder
        /// </summary>
        [XmlElement("parentid")]
        public int ParentID { get; set; }

        [XmlElement("physicalpath")]
        public string PhysicalPath
        {
            get
            {
                string physicalPath;
                PortalSettings portalSettings = null;
                if (HttpContext.Current != null)
                {
                    portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                }

                if (PortalID == Null.NullInteger)
                {
                    physicalPath = Globals.HostMapPath + FolderPath;
                }
                else
                {
                    if (portalSettings == null || portalSettings.PortalId != PortalID)
                    {
                        //Get the PortalInfo  based on the Portalid
                        var portal = PortalController.Instance.GetPortal(PortalID);

                        physicalPath = portal.HomeDirectoryMapPath + FolderPath;
                    }
                    else
                    {
                        physicalPath = portalSettings.HomeDirectoryMapPath + FolderPath;
                    }
                }

                return physicalPath.Replace("/", "\\");
            }
        }

        [XmlElement("portalid")]
        public int PortalID { get; set; }

        [XmlElement("storagelocation")]
        public int StorageLocation { get; set; }

        [XmlElement("folderpermissions")]
        public FolderPermissionCollection FolderPermissions
        {
            get 
            {
                return _folderPermissions ?? (_folderPermissions = new FolderPermissionCollection(FolderPermissionController.GetFolderPermissionsCollectionByFolder(PortalID, FolderPath)));
            }
        }

        public int FolderMappingID
        {
            get
            {
                if (_folderMappingId == 0)
                {
                    switch (StorageLocation)
                    {
                        case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                            _folderMappingId = FolderMappingController.Instance.GetFolderMapping(PortalID, "Standard").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                            _folderMappingId = FolderMappingController.Instance.GetFolderMapping(PortalID, "Secure").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                            _folderMappingId = FolderMappingController.Instance.GetFolderMapping(PortalID, "Database").FolderMappingID;
                            break;
                        default:
                            _folderMappingId = FolderMappingController.Instance.GetDefaultFolderMapping(PortalID).FolderMappingID;
                            break;
                    }
                }

                return _folderMappingId;
            }
            set
            {
                _folderMappingId = value;
            }
        }

        public bool IsStorageSecure
        {
            get
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(PortalID, FolderMappingID);
                return FolderProvider.Instance(folderMapping.FolderProviderType).IsStorageSecure;
            }   
        }

        #endregion

        #region IHydratable Implementation

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Fills a FolderInfo from a Data Reader
        /// </summary>
        /// <param name = "dr">The Data Reader to use</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            FolderID = Null.SetNullInteger(dr["FolderID"]);
            UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            FolderPath = Null.SetNullString(dr["FolderPath"]);
            MappedPath = Null.SetNullString(dr["MappedPath"]);
            IsCached = Null.SetNullBoolean(dr["IsCached"]);
            IsProtected = Null.SetNullBoolean(dr["IsProtected"]);
            StorageLocation = Null.SetNullInteger(dr["StorageLocation"]);
            LastUpdated = Null.SetNullDateTime(dr["LastUpdated"]);
            FolderMappingID = Null.SetNullInteger(dr["FolderMappingID"]);
            IsVersioned = Null.SetNullBoolean(dr["IsVersioned"]);
            WorkflowID = Null.SetNullInteger(dr["WorkflowID"]);
            ParentID = Null.SetNullInteger(dr["ParentID"]);
            FillBaseProperties(dr);            
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets and sets the Key ID
        /// </summary>
        /// <returns>An Integer</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return FolderID;
            }
            set
            {
                FolderID = value;
            }
        }

        #endregion

        #region Obsolete Methods

        [Obsolete("Deprecated in DNN 7.1.  Use the parameterless constructor and object initializers")]
        public FolderInfo(int portalId, string folderpath, int storageLocation, bool isProtected, bool isCached, DateTime lastUpdated)
            : this(Guid.NewGuid(), portalId, folderpath, storageLocation, isProtected, isCached, lastUpdated)
        {
        }

        [Obsolete("Deprecated in DNN 7.1.  Use the parameterless constructor and object initializers")]
        public FolderInfo(Guid uniqueId, int portalId, string folderpath, int storageLocation, bool isProtected, bool isCached, DateTime lastUpdated)
        {
            FolderID = Null.NullInteger;
            UniqueId = uniqueId;
            VersionGuid = Guid.NewGuid();
            WorkflowID = Null.NullInteger;

            PortalID = portalId;
            FolderPath = folderpath;
            StorageLocation = storageLocation;
            IsProtected = isProtected;
            IsCached = isCached;
            LastUpdated = lastUpdated;
        }

        #endregion
    }
}
