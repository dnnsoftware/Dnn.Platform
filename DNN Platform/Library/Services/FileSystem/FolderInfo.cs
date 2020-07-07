// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
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

    [XmlRoot("folder", IsNullable = false)]
    [Serializable]
    public class FolderInfo : BaseEntityInfo, IHydratable, IFolderInfo
    {
        // local property declarations
        private string _displayName;
        private string _displayPath;
        private FolderPermissionCollection _folderPermissions;
        private int _folderMappingId;

        public FolderInfo()
            : this(false)
        {
        }

        [Obsolete("Deprecated in DNN 7.1.  Use the parameterless constructor and object initializers. Scheduled removal in v10.0.0.")]
        public FolderInfo(int portalId, string folderpath, int storageLocation, bool isProtected, bool isCached, DateTime lastUpdated)
            : this(Guid.NewGuid(), portalId, folderpath, storageLocation, isProtected, isCached, lastUpdated)
        {
        }

        [Obsolete("Deprecated in DNN 7.1.  Use the parameterless constructor and object initializers. Scheduled removal in v10.0.0.")]
        public FolderInfo(Guid uniqueId, int portalId, string folderpath, int storageLocation, bool isProtected, bool isCached, DateTime lastUpdated)
        {
            this.FolderID = Null.NullInteger;
            this.UniqueId = uniqueId;
            this.VersionGuid = Guid.NewGuid();
            this.WorkflowID = Null.NullInteger;

            this.PortalID = portalId;
            this.FolderPath = folderpath;
            this.StorageLocation = storageLocation;
            this.IsProtected = isProtected;
            this.IsCached = isCached;
            this.LastUpdated = lastUpdated;
        }

        internal FolderInfo(bool initialiseEmptyPermissions)
        {
            this.FolderID = Null.NullInteger;
            this.UniqueId = Guid.NewGuid();
            this.VersionGuid = Guid.NewGuid();
            this.WorkflowID = Null.NullInteger;
            if (initialiseEmptyPermissions)
            {
                this._folderPermissions = new FolderPermissionCollection();
            }
        }

        [XmlElement("uniqueid")]
        public Guid UniqueId { get; set; }

        [XmlElement("versionguid")]
        public Guid VersionGuid { get; set; }

        [XmlElement("foldername")]
        public string FolderName
        {
            get
            {
                string folderName = PathUtils.Instance.RemoveTrailingSlash(this.FolderPath);
                if (folderName.Length > 0 && folderName.LastIndexOf("/", StringComparison.Ordinal) > -1)
                {
                    folderName = folderName.Substring(folderName.LastIndexOf("/", StringComparison.Ordinal) + 1);
                }

                return folderName;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the folder has any child subfolder.
        /// </summary>
        [XmlElement("haschildren")]
        public bool HasChildren
        {
            get
            {
                return FolderManager.Instance.GetFolders(this).Any();
            }
        }

        /// <summary>
        /// Gets or sets a reference to the active Workflow for the folder.
        /// </summary>
        [XmlElement("workflowid")]
        public int WorkflowID { get; set; }

        /// <summary>
        /// Gets or sets a reference to the parent folder.
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

                if (this.PortalID == Null.NullInteger)
                {
                    physicalPath = Globals.HostMapPath + this.FolderPath;
                }
                else
                {
                    if (portalSettings == null || portalSettings.PortalId != this.PortalID)
                    {
                        // Get the PortalInfo  based on the Portalid
                        var portal = PortalController.Instance.GetPortal(this.PortalID);

                        physicalPath = portal.HomeDirectoryMapPath + this.FolderPath;
                    }
                    else
                    {
                        physicalPath = portalSettings.HomeDirectoryMapPath + this.FolderPath;
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
                return this._folderPermissions ?? (this._folderPermissions = new FolderPermissionCollection(FolderPermissionController.GetFolderPermissionsCollectionByFolder(this.PortalID, this.FolderPath)));
            }
        }

        public bool IsStorageSecure
        {
            get
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(this.PortalID, this.FolderMappingID);
                return FolderProvider.Instance(folderMapping.FolderProviderType).IsStorageSecure;
            }
        }

        [XmlElement("folderid")]
        public int FolderID { get; set; }


        [XmlElement("displayname")]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this._displayName))
                {
                    this._displayName = this.FolderName;
                }

                return this._displayName;
            }

            set
            {
                this._displayName = value;
            }
        }

        [XmlElement("folderpath")]
        public string FolderPath { get; set; }

        [XmlElement("displaypath")]
        public string DisplayPath
        {
            get
            {
                if (string.IsNullOrEmpty(this._displayPath))
                {
                    this._displayPath = this.FolderPath;
                }

                return this._displayPath;
            }

            set
            {
                this._displayPath = value;
            }
        }

        [XmlElement("iscached")]
        public bool IsCached { get; set; }

        [XmlElement("isprotected")]
        public bool IsProtected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether file versions are active for the folder.
        /// </summary>
        [XmlElement("isversioned")]
        public bool IsVersioned { get; set; }

        /// <summary>
        /// Gets or sets the path this folder is mapped on its provider file system.
        /// </summary>
        [XmlElement("mappedpath")]
        public string MappedPath { get; set; }


        [XmlIgnore]
        public DateTime LastUpdated { get; set; }


        public int FolderMappingID
        {
            get
            {
                if (this._folderMappingId == 0)
                {
                    switch (this.StorageLocation)
                    {
                        case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                            this._folderMappingId = FolderMappingController.Instance.GetFolderMapping(this.PortalID, "Standard").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                            this._folderMappingId = FolderMappingController.Instance.GetFolderMapping(this.PortalID, "Secure").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                            this._folderMappingId = FolderMappingController.Instance.GetFolderMapping(this.PortalID, "Database").FolderMappingID;
                            break;
                        default:
                            this._folderMappingId = FolderMappingController.Instance.GetDefaultFolderMapping(this.PortalID).FolderMappingID;
                            break;
                    }
                }

                return this._folderMappingId;
            }

            set
            {
                this._folderMappingId = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets and sets the Key ID.
        /// </summary>
        /// <returns>An Integer.</returns>
        /// -----------------------------------------------------------------------------
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.FolderID;
            }

            set
            {
                this.FolderID = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Fills a FolderInfo from a Data Reader.
        /// </summary>
        /// <param name = "dr">The Data Reader to use.</param>
        /// -----------------------------------------------------------------------------
        public void Fill(IDataReader dr)
        {
            this.FolderID = Null.SetNullInteger(dr["FolderID"]);
            this.UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            this.VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.FolderPath = Null.SetNullString(dr["FolderPath"]);
            this.MappedPath = Null.SetNullString(dr["MappedPath"]);
            this.IsCached = Null.SetNullBoolean(dr["IsCached"]);
            this.IsProtected = Null.SetNullBoolean(dr["IsProtected"]);
            this.StorageLocation = Null.SetNullInteger(dr["StorageLocation"]);
            this.LastUpdated = Null.SetNullDateTime(dr["LastUpdated"]);
            this.FolderMappingID = Null.SetNullInteger(dr["FolderMappingID"]);
            this.IsVersioned = Null.SetNullBoolean(dr["IsVersioned"]);
            this.WorkflowID = Null.SetNullInteger(dr["WorkflowID"]);
            this.ParentID = Null.SetNullInteger(dr["ParentID"]);
            this.FillBaseProperties(dr);
        }
    }
}
