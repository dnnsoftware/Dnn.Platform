#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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