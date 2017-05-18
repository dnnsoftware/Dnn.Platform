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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : FileController
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// Business Class that provides access to the Database for the functions within the calling classes
    /// Instantiates the instance of the DataProvider and returns the object, if any
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager class.")]
    public class FileController
    {
        #region "Obsolete Methods"

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.AddFile(IFolderInfo folder, string fileName, Stream fileContent, bool overwrite) ")]
        public int AddFile(FileInfo file)
        {
            var fileManager = FileManager.Instance;

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            
            var existingFile = fileManager.GetFile(folder, file.FileName);

            if (existingFile == null)
            {
                using (var fileContent = fileManager.GetFileContent(file))
                {
                    file.FileId = fileManager.AddFile(folder, file.FileName, fileContent, false).FileId;
                }
            }
            else
            {
                using (var fileContent = fileManager.GetFileContent(file))
                {
                    file.FileId = fileManager.UpdateFile(file, fileContent).FileId;
                }
            }

            return file.FileId;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.UpdateFile(IFileInfo file) ")]
        public void UpdateFile(FileInfo file)
        {
            FileManager.Instance.UpdateFile(file);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by DatabaseFolderProvider.ClearFileContent(int fileId) ")]
        public void ClearFileContent(int fileId)
        {
            DatabaseFolderProvider.ClearFileContent(fileId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public int ConvertFilePathToFileId(string filePath, int portalID)
        {
            var fileName = Path.GetFileName(filePath);

            var folderPath = filePath.Substring(0, filePath.LastIndexOf(fileName));
            var folder = FolderManager.Instance.GetFolder(portalID, folderPath);

            var file = FileManager.Instance.GetFile(folder, fileName);

            return file.FileId;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.DeleteFile(IFileInfo file) ")]
        public void DeleteFile(int portalId, string fileName, int folderID, bool clearCache)
        {
            var folder = FolderManager.Instance.GetFolder(folderID);
            var file = FileManager.Instance.GetFile(folder, fileName);
            FileManager.Instance.DeleteFile(file);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public void DeleteFiles(int portalId)
        {
            DeleteFiles(portalId, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public void DeleteFiles(int portalId, bool clearCache)
        {
            DataProvider.Instance().DeleteFiles(portalId);

            if (clearCache)
            {
                GetAllFilesRemoveCache();
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public DataTable GetAllFiles()
        {
            var dt = (DataTable) DataCache.GetCache("GetAllFiles");
            if (dt == null)
            {
                dt = DataProvider.Instance().GetAllFiles();
                DataCache.SetCache("GetAllFiles", dt);
            }

            if (dt != null)
            {
                return dt.Copy();
            }

            return new DataTable();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public void GetAllFilesRemoveCache()
        {
            DataCache.RemoveCache("GetAllFiles");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.GetFile(IFolderInfo folder, string fileName) ")]
        public FileInfo GetFile(string fileName, int portalId, int folderID)
        {
            var folder = FolderManager.Instance.GetFolder(folderID);
            return (FileInfo)FileManager.Instance.GetFile(folder, fileName);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.GetFile(int fileID) ")]
        public FileInfo GetFileById(int fileId, int portalId)
        {
            return (FileInfo)FileManager.Instance.GetFile(fileId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public FileInfo GetFileByUniqueID(Guid uniqueId)
        {
            return CBO.FillObject<FileInfo>(DataProvider.Instance().GetFileByUniqueID(uniqueId));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FileManager.Instance.GetFileContent(IFileInfo file) ")]
        public byte[] GetFileContent(int fileId, int portalId)
        {
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by FolderManager.Instance.GetFiles(IFolderInfo folder) ")]
        public IDataReader GetFiles(int portalId, int folderID)
        {
            return DataProvider.Instance().GetFiles(folderID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by DatabaseFolderProvider.UpdateFileContent(int fileId, Stream content) ")]
        public void UpdateFileContent(int fileId, Stream content)
        {
            DatabaseFolderProvider.UpdateFileContent(fileId, content);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.  It has been replaced by DatabaseFolderProvider.UpdateFileContent(int fileId, byte[] content) ")]
        public void UpdateFileContent(int fileId, byte[] content)
        {
            DatabaseFolderProvider.UpdateFileContent(fileId, content);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static XmlNode SerializeFile(XmlDocument xmlFile, FileInfo file)
        {
            CBO.SerializeObject(file, xmlFile);

            XmlNode nodeTab = xmlFile.SelectSingleNode("file");
            nodeTab.Attributes.Remove(nodeTab.Attributes["xmlns:xsd"]);
            nodeTab.Attributes.Remove(nodeTab.Attributes["xmlns:xsi"]);

            return nodeTab;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.0.")]
        public static FileInfo DeserializeFile(XmlNode nodeFile, int portalId, int folderId)
        {
            var node = nodeFile.SelectSingleNode("file");
            
            var newFile = new FileInfo
            {
                UniqueId = new Guid(XmlUtils.GetNodeValue(node.CreateNavigator(), "uniqueid")),
                VersionGuid = new Guid(XmlUtils.GetNodeValue(node.CreateNavigator(), "versionguid")),
                PortalId = portalId,
                FileName = XmlUtils.GetNodeValue(node.CreateNavigator(), "filename"),
                Folder = XmlUtils.GetNodeValue(node.CreateNavigator(), "folder"),
                FolderId = folderId,
                ContentType = XmlUtils.GetNodeValue(node.CreateNavigator(), "contenttype"),
                Extension = XmlUtils.GetNodeValue(node.CreateNavigator(), "extension"),
                StorageLocation = XmlUtils.GetNodeValueInt(node, "storagelocation"),
                IsCached = XmlUtils.GetNodeValueBoolean(node, "iscached", false),
                Size = XmlUtils.GetNodeValueInt(node, "size", Null.NullInteger),
                Width = XmlUtils.GetNodeValueInt(node, "width", Null.NullInteger),
                Height = XmlUtils.GetNodeValueInt(node, "height", Null.NullInteger)
            };

            // create/update file
            var fileCtrl = new FileController();

            var originalFile = fileCtrl.GetFileByUniqueID(newFile.UniqueId);

            if (originalFile == null)
            {
                var folder = FolderManager.Instance.GetFolder(folderId);
                using (var fileContent = FileManager.Instance.GetFileContent(newFile))
                {
                    newFile.FileId = FileManager.Instance.AddFile(folder, newFile.FileName, fileContent, false).FileId;
                }
            }
            else
            {
                newFile.FileId = originalFile.FileId;
            }

            return (FileInfo)FileManager.Instance.UpdateFile(newFile);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by AddFile(ByVal file As FileInfo)")]
        public int AddFile(FileInfo file, string folderPath)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(file.PortalId, folderPath, false);
            file.FolderId = objFolder.FolderID;
            file.Folder = folderPath;
            return AddFile(file);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by AddFile(ByVal file As FileInfo)")]
        public int AddFile(int portalId, string fileName, string extension, long size, int width, int height, string contentType, string folderPath)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(portalId, folderPath, false);
            var objFile = new FileInfo();

            objFile.UniqueId = Guid.NewGuid();
            objFile.VersionGuid = Guid.NewGuid();

            objFile.PortalId = portalId;
            objFile.FileName = fileName;
            objFile.Extension = extension;
            objFile.Size = Convert.ToInt32(size);
            objFile.Width = width;
            objFile.Height = height;
            objFile.ContentType = contentType;
            objFile.Folder = FileSystemUtils.FormatFolderPath(folderPath);
            objFile.FolderId = objFolder.FolderID;
            objFile.IsCached = true;

            return AddFile(objFile);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by AddFile(ByVal file As FileInfo)")]
        public int AddFile(int portalId, string fileName, string extension, long size, int width, int height, string ContentType, string FolderPath, bool ClearCache)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(portalId, FolderPath, false);
            var objFile = new FileInfo();

            objFile.UniqueId = Guid.NewGuid();
            objFile.VersionGuid = Guid.NewGuid();

            objFile.PortalId = portalId;
            objFile.FileName = fileName;
            objFile.Extension = extension;
            objFile.Size = Convert.ToInt32(size);
            objFile.Width = width;
            objFile.Height = height;
            objFile.ContentType = ContentType;
            objFile.Folder = FileSystemUtils.FormatFolderPath(FolderPath);
            objFile.FolderId = objFolder.FolderID;
            objFile.IsCached = ClearCache;

            return AddFile(objFile);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by DeleteFile(PortalId, FileName, FolderID, ClearCache)")]
        public void DeleteFile(int PortalId, string FileName, string FolderPath, bool ClearCache)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(PortalId, FolderPath, false);
            DeleteFile(PortalId, FileName, objFolder.FolderID, ClearCache);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by DeleteFile(PortalId, FileName, FolderID, ClearCache)")]
        public void DeleteFile(int PortalId, string FileName, string FolderPath)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(PortalId, FolderPath, false);
            DeleteFile(PortalId, FileName, objFolder.FolderID, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by GetFile(FileName, PortalId, FolderID)")]
        public FileInfo GetFile(string FilePath, int PortalId)
        {
            var objFolders = new FolderController();
            string FileName = Path.GetFileName(FilePath);
            FolderInfo objFolder = objFolders.GetFolder(PortalId, FilePath.Replace(FileName, ""), false);
            if (objFolder == null)
            {
                return null;
            }
            else
            {
                return GetFile(FileName, PortalId, objFolder.FolderID);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by GetFile(FileName, PortalId, FolderID)")]
        public FileInfo GetFile(string FileName, int PortalId, string FolderPath)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(PortalId, FolderPath, false);
            if (objFolder == null)
            {
                return null;
            }
            else
            {
                return GetFile(FileName, PortalId, objFolder.FolderID);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by GetFiles(PortalId, FolderID)")]
        public IDataReader GetFiles(int PortalId, string FolderPath)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(PortalId, FolderPath, false);
            if (objFolder == null)
            {
                return null;
            }
            return GetFiles(PortalId, objFolder.FolderID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This function has been replaced by ???")]
        public ArrayList GetFilesByFolder(int portalId, string folderPath)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(portalId, folderPath, false);
            if (objFolder == null)
            {
                return null;
            }
            return CBO.FillCollection(GetFiles(portalId, objFolder.FolderID), typeof (FileInfo));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by UpdateFile(ByVal file As FileInfo)")]
        public void UpdateFile(int PortalId, string OriginalFileName, string FileName, string Extension, long Size, int Width, int Height, string ContentType, string SourceFolder,
                               string DestinationFolder)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(PortalId, DestinationFolder, false);
            FileInfo objFile = GetFile(OriginalFileName, PortalId, objFolder.FolderID);

            objFile.FileName = FileName;
            objFile.Extension = Extension;
            objFile.Size = Convert.ToInt32(Size);
            objFile.Width = Width;
            objFile.Height = Height;
            objFile.ContentType = ContentType;
            objFile.Folder = DestinationFolder;

            if ((objFile != null))
            {
                UpdateFile(objFile);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by UpdateFile(ByVal file As FileInfo)")]
        public void UpdateFile(int PortalId, string OriginalFileName, string FileName, string Extension, long Size, int Width, int Height, string ContentType, string SourceFolder,
                               string DestinationFolder, bool ClearCache)
        {
            var objFolders = new FolderController();
            FolderInfo objFolder = objFolders.GetFolder(PortalId, DestinationFolder, false);
            FileInfo objFile = GetFile(OriginalFileName, PortalId, objFolder.FolderID);

            objFile.FileName = FileName;
            objFile.Extension = Extension;
            objFile.Size = Convert.ToInt32(Size);
            objFile.Width = Width;
            objFile.Height = Height;
            objFile.ContentType = ContentType;
            objFile.Folder = DestinationFolder;

            if ((objFile != null))
            {
                UpdateFile(objFile);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by UpdateFile(ByVal file As FileInfo)")]
        public void UpdateFile(int PortalId, string OriginalFileName, string FileName, string Extension, long Size, int Width, int Height, string ContentType, string SourceFolder,
                               string DestinationFolder, int FolderID, bool ClearCache)
        {
            FileInfo objFile = GetFile(OriginalFileName, PortalId, FolderID);

            objFile.FileName = FileName;
            objFile.Extension = Extension;
            objFile.Size = Convert.ToInt32(Size);
            objFile.Width = Width;
            objFile.Height = Height;
            objFile.ContentType = ContentType;
            objFile.Folder = DestinationFolder;
            objFile.FolderId = FolderID;

            if ((objFile != null))
            {
                UpdateFile(objFile);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by AddFile(ByVal file As FileInfo)")]
        public int AddFile(int PortalId, string FileName, string Extension, long Size, int Width, int Height, string ContentType, string FolderPath, int FolderID, bool ClearCache)
        {
            var objFile = new FileInfo();

            objFile.UniqueId = Guid.NewGuid();
            objFile.VersionGuid = Guid.NewGuid();

            objFile.PortalId = PortalId;
            objFile.FileName = FileName;
            objFile.Extension = Extension;
            objFile.Size = Convert.ToInt32(Size);
            objFile.Width = Width;
            objFile.Height = Height;
            objFile.ContentType = ContentType;
            objFile.Folder = FileSystemUtils.FormatFolderPath(FolderPath);
            objFile.FolderId = FolderID;

            if (ClearCache)
            {
                GetAllFilesRemoveCache();
            }

            return AddFile(objFile);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DotNetNuke 5.5. This function has been replaced by UpdateFile(ByVal file As FileInfo)")]
        public void UpdateFile(int FileId, string FileName, string Extension, long Size, int Width, int Height, string ContentType, string DestinationFolder, int FolderID)
        {
            var objFile = new FileInfo();

            objFile.FileId = FileId;
            objFile.VersionGuid = Guid.NewGuid();

            objFile.FileName = FileName;
            objFile.Extension = Extension;
            objFile.Size = Convert.ToInt32(Size);
            objFile.Width = Width;
            objFile.Height = Height;
            objFile.ContentType = ContentType;
            objFile.Folder = FileSystemUtils.FormatFolderPath(DestinationFolder);
            objFile.FolderId = FolderID;

            UpdateFile(objFile);
        }

        #endregion
    }
}
