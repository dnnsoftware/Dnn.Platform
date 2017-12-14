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
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem.Internal;

namespace DotNetNuke.Services.FileSystem
{
    public class FolderMappingController : ComponentBase<IFolderMappingController, FolderMappingController>, IFolderMappingController
    {
        #region Constructor

        internal FolderMappingController()
        {
        }

        #endregion

        #region Private Variables

        private static readonly DataProvider dataProvider = DataProvider.Instance();
        private const string CacheKeyPrefix = "GetFolderMappingSettings";

        #endregion

        #region Public Methods

        public FolderMappingInfo GetDefaultFolderMapping(int portalId)
        {
            var defaultFolderMapping = Config.GetSection("dotnetnuke/folder") != null ?
                GetFolderMappings(portalId).Find(fm => fm.FolderProviderType == Config.GetDefaultProvider("folder").Name) :
                GetFolderMapping(portalId, "Standard");
            return defaultFolderMapping ?? GetFolderMapping(portalId, "Standard");
        }

        public int AddFolderMapping(FolderMappingInfo objFolderMapping)
        {
            objFolderMapping.FolderMappingID = dataProvider.AddFolderMapping(objFolderMapping.PortalID,
                                                                             objFolderMapping.MappingName,
                                                                             objFolderMapping.FolderProviderType,
                                                                             UserController.Instance.GetCurrentUserInfo().UserID);

            UpdateFolderMappingSettings(objFolderMapping);

            ClearFolderMappingCache(objFolderMapping.PortalID);

            return objFolderMapping.FolderMappingID;
        }

        public void DeleteFolderMapping(int portalID, int folderMappingID)
        {
            var folderManager = FolderManager.Instance;
            var folders = folderManager.GetFolders(portalID);

            var folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingID);

            if (folderMappingFolders.Count() > 0)
            {
                // Delete files in folders with the provided mapping (only in the database)
                foreach (var file in folderMappingFolders.Select<IFolderInfo, IEnumerable<IFileInfo>>(folderManager.GetFiles).SelectMany(files => files))
                {
                    dataProvider.DeleteFile(portalID, file.FileName, file.FolderId);
                }

                // Remove the folders with the provided mapping that doesn't have child folders with other mapping (only in the database and filesystem)
                var folders1 = folders; // copy the variable to not access a modified closure
                var removableFolders = folders.Where(f => f.FolderMappingID == folderMappingID && !folders1.Any(f2 => f2.FolderID != f.FolderID &&
                                f2.FolderPath.StartsWith(f.FolderPath) && f2.FolderMappingID != folderMappingID));

                if (removableFolders.Count() > 0)
                {
                    foreach (var removableFolder in removableFolders.OrderByDescending(rf => rf.FolderPath))
                    {
                        DirectoryWrapper.Instance.Delete(removableFolder.PhysicalPath, false);
                        dataProvider.DeleteFolder(portalID, removableFolder.FolderPath);
                    }
                }

                // Update the rest of folders with the provided mapping to use the standard mapping
                folders = folderManager.GetFolders(portalID, false); // re-fetch the folders

                folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingID);

                if (folderMappingFolders.Count() > 0)
                {
                    var defaultFolderMapping = GetDefaultFolderMapping(portalID);

                    foreach (var folderMappingFolder in folderMappingFolders)
                    {
                        folderMappingFolder.FolderMappingID = defaultFolderMapping.FolderMappingID;
                        folderManager.UpdateFolder(folderMappingFolder);
                    }
                }
            }

