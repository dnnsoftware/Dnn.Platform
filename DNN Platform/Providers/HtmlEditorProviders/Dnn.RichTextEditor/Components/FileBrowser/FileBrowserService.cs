// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.FileBrowser
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Services.FileSystem;

    /// <summary>
    /// The DNN side of the editor's file browser. Every operation is checked against the current user's folder
    /// permissions; callers pass paths exactly as the browser sent them.
    /// </summary>
    public class FileBrowserService
    {
        /// <summary>The folder provider whose files are served straight from the portal's home directory.</summary>
        public const string StandardFolderProviderType = "StandardFolderProvider";

        private const string ParentFolderName = "..";

        private static readonly HashSet<string> ImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "jpg", "jpeg", "png", "gif", "webp", "bmp", "svg", "avif", "tif", "tiff", "ico",
        };

        // Characters that are invalid in file names, plus those the browser would have to escape in its markup.
        private static readonly char[] InvalidNameChars = Path.GetInvalidFileNameChars().Concat(new[] { '<', '>', '"', }).Distinct().ToArray();

        // Characters that would also break the URL the browser builds from a name; kept out of names created here.
        private static readonly char[] InvalidNewNameChars = InvalidNameChars.Concat(new[] { '&', '#', '%', '+', }).ToArray();

        private readonly IFolderManager folderManager;
        private readonly IFileManager fileManager;
        private readonly IFolderMappingController folderMappingController;
        private readonly IFolderPermissionChecker permissions;
        private readonly Func<string, string> getContentType;

        /// <summary>Initializes a new instance of the <see cref="FileBrowserService"/> class.</summary>
        /// <param name="folderManager">The folder manager.</param>
        /// <param name="fileManager">The file manager.</param>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        /// <param name="permissions">The folder permission checker for the current user.</param>
        /// <param name="getContentType">Returns the MIME type for a file extension.</param>
        public FileBrowserService(
            IFolderManager folderManager,
            IFileManager fileManager,
            IFolderMappingController folderMappingController,
            IFolderPermissionChecker permissions,
            Func<string, string> getContentType)
        {
            this.folderManager = folderManager ?? throw new ArgumentNullException(nameof(folderManager));
            this.fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            this.folderMappingController = folderMappingController ?? throw new ArgumentNullException(nameof(folderMappingController));
            this.permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
            this.getContentType = getContentType ?? throw new ArgumentNullException(nameof(getContentType));
        }

        /// <summary>
        /// Converts a browser path to a DNN folder path: forward slashes, no leading slash, a trailing slash
        /// unless it is the root (empty), with <c>.</c> and <c>..</c> segments resolved. <c>..</c> never goes above the root.
        /// </summary>
        /// <param name="path">The path sent by the browser.</param>
        /// <returns>The DNN folder path.</returns>
        public static string NormalizePath(string path)
        {
            var segments = new List<string>();
            foreach (var segment in (path ?? string.Empty).Replace('\\', '/').Split('/'))
            {
                var trimmed = segment.Trim();
                if (trimmed.Length == 0 || trimmed == ".")
                {
                    continue;
                }

                if (trimmed == ParentFolderName)
                {
                    if (segments.Count > 0)
                    {
                        segments.RemoveAt(segments.Count - 1);
                    }

                    continue;
                }

                segments.Add(trimmed);
            }

            return segments.Count == 0 ? string.Empty : string.Join("/", segments) + "/";
        }

        /// <summary>Gets a value indicating whether an existing file or folder name can be shown and acted on by the browser.</summary>
        /// <param name="name">The name.</param>
        /// <returns><see langword="true"/> if the name is valid.</returns>
        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name)
                && name.IndexOfAny(InvalidNameChars) < 0;
        }

        /// <summary>Gets a value indicating whether a name can be used for a new file or folder.</summary>
        /// <param name="name">The name.</param>
        /// <returns><see langword="true"/> if the name is valid.</returns>
        public static bool IsValidNewName(string name)
        {
            return !string.IsNullOrWhiteSpace(name)
                && name == name.Trim()
                && !name.EndsWith(".", StringComparison.Ordinal)
                && name.IndexOfAny(InvalidNewNameChars) < 0;
        }

        /// <summary>Turns an uploaded file's name into a safe file name, keeping its extension.</summary>
        /// <param name="uploadedName">The name sent by the browser, which may include a client-side path.</param>
        /// <returns>The cleaned name.</returns>
        public static string CleanFileName(string uploadedName)
        {
            var name = (uploadedName ?? string.Empty).Replace('\\', '/');
            name = name.Substring(name.LastIndexOf('/') + 1);
            name = new string(name.Select(c => InvalidNewNameChars.Contains(c) ? '_' : c).ToArray()).Trim().TrimEnd('.');
            if (Path.GetFileNameWithoutExtension(name).Trim().Length == 0)
            {
                name = "file" + name;
            }

            return name;
        }

        /// <summary>Gets a value indicating whether a file is an image the editor can show inline.</summary>
        /// <param name="fileName">The file name.</param>
        /// <returns><see langword="true"/> for image files.</returns>
        public static bool IsImage(string fileName)
        {
            return ImageExtensions.Contains(Path.GetExtension(fileName ?? string.Empty).TrimStart('.'));
        }

        /// <summary>Gets a folder the current user can browse.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="path">The path sent by the browser.</param>
        /// <returns>The folder, or <see langword="null"/> if it does not exist or cannot be browsed.</returns>
        public IFolderInfo GetBrowsableFolder(int portalId, string path)
        {
            var folder = this.folderManager.GetFolder(portalId, NormalizePath(path));
            return folder != null && this.permissions.CanBrowse(folder) ? folder : null;
        }

        /// <summary>Gets what the current user may do in a folder, using the browser's permission names.</summary>
        /// <param name="folder">The folder.</param>
        /// <returns>The permissions, keyed by the browser's <c>allow…</c> rule names.</returns>
        public IDictionary<string, bool> GetPermissions(IFolderInfo folder)
        {
            var canAddFiles = this.permissions.CanAddFiles(folder);
            return new Dictionary<string, bool>
            {
                { "allowFiles", this.permissions.CanBrowse(folder) },
                { "allowFileUpload", canAddFiles },
                { "allowFileUploadRemote", false },
                { "allowFileRemove", this.permissions.CanDeleteFiles(folder) },
                { "allowFileRename", false },
                { "allowFileMove", false },
                { "allowFolders", true },
                { "allowFolderCreate", this.permissions.CanAddFolder(folder) },
                { "allowFolderRemove", false },
                { "allowFolderRename", false },
                { "allowFolderMove", false },
                { "allowImageResize", false },
                { "allowImageCrop", false },
            };
        }

        /// <summary>Gets the names of the subfolders the current user can browse, preceded by <c>..</c> below the root.</summary>
        /// <param name="folder">The parent folder.</param>
        /// <returns>The folder names.</returns>
        public IList<string> GetSubfolderNames(IFolderInfo folder)
        {
            var names = new List<string>();
            if (!string.IsNullOrEmpty(folder.FolderPath))
            {
                names.Add(ParentFolderName);
            }

            names.AddRange(
                this.folderManager.GetFolders(folder)
                    .Where(this.permissions.CanBrowse)
                    .Select(f => f.FolderName)
                    .Where(IsValidName)
                    .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase));
            return names;
        }

        /// <summary>Gets the files in a folder.</summary>
        /// <param name="folder">The folder.</param>
        /// <param name="onlyImages">Whether to return only image files.</param>
        /// <returns>The files.</returns>
        public IList<IFileInfo> GetFiles(IFolderInfo folder, bool onlyImages)
        {
            return this.folderManager.GetFiles(folder)

                // Names the browser cannot display safely are skipped rather than escaped, since it also uses them to identify the file.
                .Where(f => IsValidName(f.FileName))
                .Where(f => !onlyImages || IsImage(f.FileName))
                .ToList();
        }

        /// <summary>
        /// Gets a value indicating whether the folder's files are served from <c>{home directory}{folder path}{file name}</c>.
        /// Files in other folders (secure, database or cloud) must be linked through <see cref="IFileManager.GetUrl"/>.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns><see langword="true"/> for standard folders.</returns>
        public bool HasDirectUrls(IFolderInfo folder)
        {
            var mapping = this.folderMappingController.GetFolderMapping(folder.PortalID, folder.FolderMappingID);
            return string.Equals(mapping?.FolderProviderType, StandardFolderProviderType, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Saves an uploaded file in the folder, adding a numeric suffix if the name is taken.</summary>
        /// <param name="folder">The target folder.</param>
        /// <param name="uploadedName">The name sent by the browser.</param>
        /// <param name="content">The file content.</param>
        /// <param name="userId">The uploading user's ID.</param>
        /// <param name="maxBytes">The user's upload size limit, or <see langword="null"/> for none.</param>
        /// <returns>The saved file.</returns>
        public FileBrowserResult<IFileInfo> AddFile(IFolderInfo folder, string uploadedName, Stream content, int userId, long? maxBytes)
        {
            if (!this.permissions.CanAddFiles(folder))
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.PermissionDenied);
            }

            if (maxBytes.HasValue && content.Length > maxBytes.Value)
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.FileTooLarge);
            }

            var fileName = this.GetAvailableFileName(folder, CleanFileName(uploadedName));
            try
            {
                // Permissions were checked above (WRITE or ADD, as the core upload service does);
                // the file manager still enforces the allowed extensions, file content checks and disk quota.
                var file = this.fileManager.AddFile(folder, fileName, content, false, false, this.getContentType(Path.GetExtension(fileName)), userId);
                return FileBrowserResult<IFileInfo>.Success(file);
            }
            catch (InvalidFileExtensionException)
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.ExtensionNotAllowed);
            }
            catch (InvalidFilenameException)
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.InvalidName);
            }
            catch (PermissionsNotMetException)
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.PermissionDenied);
            }
            catch (Exception ex) when (ex is NoSpaceAvailableException || ex is InvalidFileContentException || ex is FolderProviderException)
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.SaveFailed);
            }
        }

        /// <summary>Creates a subfolder, which inherits the parent folder's permissions.</summary>
        /// <param name="parent">The parent folder.</param>
        /// <param name="name">The new folder's name.</param>
        /// <returns>The new folder.</returns>
        public FileBrowserResult<IFolderInfo> CreateFolder(IFolderInfo parent, string name)
        {
            if (!this.permissions.CanAddFolder(parent))
            {
                return FileBrowserResult<IFolderInfo>.Failure(FileBrowserError.PermissionDenied);
            }

            if (!IsValidNewName(name))
            {
                return FileBrowserResult<IFolderInfo>.Failure(FileBrowserError.InvalidName);
            }

            var folderPath = parent.FolderPath + name + "/";
            if (this.folderManager.FolderExists(parent.PortalID, folderPath))
            {
                return FileBrowserResult<IFolderInfo>.Failure(FileBrowserError.AlreadyExists);
            }

            try
            {
                var mapping = this.folderMappingController.GetFolderMapping(parent.PortalID, parent.FolderMappingID);
                return FileBrowserResult<IFolderInfo>.Success(this.folderManager.AddFolder(mapping, folderPath));
            }
            catch (FolderAlreadyExistsException)
            {
                return FileBrowserResult<IFolderInfo>.Failure(FileBrowserError.AlreadyExists);
            }
            catch (InvalidFolderPathException)
            {
                return FileBrowserResult<IFolderInfo>.Failure(FileBrowserError.InvalidName);
            }
            catch (FolderProviderException)
            {
                return FileBrowserResult<IFolderInfo>.Failure(FileBrowserError.SaveFailed);
            }
        }

        /// <summary>Deletes a file from the folder.</summary>
        /// <param name="folder">The folder.</param>
        /// <param name="fileName">The file's name.</param>
        /// <returns>The deleted file.</returns>
        public FileBrowserResult<IFileInfo> RemoveFile(IFolderInfo folder, string fileName)
        {
            if (!this.permissions.CanDeleteFiles(folder))
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.PermissionDenied);
            }

            var file = IsValidName(fileName) ? this.fileManager.GetFile(folder, fileName) : null;
            if (file == null)
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.FileNotFound);
            }

            try
            {
                this.fileManager.DeleteFile(file);
                return FileBrowserResult<IFileInfo>.Success(file);
            }
            catch (Exception ex) when (ex is FileLockedException || ex is FolderProviderException)
            {
                return FileBrowserResult<IFileInfo>.Failure(FileBrowserError.SaveFailed);
            }
        }

        /// <summary>Gets a file the current user is allowed to open.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="filePath">The file's folder path and name, e.g. <c>Images/logo.png</c>.</param>
        /// <returns>The file, or <see langword="null"/> if it does not exist or the user cannot view its folder.</returns>
        public IFileInfo GetViewableFile(int portalId, string filePath)
        {
            var normalized = (filePath ?? string.Empty).Replace('\\', '/');
            var separator = normalized.LastIndexOf('/');
            var fileName = normalized.Substring(separator + 1);
            if (!IsValidName(fileName))
            {
                return null;
            }

            var folder = this.folderManager.GetFolder(portalId, NormalizePath(separator < 0 ? string.Empty : normalized.Substring(0, separator)));
            if (folder == null || !this.permissions.CanView(folder))
            {
                return null;
            }

            return this.fileManager.GetFile(folder, fileName);
        }

        private string GetAvailableFileName(IFolderInfo folder, string fileName)
        {
            if (!this.fileManager.FileExists(folder, fileName, true))
            {
                return fileName;
            }

            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            for (var index = 1; ; index++)
            {
                var candidate = string.Format(CultureInfo.InvariantCulture, "{0}_{1}{2}", baseName, index, extension);
                if (!this.fileManager.FileExists(folder, candidate, true))
                {
                    return candidate;
                }
            }
        }
    }
}
