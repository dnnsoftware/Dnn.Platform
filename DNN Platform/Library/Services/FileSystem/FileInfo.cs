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
    using Newtonsoft.Json;

    /// <summary>Represents the File object and holds the Properties of that object.</summary>
    [XmlRoot("file", IsNullable = false)]
    [Serializable]
    public class FileInfo : BaseEntityInfo, IHydratable, IFileInfo
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileInfo));
        private string folder;
        private bool? supportsFileAttributes;
        private DateTime? lastModificationTime;
        private int folderMappingID;

        private int? width = null;
        private int? height = null;
        private string sha1Hash = null;

        /// <summary>Initializes a new instance of the <see cref="FileInfo"/> class.</summary>
        public FileInfo()
        {
            this.UniqueId = Guid.NewGuid();
            this.VersionGuid = Guid.NewGuid();
        }

        /// <summary>Initializes a new instance of the <see cref="FileInfo"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="extension">The extension of the file (without leading <c>.</c>).</param>
        /// <param name="filesize">The length of the file in bytes.</param>
        /// <param name="width">If the file is an image, the width of the image in pixels, otherwise <see cref="Null.NullInteger"/>.</param>
        /// <param name="height">If the file is an image, the height of the image in pixels, otherwise <see cref="Null.NullInteger"/>.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="folder">The folder path of the file's folder.</param>
        /// <param name="folderId">The ID of the file's folder.</param>
        /// <param name="storageLocation">The value of the <see cref="FolderController.StorageLocationTypes"/> for this file.</param>
        /// <param name="cached">Whether the file is cached.</param>
        public FileInfo(int portalId, string filename, string extension, int filesize, int width, int height, string contentType, string folder, int folderId, int storageLocation, bool cached)
            : this(portalId, filename, extension, filesize, width, height, contentType, folder, folderId, storageLocation, cached, Null.NullString)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FileInfo"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="extension">The extension of the file (without leading <c>.</c>).</param>
        /// <param name="filesize">The length of the file in bytes.</param>
        /// <param name="width">If the file is an image, the width of the image in pixels, otherwise <see cref="Null.NullInteger"/>.</param>
        /// <param name="height">If the file is an image, the height of the image in pixels, otherwise <see cref="Null.NullInteger"/>.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="folder">The folder path of the file's folder.</param>
        /// <param name="folderId">The ID of the file's folder.</param>
        /// <param name="storageLocation">The value of the <see cref="FolderController.StorageLocationTypes"/> for this file.</param>
        /// <param name="cached">Whether the file is cached.</param>
        /// <param name="hash">The SHA1 hash of the file contents.</param>
        public FileInfo(int portalId, string filename, string extension, int filesize, int width, int height, string contentType, string folder, int folderId, int storageLocation, bool cached, string hash)
            : this(Guid.NewGuid(), Guid.NewGuid(), portalId, filename, extension, filesize, width, height, contentType, folder, folderId, storageLocation, cached, hash)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FileInfo"/> class.</summary>
        /// <param name="uniqueId">The unique ID of the file.</param>
        /// <param name="versionGuid">The unique ID of the file version.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="filename">The name of the file.</param>
        /// <param name="extension">The extension of the file (without leading <c>.</c>).</param>
        /// <param name="filesize">The length of the file in bytes.</param>
        /// <param name="width">If the file is an image, the width of the image in pixels, otherwise <see cref="Null.NullInteger"/>.</param>
        /// <param name="height">If the file is an image, the height of the image in pixels, otherwise <see cref="Null.NullInteger"/>.</param>
        /// <param name="contentType">The content type of the file.</param>
        /// <param name="folder">The folder path of the file's folder.</param>
        /// <param name="folderId">The ID of the file's folder.</param>
        /// <param name="storageLocation">The value of the <see cref="FolderController.StorageLocationTypes"/> for this file.</param>
        /// <param name="cached">Whether the file is cached.</param>
        /// <param name="hash">The SHA1 hash of the file contents or <see cref="Null.NullString"/>.</param>
        public FileInfo(Guid uniqueId, Guid versionGuid, int portalId, string filename, string extension, int filesize, int width, int height, string contentType, string folder, int folderId, int storageLocation, bool cached, string hash)
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

        /// <inheritdoc/>
        [XmlElement("height")]
        public int Height
        {
            get
            {
                if (this.FileId != 0 && (!this.height.HasValue || this.height.Value == Null.NullInteger))
                {
                    this.LoadImageProperties();
                }

                return this.height.Value;
            }

            set
            {
                this.height = value;
            }
        }

        /// <inheritdoc/>
        [XmlElement("iscached")]
        public bool IsCached { get; set; }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public string RelativePath
        {
            get
            {
                return this.Folder + this.FileName;
            }
        }

        /// <inheritdoc/>
        public FileAttributes? FileAttributes
        {
            get
            {
                FileAttributes? fileAttributes = null;

                if (this.SupportsFileAttributes)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(this.PortalId, this.FolderMappingID);
                    fileAttributes = FolderProvider.Instance(folderMapping.FolderProviderType).GetFileAttributes(this);
                }

                return fileAttributes;
            }
        }

        /// <inheritdoc/>
        public bool SupportsFileAttributes
        {
            get
            {
                if (!this.supportsFileAttributes.HasValue)
                {
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(this.PortalId, this.FolderMappingID);

                    try
                    {
                        this.supportsFileAttributes = FolderProvider.Instance(folderMapping.FolderProviderType).SupportsFileAttributes();
                    }
                    catch
                    {
                        this.supportsFileAttributes = false;
                    }
                }

                return this.supportsFileAttributes.Value;
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

        /// <inheritdoc/>
        [XmlElement("contenttype")]
        public string ContentType { get; set; }

        /// <inheritdoc/>
        [XmlElement("extension")]
        public string Extension { get; set; }

        /// <inheritdoc/>
        [XmlElement("fileid")]
        public int FileId { get; set; }

        /// <inheritdoc/>
        [XmlElement("uniqueid")]
        public Guid UniqueId { get; set; }

        /// <inheritdoc/>
        [XmlElement("versionguid")]
        public Guid VersionGuid { get; set; }

        /// <inheritdoc/>
        [XmlElement("filename")]
        public string FileName { get; set; }

        /// <inheritdoc/>
        [XmlElement("folder")]
        public string Folder
        {
            get
            {
                return this.folder;
            }

            set
            {
                // Make sure folder name ends with /
                if (!string.IsNullOrEmpty(value) && !value.EndsWith("/"))
                {
                    value = value + "/";
                }

                this.folder = value;
            }
        }

        /// <inheritdoc/>
        [XmlElement("folderid")]
        public int FolderId { get; set; }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
        public int PortalId { get; set; }

        /// <inheritdoc/>
        [XmlElement("size")]
        public int Size { get; set; }

        /// <inheritdoc/>
        [XmlElement("storagelocation")]
        public int StorageLocation { get; set; }

        /// <inheritdoc/>
        [XmlElement("width")]
        public int Width
        {
            get
            {
                if (this.FileId != 0 && (!this.width.HasValue || this.width.Value == Null.NullInteger))
                {
                    this.LoadImageProperties();
                }

                return this.width.Value;
            }

            set
            {
                this.width = value;
            }
        }

        /// <inheritdoc/>
        [XmlElement("sha1hash")]
        public string SHA1Hash
        {
            get
            {
                if (this.FileId > 0 && string.IsNullOrEmpty(this.sha1Hash))
                {
                    this.LoadHashProperty();
                }

                return this.sha1Hash;
            }

            set
            {
                this.sha1Hash = value;
            }
        }

        /// <inheritdoc/>
        public DateTime LastModificationTime
        {
            get
            {
                if (!this.lastModificationTime.HasValue)
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

                return this.lastModificationTime.Value;
            }

            set
            {
                this.lastModificationTime = value;
            }
        }

        /// <inheritdoc/>
        public int FolderMappingID
        {
            get
            {
                if (this.folderMappingID == 0)
                {
                    if (this.FolderId > 0)
                    {
                        var folder = FolderManager.Instance.GetFolder(this.FolderId);

                        if (folder != null)
                        {
                            this.folderMappingID = folder.FolderMappingID;
                            return this.folderMappingID;
                        }
                    }

                    switch (this.StorageLocation)
                    {
                        case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                            this.folderMappingID = FolderMappingController.Instance.GetFolderMapping(this.PortalId, "Standard").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                            this.folderMappingID = FolderMappingController.Instance.GetFolderMapping(this.PortalId, "Secure").FolderMappingID;
                            break;
                        case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                            this.folderMappingID = FolderMappingController.Instance.GetFolderMapping(this.PortalId, "Database").FolderMappingID;
                            break;
                        default:
                            this.folderMappingID = FolderMappingController.Instance.GetDefaultFolderMapping(this.PortalId).FolderMappingID;
                            break;
                    }
                }

                return this.folderMappingID;
            }

            set
            {
                this.folderMappingID = value;
            }
        }

        /// <summary>Gets or sets a metadata field with an optional title associated to the file.</summary>
        public string Title { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <summary>Gets or sets the date on which the file starts to be published.</summary>
        public DateTime StartDate { get; set; }

        /// <summary>Gets or sets the date on which the file ends to be published.</summary>
        public DateTime EndDate { get; set; }

        /// <summary>Gets or sets a value indicating whether publish period is enabled for the file.</summary>
        public bool EnablePublishPeriod { get; set; }

        /// <summary>Gets or sets the published version number of the file.</summary>
        public int PublishedVersion { get; set; }

        /// <summary>Gets a value indicating whether the file has ever been published.</summary>
        [XmlIgnore]
        [JsonIgnore]
        public bool HasBeenPublished { get; private set; }

        /// <summary>Gets or sets a reference to ContentItem, to use in Workflows.</summary>
        public int ContentItemID { get; set; }

        /// <inheritdoc/>
        [XmlIgnore]
        [JsonIgnore]
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

        /// <inheritdoc/>
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
                this.width = this.height = 0;
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

                this.width = image.Width;
                this.height = image.Height;
            }
            catch
            {
                this.width = 0;
                this.height = 0;
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
            if (currentHashCode != this.sha1Hash)
            {
                this.sha1Hash = currentHashCode;
                fileManager.UpdateFile(this);
            }
        }
    }
}
