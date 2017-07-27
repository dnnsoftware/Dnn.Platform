#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Caching;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Providers.FolderProviders.Components
{
    public abstract class BaseRemoteStorageProvider : FolderProvider
    {
        #region Private Members

        private readonly string _encryptionKey = Host.GUID;
        private readonly PortalSecurity _portalSecurity = new PortalSecurity();
        #endregion

        #region Private Methods

        private IRemoteStorageItem GetStorageItemInternal(FolderMappingInfo folderMapping, string key)
        {
            var cacheKey = string.Format(ObjectCacheKey, folderMapping.FolderMappingID, key);

            return CBO.GetCachedObject<IRemoteStorageItem>(new CacheItemArgs(cacheKey,
                ObjectCacheTimeout,
                CacheItemPriority.Default,
                folderMapping.FolderMappingID),
                c =>
                {
                    var list = GetObjectList(folderMapping, key);
                    
                    //return list.FirstOrDefault(i => i.Key == key);
                    return list.FirstOrDefault(i => i.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                });
            
        }
        
        #endregion

        #region Protected Properties

        protected virtual string FileNotFoundMessage
        {
            get { return String.Empty; }
        }
        
        protected virtual string ObjectCacheKey
        {
            get { return String.Empty; }
        }

        protected virtual int ObjectCacheTimeout
        {
            get { return 150; }
        }

        protected virtual string ListObjectsCacheKey
        {
            get { return String.Empty; }
        }

        protected virtual int ListObjectsCacheTimeout
        {
            get { return 300; }
        }

        #endregion

        public override bool SupportsMappedPaths
        {
            get { return true; }
        }

        public override bool SupportsMoveFile
        {
            get { return false; }
        }

        public override bool SupportsMoveFolder
        {
            get { return true; }
        }

        #region Protected Methods

        protected abstract void CopyFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void DeleteFileInternal(FolderMappingInfo folderMapping, string uri);

        protected abstract void DeleteFolderInternal(FolderMappingInfo folderMapping, IFolderInfo folder);

        protected static bool GetBooleanSetting(FolderMappingInfo folderMapping, string settingName)
        {
            return Boolean.Parse(folderMapping.FolderMappingSettings[settingName].ToString());
        }
        
        protected abstract Stream GetFileStreamInternal(FolderMappingInfo folderMapping, string uri);

        protected abstract IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping);

        protected virtual IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping, string path)
        {
            return GetObjectList(folderMapping);
        }

        protected static string GetSetting(FolderMappingInfo folderMapping, string settingName)
        {
            Requires.NotNull(nameof(folderMapping), folderMapping);
            Requires.NotNullOrEmpty(nameof(settingName), settingName);
            
            return folderMapping.FolderMappingSettings[settingName]?.ToString();
        }

        protected abstract void MoveFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void MoveFolderInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void UpdateFileInternal(Stream stream, FolderMappingInfo folderMapping, string uri);

        protected virtual IRemoteStorageItem GetStorageItem(FolderMappingInfo folderMapping, string key)
        {
            return GetStorageItemInternal(folderMapping, key);
        }
        #endregion

        /// <summary>
        /// Adds a new folder
        /// </summary>
        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping)
        {
            AddFolder(folderPath, folderMapping, folderPath);
        }

        /// <summary>
        /// Adds a new file to the specified folder.
        /// </summary>
        public override void AddFile(IFolderInfo folder, string fileName, Stream content)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            UpdateFile(folder, fileName, content);
        }

        public virtual void ClearCache(int folderMappingId)
        {
            var cacheKey = String.Format(ListObjectsCacheKey, folderMappingId);
            DataCache.RemoveCache(cacheKey);
            //Clear cached objects
            DataCache.ClearCache(String.Format(ObjectCacheKey, folderMappingId, String.Empty));
        }

        /// <summary>
        /// Copies the specified file to the destination folder.
        /// </summary>
        public override void CopyFile(string folderPath, string fileName, string newFolderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            if (folderPath == newFolderPath) return;

            CopyFileInternal(folderMapping, folderPath + fileName, newFolderPath + fileName);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        public override void DeleteFile(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            DeleteFileInternal(folderMapping, folder.MappedPath + file.FileName);
        }

        /// <remarks>
        /// Azure Storage doesn't support folders, but we need to delete the file added while creating the folder.
        /// </remarks>
        public override void DeleteFolder(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            DeleteFolderInternal(folderMapping, folder);
        }

        /// <summary>
        /// Checks the existence of the specified file.
        /// </summary>
        public override bool FileExists(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var item = GetStorageItem(folderMapping, folder.MappedPath + fileName);
            return (item != null);
        }

        /// <summary>
        /// Checks the existence of the specified folder.
        /// </summary>
        public override bool FolderExists(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            //the root folder should always exist.
            if (folderPath == "")
            {
                return true;
            }

            var list = GetObjectList(folderMapping, folderPath);

            return list.Any(o => o.Key.StartsWith(folderPath, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <remarks>
        /// Amazon doesn't support file attributes.
        /// </remarks>
        public override FileAttributes? GetFileAttributes(IFileInfo file)
        {
            return null;
        }

        /// <summary>
        /// Gets the list of file names contained in the specified folder.
        /// </summary>
        public override string[] GetFiles(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var list = GetObjectList(folderMapping, folder.MappedPath);
            var mappedPath = folder.MappedPath;

            //return (from i in list
            //        let f = i.Key
            //        let r = (!string.IsNullOrEmpty(mappedPath) ? f.Replace(mappedPath, "") : f)
            //        where f.StartsWith(mappedPath, true, CultureInfo.InvariantCulture) && f.Length > mappedPath.Length && r.IndexOf("/", StringComparison.Ordinal) == -1
            //        select Path.GetFileName(f)).ToArray();

            var pattern = "^" + mappedPath;
            return  (from i in list
                    let f = i.Key
                     let r = (!string.IsNullOrEmpty(mappedPath) ? Regex.Replace(f, pattern, "", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2)) : f)
                    where f.StartsWith(mappedPath, true, CultureInfo.InvariantCulture) && f.Length > mappedPath.Length && r.IndexOf("/", StringComparison.Ordinal) == -1
                    select Path.GetFileName(f)).ToArray();

        }

        /// <summary>
        /// Gets the file length.
        /// </summary>
        public override long GetFileSize(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            var item = GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
            if (item == null)
            {
                throw new FileNotFoundException(FileNotFoundMessage, file.RelativePath);
            }
            return item.Size;

        }

        /// <summary>
        ///   Gets a file Stream of the specified file.
        /// </summary>
        public override Stream GetFileStream(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            return GetFileStreamInternal(folderMapping, folder.MappedPath + file.FileName);
        }

        /// <summary>
        /// Gets a file Stream of the specified file.
        /// </summary>
        public override Stream GetFileStream(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            return GetFileStreamInternal(folderMapping, folder.MappedPath + fileName);
        }

        /// <summary>
        /// Gets the time when the specified file was last modified.
        /// </summary>
        public override DateTime GetLastModificationTime(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            var item = GetStorageItem(folderMapping, folder.MappedPath+file.FileName);
            if (item == null)
            {
                throw new FileNotFoundException(FileNotFoundMessage, file.RelativePath);
            }
            return item.LastModified;            
        }

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        public override IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            var list = GetObjectList(folderMapping, folderPath);
            
            var pattern = "^" + Regex.Escape(folderPath);

            return (from o in list
                let f = o.Key
                let r =
                    (!string.IsNullOrEmpty(folderPath)
                        ? Regex.Replace(f,  pattern, "", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2))
                        : f)
                where f.StartsWith(folderPath, StringComparison.InvariantCultureIgnoreCase)
                                   && f.Length > folderPath.Length
                                   && r.IndexOf("/", StringComparison.Ordinal) > -1
                           select folderPath + r.Substring(0, r.IndexOf("/", StringComparison.Ordinal)) + "/").Distinct().ToList();

            //var mylist =  (from o in list
            //        let f = o.Key
            //        let r = (!string.IsNullOrEmpty(folderPath) ? RegexUtils.GetCachedRegex(Regex.Escape(folderPath)).Replace(f, string.Empty, 1) : f)
            //        where f.StartsWith(folderPath)
            //            && f.Length > folderPath.Length
            //            && r.IndexOf("/", StringComparison.Ordinal) > -1
            //        select folderPath + r.Substring(0, r.IndexOf("/", StringComparison.Ordinal)) + "/").Distinct().ToList();

            //return mylist;

        }

        /// <summary>
        /// Indicates if the specified file is synchronized.
        /// </summary>
        /// <remarks>
        /// For now, it returns false always until we find a better way to check if the file is synchronized.
        /// </remarks>
        public override bool IsInSync(IFileInfo file)
        {
            return Convert.ToInt32((file.LastModificationTime - GetLastModificationTime(file)).TotalSeconds) == 0;
        }

        /// <summary>
        /// Moves the folder and files at the specified folder path to the new folder path.
        /// </summary>
        public override void MoveFolder(string folderPath, string newFolderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNullOrEmpty("folderPath", folderPath);
            Requires.NotNullOrEmpty("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            MoveFolderInternal(folderMapping, folderPath, newFolderPath);
        }

        /// <summary>
        /// Renames the specified file using the new filename.
        /// </summary>
        public override void RenameFile(IFileInfo file, string newFileName)
        {
            Requires.NotNull("file", file);
            Requires.NotNullOrEmpty("newFileName", newFileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            MoveFileInternal(folderMapping, folder.MappedPath + file.FileName, folder.MappedPath + newFileName);
        }

        /// <summary>
        /// Renames the specified folder using the new foldername.
        /// </summary>
        public override void RenameFolder(IFolderInfo folder, string newFolderName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var mappedPath = folder.MappedPath;
            if (mappedPath == folder.FolderName + "/" || mappedPath.EndsWith("/" + folder.FolderName + "/", StringComparison.Ordinal))
            {
                var newMappedPath =
                    PathUtils.Instance.FormatFolderPath(
                        mappedPath.Substring(0, mappedPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal)) +
                        newFolderName);

                MoveFolderInternal(folderMapping, mappedPath, newMappedPath);
            }
        }

        /// <remarks>
        /// No implementation needed because this provider doesn't support FileAttributes.
        /// </remarks>
        public override void SetFileAttributes(IFileInfo file, FileAttributes fileAttributes)
        {
        }

        /// <remarks>
        /// Amazon doesn't support file attributes.
        /// </remarks>
        public override bool SupportsFileAttributes()
        {
            return false;
        }

        /// <summary>
        /// Updates the content of the specified file. It creates it if it doesn't exist.
        /// </summary>
        public override void UpdateFile(IFileInfo file, Stream content)
        {
            Requires.NotNull("file", file);
            Requires.NotNull("content", content);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            UpdateFileInternal(content, folderMapping, folder.MappedPath + file.FileName);
        }

        /// <summary>
        /// Updates the content of the specified file. It creates it if it doesn't exist.
        /// </summary>
        public override void UpdateFile(IFolderInfo folder, string fileName, Stream content)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            UpdateFileInternal(content, folderMapping, folder.MappedPath + fileName);
        }

        public override string GetHashCode(IFileInfo file)
        {
            Requires.NotNull("file", file);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            var item = GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
            if (item == null)
            {
                throw new FileNotFoundException(FileNotFoundMessage, file.RelativePath);
            }
            return (item.Size == file.Size) ? item.HashCode : String.Empty;
        }

        public override string GetHashCode(IFileInfo file, Stream fileContent)
        {
            if (FileExists(FolderManager.Instance.GetFolder(file.FolderId), file.FileName))
            {
                return GetHashCode(file);                
            }
            return String.Empty;
        }
    }
}
