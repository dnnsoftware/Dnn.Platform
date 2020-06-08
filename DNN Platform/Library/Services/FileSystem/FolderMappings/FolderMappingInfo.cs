// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.FileSystem.Internal;

namespace DotNetNuke.Services.FileSystem
{
    /// <summary>
    ///   Represents the FolderMapping object and holds the Properties of that object
    /// </summary>
    [Serializable]
    public class FolderMappingInfo : IHydratable
    {
        private Hashtable _folderMappingSettings;

        #region "Public Properties"

        public int FolderMappingID { get; set; }
        public int PortalID { get; set; }
        public string MappingName { get; set; }
        public string FolderProviderType { get; set; }
        public int Priority { get; set; }

        public Hashtable FolderMappingSettings
        {
            get
            {
                if (_folderMappingSettings == null)
                {
                    if (FolderMappingID == Null.NullInteger)
                    {
                        _folderMappingSettings = new Hashtable();
                    }
                    else
                    {
                        _folderMappingSettings = FolderMappingController.Instance.GetFolderMappingSettings(FolderMappingID);
                    }
                }
                return _folderMappingSettings;
            }
        }

        private string _imageUrl;
        public string ImageUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_imageUrl))
                {
                    _imageUrl = FolderProvider.Instance(FolderProviderType).GetFolderProviderIconPath();
                }
                
                return _imageUrl;
            }
        }

        public bool IsEditable
        {
            get
            {
                return !DefaultFolderProviders.GetDefaultProviders().Contains(FolderProviderType);
            }
        }

        public bool SyncAllSubFolders
        {
            get
            {
                if(FolderMappingSettings.ContainsKey("SyncAllSubFolders"))
                {
                    return bool.Parse(FolderMappingSettings["SyncAllSubFolders"].ToString());
                }

                return true;
            }
            set
            {
                FolderMappingSettings["SyncAllSubFolders"] = value;
            }
        }

        #endregion

        #region "Constructors"

        public FolderMappingInfo()
        {
            FolderMappingID = Null.NullInteger;
            PortalID = Null.NullInteger;
        }

        public FolderMappingInfo(int portalID, string mappingName, string folderProviderType)
        {
            FolderMappingID = Null.NullInteger;
            PortalID = portalID;
            MappingName = mappingName;
            FolderProviderType = folderProviderType;
        }

        #endregion

        #region "IHydratable Implementation"

        /// <summary>
        ///   Fills a FolderInfo from a Data Reader
        /// </summary>
        /// <param name = "dr">The Data Reader to use</param>
        public void Fill(IDataReader dr)
        {
            FolderMappingID = Null.SetNullInteger(dr["FolderMappingID"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            MappingName = Null.SetNullString(dr["MappingName"]);
            FolderProviderType = Null.SetNullString(dr["FolderProviderType"]);
            Priority = Null.SetNullInteger(dr["Priority"]);
        }

        /// <summary>
        ///   Gets and sets the Key ID
        /// </summary>
        public int KeyID
        {
            get
            {
                return FolderMappingID;
            }
            set
            {
                FolderMappingID = value;
            }
        }

        #endregion
    }
}
