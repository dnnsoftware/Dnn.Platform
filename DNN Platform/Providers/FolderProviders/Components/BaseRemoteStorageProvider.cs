// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.Components
{
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

    public abstract class BaseRemoteStorageProvider : FolderProvider
    {
        private readonly string _encryptionKey = Host.GUID;
        private readonly PortalSecurity _portalSecurity = PortalSecurity.Instance;

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

        protected virtual string FileNotFoundMessage
        {
            get { return string.Empty; }
        }

        protected virtual string ObjectCacheKey
        {
            get { return string.Empty; }
        }

        protected virtual int ObjectCacheTimeout
        {
            get { return 150; }
        }

        protected virtual string ListObjectsCacheKey
        {
            get { return string.Empty; }
        }

        protected virtual int ListObjectsCacheTimeout
        {
            get { return 300; }
        }

        /// <summary>
        /// Adds a new folder.
        /// </summary>
        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping)
        {
            this.AddFolder(folderPath, folderMapping, folderPath);
        }

        /// <summary>
        /// Adds a new file to the specified folder.
        /// </summary>
        public override void AddFile(IFolderInfo folder, string fileName, Stream content)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.NotNull("content", content);

            this.UpdateFile(folder, fileName, content);
        }

        public virtual void ClearCache(int folderMappingId)
        {
            var cacheKey = string.Format(this.ListObjectsCacheKey, folderMappingId);
            DataCache.RemoveCache(cacheKey);

            // Clear cached objects
            DataCache.ClearCache(string.Format(this.ObjectCacheKey, folderMappingId, string.Empty));
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

            if (folderPath == newFolderPath)
            {
                return;
            }

            this.CopyFileInternal(folderMapping, folderPath + fileName, newFolderPath + fileName);
        }

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        public override void DeleteFile(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            this.DeleteFileInternal(folderMapping, folder.MappedPath + file.FileName);
        }

        /// <remarks>
        /// Azure Storage doesn't support folders, but we need to delete the file added while creating the folder.
        /// </remarks>
        public override void DeleteFolder(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            this.DeleteFolderInternal(folderMapping, folder);
        }

        /// <summary>
        /// Checks the existence of the specified file.
        /// </summary>
        /// <returns></returns>
        public override bool FileExists(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var item = this.GetStorageItem(folderMapping, folder.MappedPath + fileName);
            return item != null;
        }

        /// <summary>
        /// Checks the existence of the specified folder.
        /// </summary>
        /// <returns></returns>
        public override bool FolderExists(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            // the root folder should always exist.
            if (folderPath == string.Empty)
            {
                return true;
            }

            var list = this.GetObjectList(folderMapping, folderPath);

            return list.Any(o => o.Key.StartsWith(folderPath, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <remarks>
        /// Amazon doesn't support file attributes.
        /// </remarks>
        /// <returns></returns>
        public override FileAttributes? GetFileAttributes(IFileInfo file)
        {
            return null;
        }

        /// <summary>
        /// Gets the list of file names contained in the specified folder.
        /// </summary>
        /// <returns></returns>
        public override string[] GetFiles(IFolderInfo folder)
        {
            Requires.NotNull("folder", folder);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            var list = this.GetObjectList(folderMapping, folder.MappedPath);
            var mappedPath = folder.MappedPath;

            // return (from i in list
            //        let f = i.Key
            //        let r = (!string.IsNullOrEmpty(mappedPath) ? f.Replace(mappedPath, "") : f)
            //        where f.StartsWith(mappedPath, true, CultureInfo.InvariantCulture) && f.Length > mappedPath.Length && r.IndexOf("/", StringComparison.Ordinal) == -1
            //        select Path.GetFileName(f)).ToArray();
            var pattern = "^" + mappedPath;
            return (from i in list
                    let f = i.Key
                    let r = !string.IsNullOrEmpty(mappedPath) ? Regex.Replace(f, pattern, string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2)) : f
                    where f.StartsWith(mappedPath, true, CultureInfo.InvariantCulture) && f.Length > mappedPath.Length && r.IndexOf("/", StringComparison.Ordinal) == -1
                    select Path.GetFileName(f)).ToArray();
        }

        /// <summary>
        /// Gets the file length.
        /// </summary>
        /// <returns></returns>
        public override long GetFileSize(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            var item = this.GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
            if (item == null)
            {
                throw new FileNotFoundException(this.FileNotFoundMessage, file.RelativePath);
            }

            return item.Size;
        }

        /// <summary>
        ///   Gets a file Stream of the specified file.
        /// </summary>
        /// <returns></returns>
        public override Stream GetFileStream(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);

            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            return this.GetFileStreamInternal(folderMapping, folder.MappedPath + file.FileName);
        }

        /// <summary>
        /// Gets a file Stream of the specified file.
        /// </summary>
        /// <returns></returns>
        public override Stream GetFileStream(IFolderInfo folder, string fileName)
        {
            Requires.NotNull("folder", folder);
            Requires.NotNullOrEmpty("fileName", fileName);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            return this.GetFileStreamInternal(folderMapping, folder.MappedPath + fileName);
        }

        /// <summary>
        /// Gets the time when the specified file was last modified.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetLastModificationTime(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);

            var item = this.GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
            if (item == null)
            {
                throw new FileNotFoundException(this.FileNotFoundMessage, file.RelativePath);
            }

            return item.LastModified;
        }

        /// <summary>
        /// Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            var list = this.GetObjectList(folderMapping, folderPath);

            var pattern = "^" + Regex.Escape(folderPath);

            return (from o in list
                    let f = o.Key
                    let r =
                        !string.IsNullOrEmpty(folderPath)
                            ? Regex.Replace(f, pattern, string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2))
                            : f
                    where f.StartsWith(folderPath, StringComparison.InvariantCultureIgnoreCase)
                                       && f.Length > folderPath.Length
                                       && r.IndexOf("/", StringComparison.Ordinal) > -1
                    select folderPath + r.Substring(0, r.IndexOf("/", StringComparison.Ordinal)) + "/").Distinct().ToList();

            // var mylist =  (from o in list
            //        let f = o.Key
            //        let r = (!string.IsNullOrEmpty(folderPath) ? RegexUtils.GetCachedRegex(Regex.Escape(folderPath)).Replace(f, string.Empty, 1) : f)
            //        where f.StartsWith(folderPath)
            //            && f.Length > folderPath.Length
            //            && r.IndexOf("/", StringComparison.Ordinal) > -1
            //        select folderPath + r.Substring(0, r.IndexOf("/", StringComparison.Ordinal)) + "/").Distinct().ToList();

            // return mylist;
        }

        /// <summary>
        /// Indicates if the specified file is synchronized.
        /// </summary>
        /// <remarks>
        /// For now, it returns false always until we find a better way to check if the file is synchronized.
        /// </remarks>
        /// <returns></returns>
        public override bool IsInSync(IFileInfo file)
        {
            return Convert.ToInt32((file.LastModificationTime - this.GetLastModificationTime(file)).TotalSeconds) == 0;
        }

        /// <summary>
        /// Moves the folder and files at the specified folder path to the new folder path.
        /// </summary>
        public override void MoveFolder(string folderPath, string newFolderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNullOrEmpty("folderPath", folderPath);
            Requires.NotNullOrEmpty("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            this.MoveFolderInternal(folderMapping, folderPath, newFolderPath);
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

            this.MoveFileInternal(folderMapping, folder.MappedPath + file.FileName, folder.MappedPath + newFileName);
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

                this.MoveFolderInternal(folderMapping, mappedPath, newMappedPath);
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
        /// <returns></returns>
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

            this.UpdateFileInternal(content, folderMapping, folder.MappedPath + file.FileName);
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

            this.UpdateFileInternal(content, folderMapping, folder.MappedPath + fileName);
        }

        public override string GetHashCode(IFileInfo file)
        {
            Requires.NotNull("file", file);
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            var folderMapping = FolderMappingController.Instance.GetFolderMapping(folder.PortalID, folder.FolderMappingID);

            var item = this.GetStorageItem(folderMapping, folder.MappedPath + file.FileName);
            if (item == null)
            {
                throw new FileNotFoundException(this.FileNotFoundMessage, file.RelativePath);
            }

            return (item.Size == file.Size) ? item.HashCode : string.Empty;
        }

        public override string GetHashCode(IFileInfo file, Stream fileContent)
        {
            if (this.FileExists(FolderManager.Instance.GetFolder(file.FolderId), file.FileName))
            {
                return this.GetHashCode(file);
            }

            return string.Empty;
        }

        protected static bool GetBooleanSetting(FolderMappingInfo folderMapping, string settingName)
        {
            return bool.Parse(folderMapping.FolderMappingSettings[settingName].ToString());
        }

        protected static int GetIntegerSetting(FolderMappingInfo folderMapping, string settingName, int defaultValue)
        {
            int value;
            if (int.TryParse(GetSetting(folderMapping, settingName), out value))
            {
                return value;
            }

            return defaultValue;
        }

        protected static string GetSetting(FolderMappingInfo folderMapping, string settingName)
        {
            Requires.NotNull(nameof(folderMapping), folderMapping);
            Requires.NotNullOrEmpty(nameof(settingName), settingName);

            return folderMapping.FolderMappingSettings[settingName]?.ToString();
        }

        protected abstract void CopyFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void DeleteFileInternal(FolderMappingInfo folderMapping, string uri);

        protected abstract void DeleteFolderInternal(FolderMappingInfo folderMapping, IFolderInfo folder);

        protected abstract Stream GetFileStreamInternal(FolderMappingInfo folderMapping, string uri);

        protected abstract IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping);

        protected virtual IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping, string path)
        {
            return this.GetObjectList(folderMapping);
        }

        protected abstract void MoveFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void MoveFolderInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri);

        protected abstract void UpdateFileInternal(Stream stream, FolderMappingInfo folderMapping, string uri);

        protected virtual IRemoteStorageItem GetStorageItem(FolderMappingInfo folderMapping, string key)
        {
            return this.GetStorageItemInternal(folderMapping, key);
        }

        private IRemoteStorageItem GetStorageItemInternal(FolderMappingInfo folderMapping, string key)
        {
            var cacheKey = string.Format(this.ObjectCacheKey, folderMapping.FolderMappingID, key);

            return CBO.GetCachedObject<IRemoteStorageItem>(
                new CacheItemArgs(
                cacheKey,
                this.ObjectCacheTimeout,
                CacheItemPriority.Default,
                folderMapping.FolderMappingID),
                c =>
                {
                    var list = this.GetObjectList(folderMapping, key);

                    // return list.FirstOrDefault(i => i.Key == key);
                    return list.FirstOrDefault(i => i.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                });
        }
    }
}
