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
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : FolderController
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Business Class that provides access to the Database for the functions within the calling classes
    /// Instantiates the instance of the DataProvider and returns the object, if any
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class FolderController
    {
        #region StorageLocationTypes enum

        public enum StorageLocationTypes
        {
            InsecureFileSystem = 0,
            SecureFileSystem = 1,
            DatabaseSecure = 2
        }

        #endregion

        #region Obsolete Methods

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.AddFolder(FolderMappingInfo folderMapping, string folderPath) ")]
        public int AddFolder(FolderInfo folder)
        {
            var tmpFolder = FolderManager.Instance.GetFolder(folder.PortalID, folder.FolderPath);

            if (tmpFolder != null && folder.FolderID == Null.NullInteger)
            {
                folder.FolderID = tmpFolder.FolderID;
            }

            if (folder.FolderID == Null.NullInteger)
            {
                folder = (FolderInfo)FolderManager.Instance.AddFolder(FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID), folder.FolderPath);
            }
            else
            {
                FolderManager.Instance.UpdateFolder(folder);
            }

            return folder.FolderID;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.DeleteFolder(IFolderInfo folder) ")]
        public void DeleteFolder(int PortalID, string FolderPath)
        {
            var folder = FolderManager.Instance.GetFolder(PortalID, FolderPath);
            if (folder != null)
            {
                FolderManager.Instance.DeleteFolder(folder);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolder(int portalID, string folderPath) ")]
        public FolderInfo GetFolder(int PortalID, string FolderPath, bool ignoreCache)
        {
            return (FolderInfo)FolderManager.Instance.GetFolder(PortalID, FolderPath);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolder(Guid uniqueId) ")]
        public FolderInfo GetFolderByUniqueID(Guid UniqueId)
        {
            return (FolderInfo)FolderManager.Instance.GetFolder(UniqueId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolder(int folderID) ")]
        public FolderInfo GetFolderInfo(int PortalID, int FolderID)
        {
            return (FolderInfo)FolderManager.Instance.GetFolder(FolderID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolders(int portalID, string permissions, int userID) ")]
        public SortedList<string, FolderInfo> GetFoldersByPermissionsSorted(int PortalID, string Permissions, int UserID)
        {
            var sortedFoldersToReturn = new SortedList<string, FolderInfo>();
            
            var sortedFolders = FolderManager.Instance.GetFolders(PortalID, Permissions, UserID);

            foreach (var folder in sortedFolders)
            {
                sortedFoldersToReturn.Add(folder.FolderPath, (FolderInfo)folder);
            }

            return sortedFoldersToReturn;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFolders(int portalID) ")]
        public SortedList<string, FolderInfo> GetFoldersSorted(int PortalID)
        {
            var sortedFoldersToReturn = new SortedList<string, FolderInfo>();

            var sortedFolders = FolderManager.Instance.GetFolders(PortalID);

            foreach (var folder in sortedFolders)
            {
                sortedFoldersToReturn.Add(folder.FolderPath, (FolderInfo)folder);
            }

            return sortedFoldersToReturn;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public string GetMappedDirectory(string virtualDirectory)
        {
            string mappedDir = Convert.ToString(DataCache.GetCache("DirMap:" + virtualDirectory));
            try
            {
                if (string.IsNullOrEmpty(mappedDir) && HttpContext.Current != null)
                {
                    mappedDir = FileSystemUtils.AddTrailingSlash(FileSystemUtils.MapPath(virtualDirectory));
                    DataCache.SetCache("DirMap:" + virtualDirectory, mappedDir);
                }
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
            return mappedDir;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public void SetMappedDirectory(string virtualDirectory)
        {
            try
            {
                string mappedDir = FileSystemUtils.AddTrailingSlash(FileSystemUtils.MapPath(virtualDirectory));
                DataCache.SetCache("DirMap:" + virtualDirectory, mappedDir);
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public void SetMappedDirectory(string virtualDirectory, HttpContext context)
        {
            try
            {
                // The logic here was updated to use the new FileSystemUtils.MapPath so that we have consistent behavior with other Overloads
                string mappedDir = FileSystemUtils.AddTrailingSlash(FileSystemUtils.MapPath(virtualDirectory));
                DataCache.SetCache("DirMap:" + virtualDirectory, mappedDir);
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public void SetMappedDirectory(PortalInfo portalInfo, HttpContext context)
        {
            try
            {
                string virtualDirectory = Common.Globals.ApplicationPath + "/" + portalInfo.HomeDirectory + "/";
                SetMappedDirectory(virtualDirectory, context);

            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.UpdateFolder(IFolderInfo folder) ")]
        public void UpdateFolder(FolderInfo objFolderInfo)
        {
            FolderManager.Instance.UpdateFolder(objFolderInfo);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0.  It has been replaced by GetFolderInfo(ByVal PortalID As Integer, ByVal FolderID As Integer) As FolderInfo ")]
        public ArrayList GetFolder(int PortalID, int FolderID)
        {
            var arrFolders = new ArrayList();
            FolderInfo folder = GetFolderInfo(PortalID, FolderID);
            if (folder != null)
            {
                arrFolders.Add(folder);
            }
            return arrFolders;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0.  It has been replaced by GetFolderInfo(ByVal PortalID As Integer, ByVal FolderID As Integer, ByVal ignoreCache As Boolean) ")]
        public FolderInfo GetFolder(int PortalID, string FolderPath)
        {
            return GetFolder(PortalID, FolderPath, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1.1.  It has been replaced by GetFolders(ByVal PortalID As Integer) As SortedList ")]
        public Dictionary<string, FolderInfo> GetFolders(int PortalID)
        {
            return new Dictionary<string, FolderInfo>(GetFoldersSorted(PortalID));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.0.  It has been replaced by GetFolders(ByVal PortalID As Integer) ")]
        public ArrayList GetFoldersByPortal(int PortalID)
        {
            var arrFolders = new ArrayList();
            foreach (KeyValuePair<string, FolderInfo> folderPair in GetFoldersSorted(PortalID))
            {
                arrFolders.Add(folderPair.Value);
            }
            return arrFolders;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by AddFolder(ByVal folder As FolderInfo)")]
        public int AddFolder(int PortalID, string FolderPath)
        {
            var objFolder = new FolderInfo();

            objFolder.UniqueId = Guid.NewGuid();
            objFolder.VersionGuid = Guid.NewGuid();
            objFolder.PortalID = PortalID;
            objFolder.FolderPath = FolderPath;
            objFolder.StorageLocation = (int) StorageLocationTypes.InsecureFileSystem;
            objFolder.IsProtected = false;
            objFolder.IsCached = false;

            return AddFolder(objFolder);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by AddFolder(ByVal folder As FolderInfo)")]
        public int AddFolder(int PortalID, string FolderPath, int StorageLocation, bool IsProtected, bool IsCached)
        {
            var objFolder = new FolderInfo();

            objFolder.UniqueId = Guid.NewGuid();
            objFolder.VersionGuid = Guid.NewGuid();
            objFolder.PortalID = PortalID;
            objFolder.FolderPath = FolderPath;
            objFolder.StorageLocation = StorageLocation;
            objFolder.IsProtected = IsProtected;
            objFolder.IsCached = IsCached;

            return AddFolder(objFolder);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by AddFolder(ByVal folder As FolderInfo)")]
        public int AddFolder(int PortalID, string FolderPath, int StorageLocation, bool IsProtected, bool IsCached, DateTime LastUpdated)
        {
            FolderPath = FileSystemUtils.FormatFolderPath(FolderPath);
            FolderInfo folder = GetFolder(PortalID, FolderPath, true);

            folder.StorageLocation = StorageLocation;
            folder.IsProtected = IsProtected;
            folder.IsCached = IsCached;
            folder.LastUpdated = Null.NullDate;

            DataCache.ClearFolderCache(PortalID);

            return AddFolder(folder);
        }

        #endregion
    }
}
