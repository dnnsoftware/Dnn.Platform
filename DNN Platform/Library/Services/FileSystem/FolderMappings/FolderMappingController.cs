// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem.Internal;

    public class FolderMappingController : ComponentBase<IFolderMappingController, FolderMappingController>, IFolderMappingController
    {
        private const string CacheKeyPrefix = "GetFolderMappingSettings";
        private static readonly DataProvider DataProvider = DataProvider.Instance();

        /// <summary>Initializes a new instance of the <see cref="FolderMappingController"/> class.</summary>
        internal FolderMappingController()
        {
        }

        /// <inheritdoc/>
        public FolderMappingInfo GetDefaultFolderMapping(int portalId)
        {
            var defaultFolderMapping = Config.GetSection("dotnetnuke/folder") != null ?
                this.GetFolderMappings(portalId).Find(fm => fm.FolderProviderType == Config.GetDefaultProvider("folder").Name) :
                this.GetFolderMapping(portalId, "Standard");
            return defaultFolderMapping ?? this.GetFolderMapping(portalId, "Standard");
        }

        /// <inheritdoc/>
        public int AddFolderMapping(FolderMappingInfo objFolderMapping)
        {
            objFolderMapping.FolderMappingID = DataProvider.AddFolderMapping(
                objFolderMapping.PortalID,
                objFolderMapping.MappingName,
                objFolderMapping.FolderProviderType,
                UserController.Instance.GetCurrentUserInfo().UserID);

            UpdateFolderMappingSettings(objFolderMapping);

            ClearFolderMappingCache(objFolderMapping.PortalID);

            return objFolderMapping.FolderMappingID;
        }

        /// <inheritdoc/>
        public void DeleteFolderMapping(int portalID, int folderMappingID)
        {
            var folderManager = FolderManager.Instance;
            var folders = folderManager.GetFolders(portalID);

            var folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingID);
            if (folderMappingFolders.Any())
            {
                // Delete files in folders with the provided mapping (only in the database)
                foreach (var file in folderMappingFolders.Select<IFolderInfo, IEnumerable<IFileInfo>>(folderManager.GetFiles).SelectMany(files => files))
                {
                    DataProvider.DeleteFile(portalID, file.FileName, file.FolderId);
                }

                // Remove the folders with the provided mapping that doesn't have child folders with other mapping (only in the database and filesystem)
                var folders1 = folders; // copy the variable to not access a modified closure
                var removableFolders = folders.Where(f =>
                    f.FolderMappingID == folderMappingID &&
                    !folders1.Any(f2 =>
                        f2.FolderID != f.FolderID &&
                        f2.FolderPath.StartsWith(f.FolderPath, StringComparison.OrdinalIgnoreCase) &&
                        f2.FolderMappingID != folderMappingID));

                if (removableFolders.Any())
                {
                    foreach (var removableFolder in removableFolders.OrderByDescending(rf => rf.FolderPath))
                    {
                        DirectoryWrapper.Instance.Delete(removableFolder.PhysicalPath, false);
                        DataProvider.DeleteFolder(portalID, removableFolder.FolderPath);
                    }
                }

                // Update the rest of folders with the provided mapping to use the standard mapping
                folders = folderManager.GetFolders(portalID, false); // re-fetch the folders

                folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingID);

                if (folderMappingFolders.Any())
                {
                    var defaultFolderMapping = this.GetDefaultFolderMapping(portalID);

                    foreach (var folderMappingFolder in folderMappingFolders)
                    {
                        folderMappingFolder.FolderMappingID = defaultFolderMapping.FolderMappingID;
                        folderManager.UpdateFolder(folderMappingFolder);
                    }
                }
            }

            DataProvider.DeleteFolderMapping(folderMappingID);
            ClearFolderMappingCache(portalID);
            ClearFolderMappingSettingsCache(folderMappingID);
        }

        /// <inheritdoc/>
        public void UpdateFolderMapping(FolderMappingInfo objFolderMapping)
        {
            DataProvider.UpdateFolderMapping(
                objFolderMapping.FolderMappingID,
                objFolderMapping.MappingName,
                objFolderMapping.Priority,
                UserController.Instance.GetCurrentUserInfo().UserID);

            ClearFolderMappingCache(objFolderMapping.PortalID);
            UpdateFolderMappingSettings(objFolderMapping);
        }

        /// <inheritdoc/>
        public FolderMappingInfo GetFolderMapping(int folderMappingID)
        {
            return CBO.FillObject<FolderMappingInfo>(DataProvider.GetFolderMapping(folderMappingID));
        }

        /// <inheritdoc/>
        public FolderMappingInfo GetFolderMapping(int portalId, int folderMappingID)
        {
            return this.GetFolderMappings(portalId).SingleOrDefault(fm => fm.FolderMappingID == folderMappingID);
        }

        /// <inheritdoc/>
        public FolderMappingInfo GetFolderMapping(int portalId, string mappingName)
        {
            return this.GetFolderMappings(portalId).SingleOrDefault(fm => fm.MappingName == mappingName);
        }

        /// <inheritdoc/>
        public List<FolderMappingInfo> GetFolderMappings(int portalId)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderMappingCacheKey, portalId);
            return CBO.GetCachedObject<List<FolderMappingInfo>>(
                new CacheItemArgs(
                cacheKey,
                DataCache.FolderMappingCacheTimeOut,
                DataCache.FolderMappingCachePriority),
                (c) => CBO.FillCollection<FolderMappingInfo>(DataProvider.GetFolderMappings(portalId)));
        }

        /// <inheritdoc/>
        public void AddDefaultFolderTypes(int portalID)
        {
            DataProvider.AddDefaultFolderTypes(portalID);
        }

        /// <inheritdoc/>
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
                    dr = DataProvider.GetFolderMappingSettings(folderMappingID);
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

                var intCacheTimeout = 20 * (int)Host.PerformanceSetting;
                DataCache.SetCache(strCacheKey, objSettings, TimeSpan.FromMinutes(intCacheTimeout));
            }

            return objSettings;
        }

        private static void UpdateFolderMappingSettings(FolderMappingInfo objFolderMapping)
        {
            foreach (string sKey in objFolderMapping.FolderMappingSettings.Keys)
            {
                UpdateFolderMappingSetting(objFolderMapping.FolderMappingID, sKey, Convert.ToString(objFolderMapping.FolderMappingSettings[sKey], CultureInfo.InvariantCulture));
            }

            ClearFolderMappingSettingsCache(objFolderMapping.FolderMappingID);
        }

        private static void UpdateFolderMappingSetting(int folderMappingId, string settingName, string settingValue)
        {
            IDataReader dr = null;
            try
            {
                dr = DataProvider.GetFolderMappingSetting(folderMappingId, settingName);
                if (dr.Read())
                {
                    DataProvider.UpdateFolderMappingSetting(folderMappingId, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID);
                }
                else
                {
                    DataProvider.AddFolderMappingSetting(folderMappingId, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID);
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
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.FolderMappingCacheKey, portalId);
            DataCache.RemoveCache(cacheKey);
        }

        private static void ClearFolderMappingSettingsCache(int folderMappingID)
        {
            DataCache.RemoveCache(CacheKeyPrefix + folderMappingID);
        }
    }
}
