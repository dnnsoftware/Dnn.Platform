// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
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

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : FileInfo
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Represents the File object and holds the Properties of that object.
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

        public FileInfo()
        {
            this.UniqueId = Guid.NewGuid();
            this.VersionGuid = Guid.NewGuid();
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
            this.UniqueId = uniqueId;
            this.VersionGuid = versionGuid;
            this.PortalId = portalId;
            this.FileName = filename;
            this.Extension = extension;
            this.Size = filesize;
            this.Width = width;
            this.Height = height;
            this.ContentType = contentType;
            this.Folder = folder;
            this.FolderId = folderId;
            this.StorageLocation = storageLocation;
            this.IsCached = cached;
            this.SHA1Hash = hash;
        }

        [XmlElement("height")]
        public int Height
        {
            get
            {
                if (this.FileId != 0 && (!this._height.HasValue || this._height.Value == Null.NullInteger))
                {
                    this.LoadImageProperties();
                }

                return this._height.Value;
            }

            set
            {
                this._height = value;
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

                if (this.PortalId == Null.NullInteger)
                {
                    physicalPath = Globals.HostMapPath + this.RelativePath;
                }
                else
                {
                    if (portalSettings == null || portalSettings.PortalId != this.PortalId)
                    {
                        // Get the PortalInfo  based on the Portalid
                        var portal = PortalController.Instance.GetPortal(this.PortalId);
                        if (portal != null)
                        {
                            physicalPath = portal.HomeDirectoryMapPath + this.RelativePath;
                        }
                    }
                    else
                    {
                        physicalPath = portalSettings.HomeDirectoryMapPath + this.RelativePath;
                    }
                }

                if (!string.IsNullOrEmpty(physicalPath))
                {
                    physicalPath = physicalPath.Replace("/", "\\");
                }

                return physicalPath;
            }
        }

        public string RelativePath
        {
            get
            {
                return this.Folder + this.FileName;
            }
        }

        public FileAttributes? FileAttributes
        {
            get
            {
                FileAttributes? _fileAttributes = null;

                if (this.SupportsFileAttributes)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(this.PortalId, this.FolderMappingID);
                    _fileAttributes = FolderProvider.Instance(folderMapping.FolderProviderType).GetFileAttributes(this);
                }

                return _fileAttributes;
            }
        }

        public bool SupportsFileAttributes
        {
            get
            {
                if (!this._supportsFileAttributes.HasValue)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(this.PortalId, this.FolderMappingID);

                    try
                    {
                        this._supportsFileAttributes = FolderProvider.Instance(folderMapping.FolderProviderType).SupportsFileAttributes();
                    }
                    catch
                    {
                        this._supportsFileAttributes = false;
                    }
                }

                return this._supportsFileAttributes.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the file is enabled,
        /// considering if the publish period is active and if the current date is within the publish period.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                var today = DateTime.Today;
                return !this.EnablePublishPeriod
                    || (this.StartDate.Date <= today && (this.EndDate == Null.NullDate || today <= this.EndDate.Date));
            }
        }

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
                return this._folder;
            }

            set
            {
                // Make sure folder name ends with /
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = value + "/";
                }

                this._folder = value;
            }
        }

        [XmlElement("folderid")]
        public int FolderId { get; set; }


        [XmlIgnore]
        public int PortalId { get; set; }

        [XmlElement("size")]
        public int Size { get; set; }

        [XmlElement("storagelocation")]
        public int StorageLocation { get; set; }

        [XmlElement("width")]
        public int Width
        {
            get
            {
                if (this.FileId != 0 && (!this._width.HasValue || this._width.Value == Null.NullInteger))
                {
                    this.LoadImageProperties();
                }

                return this._width.Value;
            }

            set
            {
                this._width = value;
            }
        }

        [XmlElement("sha1hash")]
        public string SHA1Hash
        {
            get
            {
                if (this.FileId > 0 && string.IsNullOrEmpty(this._sha1Hash))
                {
                    this.LoadHashProperty();
                }

                return this._sha1Hash;
            }

            set
            {
                this._sha1Hash = value;
            }
        }

        public DateTime LastModificationTime
        {
            get
            {
                if (!this._lastModificationTime.HasValue)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(this.PortalId, this.FolderMappingID);

                    try
                    {
                        return FolderProvider.Instance(folderMapping.FolderProviderType).GetLastModificationTime(this);
                    }
                    catch
                    {
                        return Null.NullDate;
                    }
                }

                return this._lastModificationTime.Value;
            }

            set
            {
                this._lastModificationTime = value;
            }
        }

        public int FolderMappingID
        {
            get
            {
                if (this._folderMappingID == 0)
                {
                    if (this.FolderId > 0)
                    {
                        var folder = FolderManager.Instance.GetFolder(this.FolderId);

                        if (folder != null)
                        {
                            this._folderMappingID = folder.FolderMappingID;
                            return this._folderMappingID;
                        }
                    }

                    switch (this.StorageLocation)
                    {
                        case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                            this._folderMappingID = FolderMappingController.Instance.GetFolderMapping(this.PortalId, "Standard").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                            this._folderMappingID = FolderMappingController.Instance.GetFolderMapping(this.PortalId, "Secure").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                            this._folderMappingID = FolderMappingController.Instance.GetFolderMapping(this.PortalId, "Database").FolderMappingID;
                            break;
                        default:
                            this._folderMappingID = FolderMappingController.Instance.GetDefaultFolderMapping(this.PortalId).FolderMappingID;
                            break;
                    }
                }

                return this._folderMappingID;
            }

            set
            {
                this._folderMappingID = value;
            }
        }

        /// <summary>
        /// Gets or sets a metadata field with an optional title associated to the file.
        /// </summary>
        public string Title { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the date on which the file starts to be published.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the date on which the file ends to be published.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether publish period is enabled for the file.
        /// </summary>
        public bool EnablePublishPeriod { get; set; }

        /// <summary>
        /// Gets or sets the published version number of the file.
        /// </summary>
        public int PublishedVersion { get; set; }

        /// <summary>
        /// Gets a value indicating whether gets a flag which says whether the file has ever been published.
        /// </summary>
        [XmlIgnore]
        public bool HasBeenPublished { get; private set; }

        /// <summary>
        /// Gets or sets a reference to ContentItem, to use in Workflows.
        /// </summary>
        public int ContentItemID { get; set; }

        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return this.FileId;
            }

            set
            {
                this.FileId = value;
            }
        }

        public void Fill(IDataReader dr)
        {
            this.ContentType = Null.SetNullString(dr["ContentType"]);
            this.Extension = Null.SetNullString(dr["Extension"]);
            this.FileId = Null.SetNullInteger(dr["FileId"]);
            this.FileName = Null.SetNullString(dr["FileName"]);
            this.Folder = Null.SetNullString(dr["Folder"]);
            this.FolderId = Null.SetNullInteger(dr["FolderId"]);
            this.Height = Null.SetNullInteger(dr["Height"]);
            this.IsCached = Null.SetNullBoolean(dr["IsCached"]);
            this.PortalId = Null.SetNullInteger(dr["PortalId"]);
            this.SHA1Hash = Null.SetNullString(dr["SHA1Hash"]);
            this.Size = Null.SetNullInteger(dr["Size"]);
            this.StorageLocation = Null.SetNullInteger(dr["StorageLocation"]);
            this.UniqueId = Null.SetNullGuid(dr["UniqueId"]);
            this.VersionGuid = Null.SetNullGuid(dr["VersionGuid"]);
            this.Width = Null.SetNullInteger(dr["Width"]);
            this.LastModificationTime = Null.SetNullDateTime(dr["LastModificationTime"]);
            this.FolderMappingID = Null.SetNullInteger(dr["FolderMappingID"]);
            this.Title = Null.SetNullString(dr["Title"]);
            this.Description = Null.SetNullString(dr["Description"]);
            this.EnablePublishPeriod = Null.SetNullBoolean(dr["EnablePublishPeriod"]);
            this.StartDate = Null.SetNullDateTime(dr["StartDate"]);
            this.EndDate = Null.SetNullDateTime(dr["EndDate"]);
            this.ContentItemID = Null.SetNullInteger(dr["ContentItemID"]);
            this.PublishedVersion = Null.SetNullInteger(dr["PublishedVersion"]);
            this.HasBeenPublished = Convert.ToBoolean(dr["HasBeenPublished"]);
            this.FillBaseProperties(dr);
        }

        private void LoadImageProperties()
        {
            var fileManager = (FileManager)FileManager.Instance;
            if (!fileManager.IsImageFile(this))
            {
                this._width = this._height = 0;
                return;
            }

            var fileContent = fileManager.GetFileContent(this);

            if (fileContent == null)
            {
                // If can't get file content then just exit the function, so it will load again next time.
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

                this._width = image.Width;
                this._height = image.Height;
            }
            catch
            {
                this._width = 0;
                this._height = 0;
                this.ContentType = "application/octet-stream";
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
            var currentHashCode = FolderProvider.Instance(FolderMappingController.Instance.GetFolderMapping(this.FolderMappingID).FolderProviderType).GetHashCode(this);
            if (currentHashCode != this._sha1Hash)
            {
                this._sha1Hash = currentHashCode;
                fileManager.UpdateFile(this);
            }
        }
    }
}
