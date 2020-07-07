// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Do not implement.  This interface is meant for reference and unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IFileManager
    {
        /// <summary>
        /// Gets the system defined content types.
        /// </summary>
        [Obsolete("Deprecated in DNN 7.4.2.  It has been replaced by FileContentTypeManager.Instance.ContentTypes. Scheduled removal in v10.0.0.")]
        IDictionary<string, string> ContentTypes { get; }

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent);

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exits.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite);

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, string contentType);

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="createdByUserID">ID of the user that creates the file.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, string contentType, int createdByUserID);

        /// <summary>
        /// Adds a file to the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to add the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <param name="overwrite">Indicates if the file has to be over-written if it exists.</param>
        /// <param name="checkPermissions">Indicates if permissions have to be met.</param>
        /// <param name="ignoreWhiteList">Indicates whether the whitelist should be ignored.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="createdByUserID">ID of the user that creates the file.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as specified by the parameters.</returns>
        IFileInfo AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite, bool checkPermissions, bool ignoreWhiteList, string contentType, int createdByUserID);

        /// <summary>
        /// Copies the specified file into the specified folder.
        /// </summary>
        /// <param name="file">The file to copy.</param>
        /// <param name="destinationFolder">The folder where to copy the file to.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the copied file.</returns>
        IFileInfo CopyFile(IFileInfo file, IFolderInfo destinationFolder);

        /// <summary>
        /// Deletes the specified file.
        /// </summary>
        /// <param name="file">The file to delete.</param>
        void DeleteFile(IFileInfo file);

        /// <summary>
        /// Deletes the specified files.
        /// </summary>
        /// <param name="files">The files to delete.</param>
        void DeleteFiles(IEnumerable<IFileInfo> files);

        /// <summary>
        /// Checks the existence of the specified file in the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to check the existence of the file.</param>
        /// <param name="fileName">The file name to check the existence of.</param>
        /// <returns>A boolean value indicating whether the file exists or not in the specified folder.</returns>
        bool FileExists(IFolderInfo folder, string fileName);

        /// <summary>
        /// Checks the existence of the specified file in the specified folder.
        /// </summary>
        /// <param name="folder">The folder where to check the existence of the file.</param>
        /// <param name="fileName">The file name to check the existence of.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>A boolean value indicating whether the file exists or not in the specified folder.</returns>
        bool FileExists(IFolderInfo folder, string fileName, bool retrieveUnpublishedFiles);

        /// <summary>
        /// Gets the Content Type for the specified file extension.
        /// </summary>
        /// <param name="extension">The file extension.</param>
        /// <returns>The Content Type for the specified extension.</returns>
        [Obsolete("Deprecated in DNN 7.4.2.  It has been replaced by FileContentTypeManager.Instance.GetContentType(string extension). Scheduled removal in v10.0.0.")]
        string GetContentType(string extension);

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="fileID">The file identifier.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        IFileInfo GetFile(int fileID);

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="fileID">The file identifier.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        IFileInfo GetFile(int fileID, bool retrieveUnpublishedFiles);

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="folder">The folder where the file is stored.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        IFileInfo GetFile(IFolderInfo folder, string fileName);

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="folder">The folder where the file is stored.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        IFileInfo GetFile(IFolderInfo folder, string fileName, bool retrieveUnpublishedFiles);

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="portalId">The portal ID or Null.NullInteger for the Host.</param>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <remarks>Host and portal settings commonly return a relative path to a file.  This method uses that relative path to fetch file metadata.</remarks>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        IFileInfo GetFile(int portalId, string relativePath);

        /// <summary>
        /// Gets the file metadata for the specified file.
        /// </summary>
        /// <param name="portalId">The portal ID or Null.NullInteger for the Host.</param>
        /// <param name="relativePath">Relative path to the file.</param>
        /// <param name="retrieveUnpublishedFiles">Indicates if the file is retrieved from All files or from Published files.</param>
        /// <remarks>Host and portal settings commonly return a relative path to a file.  This method uses that relative path to fetch file metadata.</remarks>
        /// <returns>The <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> object with the metadata of the specified file.</returns>
        IFileInfo GetFile(int portalId, string relativePath, bool retrieveUnpublishedFiles);

        /// <summary>
        /// Gets the content of the specified file.
        /// </summary>
        /// <param name="file">The file to get the content from.</param>
        /// <returns>A stream with the content of the file.</returns>
        Stream GetFileContent(IFileInfo file);

        /// <summary>
        /// Gets a seekable Stream based on the specified non-seekable Stream.
        /// </summary>
        /// <param name="stream">A non-seekable Stream.</param>
        /// <returns>A seekable Stream.</returns>
        Stream GetSeekableStream(Stream stream);

        /// <summary>
        /// Gets the direct Url to the file.
        /// </summary>
        /// <param name="file">The file to get the Url.</param>
        /// <returns>The direct Url to the file.</returns>
        string GetUrl(IFileInfo file);

        /// <summary>
        /// Gets a flag that dertermines if the file is an Image.
        /// </summary>
        /// <param name="file">The file to test.</param>
        /// <returns>The flag as a boolean value.</returns>
        bool IsImageFile(IFileInfo file);

        /// <summary>
        /// Moves the specified file into the specified folder.
        /// </summary>
        /// <param name="file">The file to move.</param>
        /// <param name="destinationFolder">The folder where to move the file to.</param>
        /// <returns>An <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the moved file.</returns>
        IFileInfo MoveFile(IFileInfo file, IFolderInfo destinationFolder);

        /// <summary>
        /// Renames the specified file.
        /// </summary>
        /// <param name="file">The file to rename.</param>
        /// <param name="newFileName">The new filename to assign to the file.</param>
        /// <returns>An <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> with the information of the renamed file.</returns>
        IFileInfo RenameFile(IFileInfo file, string newFileName);

        /// <summary>
        /// Sets the specified FileAttributes of the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="fileAttributes">The file attributes to add.</param>
        void SetAttributes(IFileInfo file, FileAttributes fileAttributes);

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the folder where the file belongs.
        /// </summary>
        /// <param name="file">The file to unzip.</param>
        void UnzipFile(IFileInfo file);

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the specified folder.
        /// </summary>
        /// <param name="file">The file to unzip.</param>
        /// <param name="destinationFolder">The folder to unzip to.</param>
        void UnzipFile(IFileInfo file, IFolderInfo destinationFolder);

        /// <summary>
        /// Extracts the files and folders contained in the specified zip file to the specified folder.
        /// </summary>
        /// <param name="file">The file to unzip.</param>
        /// <param name="destinationFolder">The folder to unzip to.</param>
        /// <param name="invalidFiles">Files which can't exact.</param>
        /// <returns>Total files count in the zip file.</returns>
        int UnzipFile(IFileInfo file, IFolderInfo destinationFolder, IList<string> invalidFiles);

        /// <summary>
        /// Updates the metadata of the specified file.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as the updated file.</returns>
        IFileInfo UpdateFile(IFileInfo file);

        /// <summary>
        /// Regenerates the hash and updates the metadata of the specified file.
        /// </summary>
        /// <param name="file">The file to update.</param>
        /// <param name="fileContent">Stream used to regenerate the hash.</param>
        /// <returns>A <see cref="DotNetNuke.Services.FileSystem.IFileInfo">IFileInfo</see> as the updated file.</returns>
        IFileInfo UpdateFile(IFileInfo file, Stream fileContent);

        /// <summary>
        /// Writes the content of the specified file into the specified stream.
        /// </summary>
        /// <param name="file">The file to write into the stream.</param>
        /// <param name="stream">The stream to write to.</param>
        void WriteFile(IFileInfo file, Stream stream);

        /// <summary>
        /// Downloads the specified file.
        /// </summary>
        /// <param name="file">The file to download.</param>
        /// <param name="contentDisposition">Indicates how to display the document once downloaded.</param>
        void WriteFileToResponse(IFileInfo file, ContentDisposition contentDisposition);
    }
}
