﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Security;

    /// <summary>
    ///   Base class that provides common functionallity to work with files and folders.
    /// </summary>
    public abstract class FolderProvider
    {
        private const string SettingsControlId = "Settings.ascx";
        private string _providerName;

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if the provider ensures the files/folders it manages are secure from outside access.
        /// </summary>
        /// <remarks>
        /// Some providers (e.g. Standard) store their files/folders in a way that allows for anonymous access that bypasses DotNetNuke.
        /// These providers cannot guarantee that files are only accessed by authorized users and must return false.
        /// </remarks>
        public virtual bool IsStorageSecure
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating if the provider requires network connectivity to do its tasks.
        /// </summary>
        public virtual bool RequiresNetworkConnectivity
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether indicates if the folder provider supports mapped paths when creating new folders.
        /// </summary>
        /// <remarks>
        /// If this method is not overrided it returns false.
        /// </remarks>
        public virtual bool SupportsMappedPaths
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        ///   Get the list of all the folder providers.
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, FolderProvider> GetProviderList()
        {
            var providerList = ComponentFactory.GetComponents<FolderProvider>();

            foreach (var key in providerList.Keys)
            {
                providerList[key]._providerName = key;
            }

            return providerList;
        }

        /// <summary>
        ///   Gets an instance of a specific FolderProvider of a given name.
        /// </summary>
        /// <returns></returns>
        public static FolderProvider Instance(string friendlyName)
        {
            var provider = ComponentFactory.GetComponent<FolderProvider>(friendlyName);

            provider._providerName = friendlyName;

            return provider;
        }

        /// <summary>
        /// Gets a value indicating whether the provider supports the MoveFile method.  If a provider supports the MoveFile method, the
        /// folder manager does nt have to implement move by copying the file and then deleting the original.
        /// </summary>
        public virtual bool SupportsMoveFile
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the provider supports the MoveFolder method.  If a provider supports the MoveFolder method, the
        /// folder manager does not have to implement move by copying the folder and then deleting the original.
        /// </summary>
        public virtual bool SupportsMoveFolder
        {
            get { return false; }
        }

        public virtual void AddFolder(string folderPath, FolderMappingInfo folderMapping, string mappedPath)
        {
            this.AddFolder(folderPath, folderMapping);
        }

        /// <summary>
        /// Copies the specified file to the destination folder.
        /// </summary>
        public virtual void CopyFile(string folderPath, string fileName, string newFolderPath, FolderMappingInfo folderMapping)
        {
            Requires.PropertyNotNull("folderPath", folderPath);
            Requires.NotNullOrEmpty("fileName", fileName);
            Requires.PropertyNotNull("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            if (folderPath == newFolderPath)
            {
                return;
            }

            var sourceFolder = FolderManager.Instance.GetFolder(folderMapping.PortalID, folderPath);
            var destinationFolder = FolderManager.Instance.GetFolder(folderMapping.PortalID, newFolderPath);

            Requires.NotNull("sourceFolder", sourceFolder);
            Requires.NotNull("destinationFolder", destinationFolder);

            using (var fileContent = this.GetFileStream(sourceFolder, fileName))
            {
                if (!fileContent.CanSeek)
                {
                    using (var seekableStream = FileManager.Instance.GetSeekableStream(fileContent))
                    {
                        this.AddFile(destinationFolder, fileName, seekableStream);
                    }
                }
                else
                {
                    this.AddFile(destinationFolder, fileName, fileContent);
                }
            }
        }

        private static void AddFolderAndMoveFiles(string folderPath, string newFolderPath, FolderMappingInfo folderMapping)
        {
            var folderProvider = Instance(folderMapping.FolderProviderType);

            if (!folderProvider.FolderExists(newFolderPath, folderMapping))
            {
                folderProvider.AddFolder(newFolderPath, folderMapping);
            }

            var folder = new FolderInfo { FolderPath = folderPath, FolderMappingID = folderMapping.FolderMappingID, PortalID = folderMapping.PortalID };
            var newFolder = new FolderInfo { FolderPath = newFolderPath, FolderMappingID = folderMapping.FolderMappingID, PortalID = folderMapping.PortalID };

            MoveFiles(folder, newFolder, folderMapping);
        }

        private static void MoveFiles(IFolderInfo folder, IFolderInfo newFolder, FolderMappingInfo folderMapping)
        {
            var folderProvider = Instance(folderMapping.FolderProviderType);
            var files = folderProvider.GetFiles(folder);

            foreach (var file in files)
            {
                using (var fileContent = folderProvider.GetFileStream(folder, file))
                {
                    if (!fileContent.CanSeek)
                    {
                        using (var seekableStream = FileManager.Instance.GetSeekableStream(fileContent))
                        {
                            folderProvider.AddFile(newFolder, file, seekableStream);
                        }
                    }
                    else
                    {
                        folderProvider.AddFile(newFolder, file, fileContent);
                    }
                }

                folderProvider.DeleteFile(new FileInfo { FileName = file, Folder = folder.FolderPath, FolderMappingID = folderMapping.FolderMappingID, PortalId = folderMapping.PortalID });
            }
        }

        /// <summary>
        ///   Gets a file Stream of the specified file.
        /// </summary>
        /// <returns></returns>
        public virtual Stream GetFileStream(IFolderInfo folder, IFileInfo file, int version)
        {
            return this.GetFileStream(folder, FileVersionController.GetVersionedFilename(file, version));
        }

        /// <summary>
        ///   Gets the virtual path of the control file used to display and update specific folder mapping settings. By default, the control name is Settings.ascx.
        /// </summary>
        /// <returns>
        ///   If the folder provider has special settings, this method returns the virtual path of the control that allows to display and set those settings.
        /// </returns>
        /// <remarks>
        ///   The returned control must inherit from FolderMappingSettingsControlBase.
        /// </remarks>
        public virtual string GetSettingsControlVirtualPath()
        {
            var provider = Config.GetProvider("folder", this._providerName);

            if (provider != null)
            {
                var virtualPath = provider.Attributes["providerPath"] + SettingsControlId;

                if (File.Exists(System.Web.HttpContext.Current.Server.MapPath(virtualPath)))
                {
                    return virtualPath;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Moves a file to a new folder.
        /// </summary>
        /// <param name="file"></param>
        public virtual void MoveFile(IFileInfo file, IFolderInfo destinationFolder)
        {
            throw new NotImplementedException("This provider does not implement MoveFile");
        }

        /// <summary>
        /// Moves the folder and files at the specified folder path to the new folder path.
        /// </summary>
        public virtual void MoveFolder(string folderPath, string newFolderPath, FolderMappingInfo folderMapping)
        {
            Requires.NotNullOrEmpty("folderPath", folderPath);
            Requires.NotNullOrEmpty("newFolderPath", newFolderPath);
            Requires.NotNull("folderMapping", folderMapping);

            var folderProvider = Instance(folderMapping.FolderProviderType);
            var folder = FolderManager.Instance.GetFolder(folderMapping.PortalID, folderPath);
            var folderManager = new FolderManager();
            var subFolders = folderManager.GetFolderMappingFoldersRecursive(folderMapping, folder).Skip(1).Reverse();

            AddFolderAndMoveFiles(folderPath, newFolderPath, folderMapping);

            foreach (var subFolderPath in subFolders.Select(s => s.Key))
            {
                var newSubFolderPath = newFolderPath + subFolderPath.Substring(folderPath.Length);
                AddFolderAndMoveFiles(subFolderPath, newSubFolderPath, folderMapping);

                folderProvider.DeleteFolder(new FolderInfo { FolderPath = subFolderPath, FolderMappingID = folderMapping.FolderMappingID, PortalID = folderMapping.PortalID });
            }

            folderProvider.DeleteFolder(new FolderInfo { FolderPath = folderPath, FolderMappingID = folderMapping.FolderMappingID, PortalID = folderMapping.PortalID });
        }

        /// <summary>
        /// Gets the decrypted value of an encrypted folder mapping setting.
        /// </summary>
        /// <remarks>If the value is not set the method returns null.</remarks>
        /// <param name="folderMappingSettings">Folder mapping settings.</param>
        /// <param name="settingName">Setting name.</param>
        /// <exception cref="ArgumentNullException">the input parameters of the method cannot be null.</exception>
        /// <returns>decrypted value.</returns>
        public string GetEncryptedSetting(Hashtable folderMappingSettings, string settingName)
        {
            Requires.NotNull(nameof(folderMappingSettings), folderMappingSettings);
            Requires.NotNullOrEmpty(nameof(settingName), settingName);

            return PortalSecurity.Instance.Decrypt(Host.GUID, folderMappingSettings[settingName]?.ToString());
        }

        public string EncryptValue(string settingValue)
        {
            return PortalSecurity.Instance.Encrypt(Host.GUID, settingValue.Trim());
        }

        public virtual string GetHashCode(IFileInfo file)
        {
            var currentHashCode = string.Empty;
            using (var fileContent = this.GetFileStream(file))
            {
                currentHashCode = this.GetHashCode(file, fileContent);
            }

            return currentHashCode;
        }

        public virtual string GetHashCode(IFileInfo file, Stream fileContent)
        {
            Requires.NotNull("stream", fileContent);

            if (!fileContent.CanSeek)
            {
                var tmp = FileManager.Instance.GetSeekableStream(fileContent);
                fileContent.Close();
                fileContent = tmp;
            }

            var hashText = new StringBuilder();
            using (var hasher = SHA1.Create())
            {
                var hashData = hasher.ComputeHash(fileContent);
                foreach (var b in hashData)
                {
                    hashText.Append(b.ToString("x2"));
                }
            }

            return hashText.ToString();
        }

        /// <summary>
        ///   Adds a new file to the specified folder.
        /// </summary>
        /// <remarks>
        ///   Do not close content Stream.
        /// </remarks>
        public abstract void AddFile(IFolderInfo folder, string fileName, Stream content);

        /// <summary>
        ///   Adds a new folder to a specified parent folder.
        /// </summary>
        public abstract void AddFolder(string folderPath, FolderMappingInfo folderMapping);

        /// <summary>
        ///   Deletes the specified file.
        /// </summary>
        public abstract void DeleteFile(IFileInfo file);

        /// <summary>
        ///   Deletes the specified folder.
        /// </summary>
        public abstract void DeleteFolder(IFolderInfo folder);

        /// <summary>
        ///   Checks the existence of the specified file in the underlying system.
        /// </summary>
        /// <returns></returns>
        public abstract bool FileExists(IFolderInfo folder, string fileName);

        /// <summary>
        ///   Checks the existence of the specified folder in the underlying system.
        /// </summary>
        /// <returns></returns>
        public abstract bool FolderExists(string folderPath, FolderMappingInfo folderMapping);

        /// <summary>
        ///   Gets the file attributes of the specified file.
        /// </summary>
        /// <remarks>
        ///   Because some Providers don't support file attributes, this methods returns a nullable type to allow them to return null.
        /// </remarks>
        /// <returns></returns>
        public abstract FileAttributes? GetFileAttributes(IFileInfo file);

        /// <summary>
        ///   Gets the list of file names contained in the specified folder.
        /// </summary>
        /// <returns></returns>
        public abstract string[] GetFiles(IFolderInfo folder);

        /// <summary>
        /// Gets the file length.
        /// </summary>
        /// <returns></returns>
        public abstract long GetFileSize(IFileInfo file);

        /// <summary>
        ///   Gets a file Stream of the specified file.
        /// </summary>
        /// <returns></returns>
        public abstract Stream GetFileStream(IFileInfo file);

        /// <summary>
        ///   Gets a file Stream of the specified file.
        /// </summary>
        /// <returns></returns>
        public abstract Stream GetFileStream(IFolderInfo folder, string fileName);

        /// <summary>
        /// Gets the direct Url to the file.
        /// </summary>
        /// <returns></returns>
        public abstract string GetFileUrl(IFileInfo file);

        /// <summary>
        ///   Gets the URL of the image to display in FileManager tree.
        /// </summary>
        /// <returns></returns>
        public abstract string GetFolderProviderIconPath();

        /// <summary>
        ///   Gets the time when the specified file was last modified.
        /// </summary>
        /// <returns></returns>
        public abstract DateTime GetLastModificationTime(IFileInfo file);

        /// <summary>
        ///   Gets the list of subfolders for the specified folder.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<string> GetSubFolders(string folderPath, FolderMappingInfo folderMapping);

        /// <summary>
        ///   Indicates if the specified file is synchronized.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsInSync(IFileInfo file);

        /// <summary>
        ///   Renames the specified file using the new filename.
        /// </summary>
        public abstract void RenameFile(IFileInfo file, string newFileName);

        /// <summary>
        ///   Renames the specified folder using the new foldername.
        /// </summary>
        public abstract void RenameFolder(IFolderInfo folder, string newFolderName);

        /// <summary>
        ///   Sets the specified attributes to the specified file.
        /// </summary>
        public abstract void SetFileAttributes(IFileInfo file, FileAttributes fileAttributes);

        /// <summary>
        ///   Gets a value indicating if the underlying system supports file attributes.
        /// </summary>
        /// <returns></returns>
        public abstract bool SupportsFileAttributes();

        /// <summary>
        ///   Updates the content of the specified file. It creates it if it doesn't exist.
        /// </summary>
        /// <remarks>
        ///   Do not close content Stream.
        /// </remarks>
        public abstract void UpdateFile(IFileInfo file, Stream content);

        /// <summary>
        ///   Updates the content of the specified file. It creates it if it doesn't exist.
        /// </summary>
        /// <remarks>
        ///   Do not close content Stream.
        /// </remarks>
        public abstract void UpdateFile(IFolderInfo folder, string fileName, Stream content);
    }
}