            dataProvider.DeleteFolderMapping(folderMappingID);
            ClearFolderMappingCache(portalID);
            ClearFolderMappingSettingsCache(folderMappingID);
        }

        public void UpdateFolderMapping(FolderMappingInfo objFolderMapping)
        {
            dataProvider.UpdateFolderMapping(objFolderMapping.FolderMappingID,
                                             objFolderMapping.MappingName,
                                             objFolderMapping.Priority,
                                             UserController.Instance.GetCurrentUserInfo().UserID);

            ClearFolderMappingCache(objFolderMapping.PortalID);
            UpdateFolderMappingSettings(objFolderMapping);
        }

        public FolderMappingInfo GetFolderMapping(int folderMappingID)
        {
            return CBO.FillObject<FolderMappingInfo>(dataProvider.GetFolderMapping(folderMappingID));
        }

        public FolderMappingInfo GetFolderMapping(int portalId, int folderMappingID)
        {
            return GetFolderMappings(portalId).SingleOrDefault(fm => fm.FolderMappingID == folderMappingID);
        }

        public FolderMappingInfo GetFolderMapping(int portalId, string mappingName)
        {
            return GetFolderMappings(portalId).SingleOrDefault(fm => fm.MappingName == mappingName);
        }

        public List<FolderMappingInfo> GetFolderMappings(int portalId)
        {
            var cacheKey = String.Format(DataCache.FolderMappingCacheKey, portalId);
            return CBO.GetCachedObject<List<FolderMappingInfo>>(new CacheItemArgs(cacheKey,
                                                                    DataCache.FolderMappingCacheTimeOut,
                                                                    DataCache.FolderMappingCachePriority),
                                                                (c) => CBO.FillCollection<FolderMappingInfo>(dataProvider.GetFolderMappings(portalId)));
        }

        public void AddDefaultFolderTypes(int portalID)
        {
            dataProvider.AddDefaultFolderTypes(portalID);
        }

        public Hashtable GetFolderMappingSettings(int folderMappingID)
        {
            var strCacheKey = CacheKeyPrefix + folderMappingID;
            var objSettings = (Hashtable)DataCache.GetCache(strCacheKey);
            if (objSettings == null)
            {
                objSettings = new Hashtable();
                IDataReader dr = null;
                try
                {
                    dr = dataProvider.GetFolderMappingSettings(folderMappingID);
                    while (dr.Read())
                    {
                        if (!dr.IsDBNull(1))
                        {
                            objSettings[dr.GetString(0)] = dr.GetString(1);
                        }
                        else
                        {
                            objSettings[dr.GetString(0)] = string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }
                var intCacheTimeout = 20 * Convert.ToInt32(Host.PerformanceSetting);
                DataCache.SetCache(strCacheKey, objSettings, TimeSpan.FromMinutes(intCacheTimeout));
            }
            return objSettings;
        }

        #endregion

        #region Private Methods

        private static void UpdateFolderMappingSettings(FolderMappingInfo objFolderMapping)
        {
            foreach (string sKey in objFolderMapping.FolderMappingSettings.Keys)
            {
                UpdateFolderMappingSetting(objFolderMapping.FolderMappingID, sKey, Convert.ToString(objFolderMapping.FolderMappingSettings[sKey]));
            }

            ClearFolderMappingSettingsCache(objFolderMapping.FolderMappingID);
        }

        private static void UpdateFolderMappingSetting(int folderMappingID, string settingName, string settingValue)
        {
            IDataReader dr = null;
            try
            {
                dr = dataProvider.GetFolderMappingSetting(folderMappingID, settingName);
                if (dr.Read())
                {
                    dataProvider.UpdateFolderMappingSetting(folderMappingID, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID);
                }
                else
                {
                    dataProvider.AddFolderMappingSetting(folderMappingID, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID);
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }
            finally
            {
                // Ensure DataReader is closed
                CBO.CloseDataReader(dr, true);
            }
        }

        private static void ClearFolderMappingCache(int portalId)
        {
            var cacheKey = String.Format(DataCache.FolderMappingCacheKey, portalId);
            DataCache.RemoveCache(cacheKey);
        }

        private static void ClearFolderMappingSettingsCache(int folderMappingID)
        {
            DataCache.RemoveCache(CacheKeyPrefix + folderMappingID);
        }

        #endregion
    }
}