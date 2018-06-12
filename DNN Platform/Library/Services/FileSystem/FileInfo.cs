#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Data;
using System.Drawing;
using System.IO;
using System.Web;
using System.Xml.Serialization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : FileInfo
    /// 
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Represents the File object and holds the Properties of that object
    /// </summary>
    /// -----------------------------------------------------------------------------
    [XmlRoot("file", IsNullable = false)]
    [Serializable]
    public class FileInfo : BaseEntityInfo, IHydratable, IFileInfo
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileInfo));
        private string _folder;
        private bool? _supportsFileAttributes;
        private DateTime? _lastModificationTime;
        private int _folderMappingID;

        private int? _width = null;
        private int? _height = null;
        private string _sha1Hash = null;

        #region Constructors

        public FileInfo()
        {
            UniqueId = Guid.NewGuid();
            VersionGuid = Guid.NewGuid();
        }

        public FileInfo(int portalId, string filename, string extension, int filesize, int width, int height, string contentType, string folder, int folderId, int storageLocation, bool cached)
            : this(portalId, filename, extension, filesize, width, height, contentType, folder, folderId, storageLocation, cached, Null.NullString)
        {
        }

        public FileInfo(int portalId, string filename, string extension, int filesize, int width, int height, string contentType, string folder, int folderId, int storageLocation, bool cached,
                        string hash)
            : this(Guid.NewGuid(), Guid.NewGuid(), portalId, filename, extension, filesize, width, height, contentType, folder, folderId, storageLocation, cached, hash)
        {
        }

        public FileInfo(Guid uniqueId, Guid versionGuid, int portalId, string filename, string extension, int filesize, int width, int height, string contentType, string folder, int folderId,
                        int storageLocation, bool cached, string hash)
        {
            UniqueId = uniqueId;
            VersionGuid = versionGuid;
            PortalId = portalId;
            FileName = filename;
            Extension = extension;
            Size = filesize;
            Width = width;
            Height = height;
            ContentType = contentType;
            Folder = folder;
            FolderId = folderId;
            StorageLocation = storageLocation;
            IsCached = cached;
            SHA1Hash = hash;
        }

        #endregion

        #region Properties

        [XmlElement("contenttype")]
        public string ContentType { get; set; }

        [XmlElement("extension")]
        public string Extension { get; set; }

        [XmlElement("fileid")]
        public int FileId { get; set; }

        [XmlElement("uniqueid")]
        public Guid UniqueId { get; set; }

        [XmlElement("versionguid")]
        public Guid VersionGuid { get; set; }

        [XmlElement("filename")]
        public string FileName { get; set; }

        [XmlElement("folder")]
        public string Folder
        {
            get
            {
                return _folder;
            }
            set
            {
                //Make sure folder name ends with /
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = value + "/";
                }

                _folder = value;
            }
        }

        [XmlElement("folderid")]
        public int FolderId { get; set; }

        [XmlElement("height")]
        public int Height
        {
            get
            {
                if (FileId != 0 && (!_height.HasValue || _height.Value == Null.NullInteger))
                {
                    LoadImageProperties();
                }

                return _height.Value;
            }
            set
            {
                _height = value;
            }
        }

        [XmlElement("iscached")]
        public bool IsCached { get; set; }

        [XmlElement("physicalpath")]
        public string PhysicalPath
        {
            get
            {
                string physicalPath = Null.NullString;
                PortalSettings portalSettings = null;
                if (HttpContext.Current != null)
                {
                    portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                }

                if (PortalId == Null.NullInteger)
                {
                    physicalPath = Globals.HostMapPath + RelativePath;
                }
                else
                {
                    if (portalSettings == null || portalSettings.PortalId != PortalId)
                    {
                        //Get the PortalInfo  based on the Portalid
                        var portal = PortalController.Instance.GetPortal(PortalId);
                        if ((portal != null))
                        {
                            physicalPath = portal.HomeDirectoryMapPath + RelativePath;
                        }
                    }
                    else
                    {
                        physicalPath = portalSettings.HomeDirectoryMapPath + RelativePath;
                    }
                }

                if ((!string.IsNullOrEmpty(physicalPath)))
                {
                    physicalPath = physicalPath.Replace("/", "\\");
                }

                return physicalPath;
            }
        }

        [XmlIgnore]
        public int PortalId { get; set; }

        public string RelativePath
        {
            get
            {
                return Folder + FileName;
            }
        }

        [XmlElement("size")]
        public int Size { get; set; }

        [XmlElement("storagelocation")]
        public int StorageLocation { get; set; }

        [XmlElement("width")]
        public int Width
        {
            get
            {
                if (FileId != 0 && (!_width.HasValue || _width.Value == Null.NullInteger))
                {
                    LoadImageProperties();
                }

                return _width.Value;
            }
            set
            {
                _width = value;
            }
        }

        [XmlElement("sha1hash")]
        public string SHA1Hash
        {
            get
            {
                if (FileId > 0 && string.IsNullOrEmpty(_sha1Hash))
                {
                    LoadHashProperty();
                }

                return _sha1Hash;
            }
            set
            {
                _sha1Hash = value;
            }
        }

        public FileAttributes? FileAttributes
        {
            get
            {
                FileAttributes? _fileAttributes = null;

                if (SupportsFileAttributes)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(PortalId, FolderMappingID);
                    _fileAttributes = FolderProvider.Instance(folderMapping.FolderProviderType).GetFileAttributes(this);
                }

                return _fileAttributes;
            }
        }

        public bool SupportsFileAttributes
        {
            get
            {
                if (!_supportsFileAttributes.HasValue)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(PortalId, FolderMappingID);

                    try
                    {
                        _supportsFileAttributes = FolderProvider.Instance(folderMapping.FolderProviderType).SupportsFileAttributes();
                    }
                    catch
                    {
                        _supportsFileAttributes = false;
                    }
                }

                return _supportsFileAttributes.Value;
            }
        }

        public DateTime LastModificationTime
        {
            get
            {
                if(!_lastModificationTime.HasValue)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(PortalId, FolderMappingID);

                    try
                    {
                        return FolderProvider.Instance(folderMapping.FolderProviderType).GetLastModificationTime(this);
                    }
                    catch
                    {
                        return Null.NullDate;
                    }
                }

                return _lastModificationTime.Value;
            }
            set
            {
                _lastModificationTime = value;
            }
        }

        public int FolderMappingID
        {
            get
            {
                if (_folderMappingID == 0)
                {
                    if (FolderId > 0)
                    {
                        var folder = FolderManager.Instance.GetFolder(FolderId);

                        if (folder != null)
                        {
                            _folderMappingID = folder.FolderMappingID;
                            return _folderMappingID;
                        }
                    }

                    switch (StorageLocation)
                    {
                        case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                            _folderMappingID = FolderMappingController.Instance.GetFolderMapping(PortalId, "Standard").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                            _folderMappingID = FolderMappingController.Instance.GetFolderMapping(PortalId, "Secure").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                            _folderMappingID = FolderMappingController.Instance.GetFolderMapping(PortalId, "Database").FolderMappingID;
                            break;
                        default:
                            _folderMappingID = FolderMappingController.Instance.GetDefaultFolderMapping(PortalId).FolderMappingID;
                            break;
                    }
                }

                return _folderMappingID;
            }
            set
            {
                _folderMappingID = value;
            }
        }

        /// <summary>
        /// Gets or sets a metadata field with an optional title associated to the file
        /// </summary>
        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date on which the file starts to be published
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the date on which the file ends to be published
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether publish period is enabled for the file
        /// </summary>
        public bool EnablePublishPeriod { get; set; }

        /// <summary>
        /// Gets or sets the published version number of the file
        /// </summary>
        public int PublishedVersion { get; set; }

        /// <summary>
        /// Gets a flag which says whether the file has ever been published
        /// </summary>
        [XmlIgnore]
        public bool HasBeenPublished { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the file is enabled,
        /// considering if the publish period is active and if the current date is within the publish period
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                var today = DateTime.Today;
                return !EnablePublishPeriod
                    || (StartDate.Date <= today && (EndDate == Null.NullDate || today <= EndDate.Date));
            }
        }

        /// <summary>
        /// Gets or sets a reference to ContentItem, to use in Workflows
        /// </summary>
        public int ContentItemID { get; set; }

        #endregion

        #region IHydratable Implementation

        public void Fill(IDataReader dr)
        {
            ContentType = Null.SetNullString(dr["ContentType"]);
            Extension = Null.SetNullString(dr["Extension"]);
            FileId = Null.SetNullInteger(dr["FileId"]);
            FileName = Null.SetNullString(dr["FileName"]);
            Folder = Null.SetNullString(dr["Folder"]);
            FolderId = Null.SetNullInteger(dr["FolderId"]);
            Height = Null.SetNullInteger(dr["Height"]);
            IsCached = Null.SetNullBoolean(dr["IsCached"]);
            PortalId  = Null.SetNullInteger(dr["PortalId"]);
            SHA1Hash = Null.SetNullString(dr["SHA1Hash"]);
            Size = Null.SetNullInteger(dr["Size"]);
            StorageLocation = Null.SetNullInteger(dr["StorageLocation"]);
            UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            Width = Null.SetNullInteger(dr["Width"]);
            LastModificationTime = Null.SetNullDateTime(dr["LastModificationTime"]);
            FolderMappingID = Null.SetNullInteger(dr["FolderMappingID"]);
            Title = Null.SetNullString(dr["Title"]);
            Description = Null.SetNullString(dr["Description"]);
            EnablePublishPeriod = Null.SetNullBoolean(dr["EnablePublishPeriod"]);
            StartDate = Null.SetNullDateTime(dr["StartDate"]);
            EndDate  = Null.SetNullDateTime(dr["EndDate"]);
            ContentItemID = Null.SetNullInteger(dr["ContentItemID"]);
            PublishedVersion = Null.SetNullInteger(dr["PublishedVersion"]);
            HasBeenPublished = Convert.ToBoolean(dr["HasBeenPublished"]);
            FillBaseProperties(dr);
        }

        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.FileId;
            }
            set
            {
                FileId = value;
            }
        }

        #endregion

        #region Private methods

        private void LoadImageProperties()
        {            
            var fileManager = (FileManager)FileManager.Instance;
            if (!fileManager.IsImageFile(this))
            {
                _width = _height = 0;
                return;
            }
            var fileContent = fileManager.GetFileContent(this);

            if (fileContent == null)
            {
                //If can't get file content then just exit the function, so it will load again next time.
                return;
            }

            if (!fileContent.CanSeek)
            {
                var tmp = fileManager.GetSeekableStream(fileContent);
                fileContent.Close();
                fileContent = tmp;
            }

            Image image = null;
            try
            {
                image = fileManager.GetImageFromStream(fileContent);

                _width = image.Width;
                _height = image.Height;
            }
            catch
            {
                _width = 0;
                _height = 0;
                ContentType = "application/octet-stream";
            }
            finally
            {
                if (image != null)
                {
                    image.Dispose();
                }
                fileContent.Position = 0;
            }    
            fileContent.Close();
        }

        private void LoadHashProperty()
        {
            var fileManager = (FileManager)FileManager.Instance;
            var currentHashCode = FolderProvider.Instance( FolderMappingController.Instance.GetFolderMapping(FolderMappingID).FolderProviderType).GetHashCode(this);
            if (currentHashCode != _sha1Hash)
            {
                _sha1Hash = currentHashCode;
                fileManager.UpdateFile(this);
            }
            
        }

        #endregion
    }
}