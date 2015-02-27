#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.Assets
{
    public class AssetManager : ComponentBase<IAssetManager, AssetManager>, IAssetManager
    {
        // TODO: Use correct resx
        private const string ResourceFile = "DesktopModules/DigitalAssets/App_LocalResources/SharedResources";

        public IFileInfo RenameFile(int fileId, string newFileName)
        {
            Requires.NotNullOrEmpty("newFileName", newFileName);

            var filteredName = CleanDotsAtTheEndOfTheName(newFileName);

            if (string.IsNullOrEmpty(filteredName))
            {
                throw new AssetManagerException(string.Format(GetLocalizedString("FolderFileNameHasInvalidcharacters.Error"), newFileName));
            }

            // Chech if the new name has invalid chars
            if (IsInvalidName(filteredName))
            {
                throw new AssetManagerException(GetInvalidCharsErrorText());
            }

            // Check if the new name is a reserved name
            if (IsReservedName(filteredName))
            {
                throw new AssetManagerException(GetLocalizedString("FolderFileNameIsReserved.Error"));
            }

            var file = FileManager.Instance.GetFile(fileId, true);

            // Check if the name has not changed
            if (file.FileName == newFileName)
            {
                return file;
            }

            // Check if user has appropiate permissions
            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!HasPermission(folder, "MANAGE"))
            {
                throw new AssetManagerException(GetLocalizedString("UserHasNoPermissionToEditFile.Error"));
            }

            return FileManager.Instance.RenameFile(file, newFileName);
        }

        public IFolderInfo RenameFolder(int folderId, string newFolderName)
        {
            Requires.NotNullOrEmpty("newFolderName", newFolderName);

            newFolderName = CleanDotsAtTheEndOfTheName(newFolderName);

            // Check if the new name has invalid chars
            if (IsInvalidName(newFolderName))
            {
                throw new AssetManagerException(GetInvalidCharsErrorText());
            }

            // Check if the name is reserved
            if (IsReservedName(newFolderName))
            {
                throw new AssetManagerException(GetLocalizedString("FolderFileNameIsReserved.Error"));
            }

            var folder = GetFolderInfo(folderId);

            // Check if user has appropiate permissions
            if (!HasPermission(folder, "MANAGE"))
            {
                throw new AssetManagerException(GetLocalizedString("UserHasNoPermissionToEditFolder.Error"));
            }

            // check if the name has not changed
            if (folder.FolderName == newFolderName)
            {
                return folder;
            }
            if (folder.FolderName.ToLowerInvariant() == newFolderName.ToLowerInvariant())
            {
                folder.FolderPath = ReplaceFolderName(folder.FolderPath, folder.FolderName, newFolderName);
                return FolderManager.Instance.UpdateFolder(folder);
            }

            var newFolderPath = GetNewFolderPath(newFolderName, folder);
            // Check if the new folder already exists
            if (FolderManager.Instance.FolderExists(folder.PortalID, newFolderPath))
            {
                throw new AssetManagerException(GetLocalizedString("FolderAlreadyExists.Error"));
            }

            FolderManager.Instance.RenameFolder(folder, newFolderName);
            return folder;
        }

        public bool TagsChanged(IFileInfo file, IEnumerable<string> tags)
        {
            throw new System.NotImplementedException();
        }

        public void SaveTags(IFileInfo file, IEnumerable<string> tags)
        {
            throw new System.NotImplementedException();
        }

        private static string CleanDotsAtTheEndOfTheName(string name)
        {
            return name.Trim().TrimEnd('.', ' ');
        }

        private bool IsInvalidName(string itemName)
        {
            var invalidFilenameChars = new Regex("[" + Regex.Escape(GetInvalidChars()) + "]");

            return invalidFilenameChars.IsMatch(itemName);
        }

        public string GetInvalidChars()
        {
            var invalidChars = new string(Path.GetInvalidFileNameChars());

            foreach (var ch in Path.GetInvalidPathChars())
            {
                if (invalidChars.IndexOf(ch) == -1) // The ch does not exists
                {
                    invalidChars += ch;
                }
            }

            return invalidChars;
        }

        private bool IsReservedName(string name)
        {
            var reservedNames = new[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9", "CLOCK$" };
            return reservedNames.Contains(Path.GetFileNameWithoutExtension(name.ToUpperInvariant()));
        }

        public string GetInvalidCharsErrorText()
        {
            return string.Format(GetLocalizedString("FolderFileNameHasInvalidcharacters.Error"), "\\:/*?\"<>|");
        }

        public bool HasPermission(IFolderInfo folder, string permissionKey)
        {
            var hasPermision = PortalSettings.Current.UserInfo.IsSuperUser;

            if (!hasPermision && folder != null)
            {
                hasPermision = FolderPermissionController.HasFolderPermission(folder.FolderPermissions, permissionKey);
            }

            return hasPermision;
        }

        private IFolderInfo GetFolderInfo(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
            {
                throw new AssetManagerException(GetLocalizedString("FolderDoesNotExists.Error"));
            }
            return folder;
        }

        private string ReplaceFolderName(string path, string folderName, string newFolderName)
        {
            string newPath = PathUtils.Instance.RemoveTrailingSlash(path);
            if (string.IsNullOrEmpty(newPath))
            {
                return path;
            }
            var nameIndex = newPath.LastIndexOf(folderName, StringComparison.Ordinal);
            if (nameIndex == -1)
            {
                return path;
            }

            var result = newPath.Substring(0, nameIndex) + newPath.Substring(nameIndex).Replace(folderName, newFolderName);
            return result;
        }

        private string GetNewFolderPath(string newFolderName, IFolderInfo folder)
        {
            if (folder.FolderName.ToLowerInvariant() == newFolderName.ToLowerInvariant())
            {
                return folder.FolderPath;
            }

            var oldFolderPath = folder.FolderPath;
            if (oldFolderPath.Length > 0)
            {
                oldFolderPath = oldFolderPath.Substring(0, oldFolderPath.LastIndexOf(folder.FolderName, StringComparison.Ordinal));
            }

            return PathUtils.Instance.FormatFolderPath(oldFolderPath + newFolderName);
        }

        private static string GetLocalizedString(string key)
        {
            return Localization.Localization.GetString(key, ResourceFile);
        }
    }
}
