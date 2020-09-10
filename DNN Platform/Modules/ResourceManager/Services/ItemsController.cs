// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web;
    using System.Web.Http;

    using Dnn.Modules.ResourceManager.Components;
    using Dnn.Modules.ResourceManager.Helpers;
    using Dnn.Modules.ResourceManager.Services.Attributes;
    using Dnn.Modules.ResourceManager.Services.Dto;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Assets;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;

    using CreateNewFolderRequest = Dnn.Modules.ResourceManager.Services.Dto.CreateNewFolderRequest;

    /// <summary>
    /// Expose any services via this class. You can keep services in separate classes or all together in one service class.
    /// </summary>
    [ResourceManagerExceptionFilter]
    [DnnExceptionFilter]
    [SupportedModules("ResourceManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ItemsController : DnnApiController
    {
        private readonly IFolderMappingController folderMappingController = FolderMappingController.Instance;
        private readonly IModuleControlPipeline modulePipeline;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsController"/> class.
        /// </summary>
        /// <param name="modulePipeline">
        /// An instance of an <see cref="IModuleControlPipeline"/> used to hook into the
        /// EditUrl of the webforms folders provider settings UI.
        /// </param>
        public ItemsController(IModuleControlPipeline modulePipeline)
        {
            this.modulePipeline = modulePipeline;
        }

        /// <summary>
        /// Gets the content for a specific folder.
        /// </summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <param name="startIndex">The page number to get.</param>
        /// <param name="numItems">How many items to get per page.</param>
        /// <param name="sorting">How to sort the list.</param>
        /// <returns>
        /// An object containing the folder information, a list of the folder contents and the permissions relating to that folder.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetFolderContent(int folderId, int startIndex, int numItems, string sorting)
        {
            ContentPage p;
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;
            var permissionsManager = PermissionsManager.Instance;

            p = ItemsManager.Instance.GetFolderContent(folderId, startIndex, numItems, sorting, moduleMode);

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                folder = new
                {
                    folderId = p.Folder.FolderID,
                    folderName = p.Folder.FolderName,
                    folderMappingId = p.Folder.FolderMappingID,
                    folderPath = p.Folder.FolderPath,
                    folderParentId = p.Folder.ParentID,
                },
                items = p.Items.Select(this.GetItemViewModel),
                totalCount = p.TotalCount,
                hasAddFilesPermission = permissionsManager.HasAddFilesPermission(moduleMode, folderId),
                hasAddFoldersPermission = permissionsManager.HasAddFoldersPermission(moduleMode, folderId),
                hasDeletePermission = permissionsManager.HasDeletePermission(moduleMode, folderId),
                hasManagePermission = permissionsManager.HasManagePermission(moduleMode, folderId),
            });
        }

        /// <summary>
        /// Gets an image of the thumbnail for an item.
        /// </summary>
        /// <param name="item">The thumbnail to get, <see cref="ThumbnailDownloadRequest"/>.</param>
        /// <returns>An image file.</returns>
        [HttpGet]
        public HttpResponseMessage SyncFolderContent(int folderId, int numItems, string sorting, bool recursive)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, recursive, true);
            return GetFolderContent(folderId, 0, numItems, sorting);
        }

        [HttpGet]
        public HttpResponseMessage ThumbnailDownLoad([FromUri] ThumbnailDownloadRequest item)
        {
            var file = FileManager.Instance.GetFile(item.FileId);
            if (file == null || !PermissionsManager.Instance.HasGetFileContentPermission(file.FolderId))
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "File doesn't exist." });
            }

            var thumbnailsManager = ThumbnailsManager.Instance;
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var thumbnail = thumbnailsManager.GetThumbnailContent(file, item.Width, item.Height, true);
            result.Content = thumbnail.Content;
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(thumbnail.ContentType);
            return result;
        }

        /// <summary>
        /// Downloads a file.
        /// </summary>
        /// <param name="fileId">The id of the file to download.</param>
        /// <param name="forceDownload">
        /// A value indicating whether to force the download.
        /// When true, will download the file as an attachment and ensures the browser won't just render the file if supported.
        /// When false, the browser may render the file instead of downloading it for some formats like pdf or images.
        /// </param>
        /// <returns>The actual requested file.</returns>
        [HttpGet]
        public HttpResponseMessage Download(int fileId, bool forceDownload)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var streamContent = ItemsManager.Instance.GetFileContent(fileId, out var fileName, out var contentType);
            result.Content = new StreamContent(streamContent);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue(forceDownload ? "attachment" : "inline") { FileName = fileName };
            return result;
        }

        /// <summary>
        /// Gets a list of folder mappings.
        /// </summary>
        /// <returns>
        /// A list of folder mapping including information such as if the folder mapping is default or not
        /// and a url to edit the folder mapping using the provider settings UI.
        /// </returns>
        [HttpGet]
        public HttpResponseMessage GetFolderMappings()
        {
            var isSuperTab = this.PortalSettings.ActiveTab != null && this.PortalSettings.ActiveTab.IsSuperTab;

            var mappings = FolderMappingController.Instance.GetFolderMappings(
                isSuperTab && UserInfo.IsSuperUser ? 
                    Null.NullInteger : 
                    PortalSettings.PortalId);

            var r = from m in mappings
                    select new
                    {
                        m.FolderMappingID,
                        m.MappingName,
                        m.FolderProviderType,
                        IsDefault =
                        m.MappingName == "Standard" || m.MappingName == "Secure" || m.MappingName == "Database",
                        editUrl = this.UserInfo.IsAdmin ?
                            moduleContext.EditUrl(
                                "ItemID",
                                m.FolderMappingID.ToString(),
                                "EditFolderMapping",
                                "mid",
                                this.ActiveModule.ModuleID.ToString())
                            :
                            string.Empty,
                    };

            return this.Request.CreateResponse(HttpStatusCode.OK, r);
        }

        /// <summary>
        /// Gets a url to add a new folder type.
        /// </summary>
        /// <returns>A url to the folder providers control that allows adding a new folder type.</returns>
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        public HttpResponseMessage GetAddFolderTypeUrl()
        {
            var moduleContext = this.GetModuleContext();
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                moduleContext.EditUrl(
                    "ItemID",
                    "-1",
                    "EditFolderMapping",
                    "mid",
                    this.ActiveModule.ModuleID.ToString()));
        }

        /// <summary>
        /// Removes a folder type.
        /// </summary>
        /// <param name="folderMappingId">The id of an existing folder mapping.</param>
        /// <returns>The id of the recently remove folder type.</returns>
        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveFolderType([FromBody] int folderMappingId)
        {
            this.folderMappingController.DeleteFolderMapping(this.PortalSettings.PortalId, folderMappingId);
            return this.Request.CreateResponse(HttpStatusCode.OK, folderMappingId);
        }

        /// <summary>
        /// Attempts to create a new folder.
        /// </summary>
        /// <param name="request">The request to add a new folder, <see cref="CreateNewFolderRequest"/>.</param>
        /// <returns>Information about the new folder.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateNewFolder(CreateNewFolderRequest request)
        {
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;
            var parentFolder = FolderManager.Instance.GetFolder(request.ParentFolderId);
            var folderMappingId = string.IsNullOrWhiteSpace(parentFolder.FolderPath)
                ? request.FolderMappingId
                : parentFolder.FolderMappingID;

            var folder = ItemsManager.Instance.CreateNewFolder(request.FolderName, request.ParentFolderId, folderMappingId, request.MappedName, moduleMode);

            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new
                {
                    folder.FolderID,
                    folder.FolderName,
                    IconUrl = GetFolderIconUrl(this.PortalSettings.PortalId, folder.FolderMappingID),
                    folder.FolderMappingID,
                });
        }

        /// <summary>
        /// Attempts to delete a folder.
        /// </summary>
        /// <param name="request">The request to delete a folder, <see cref="DeleteFolderRequest"/>.</param>
        /// <returns>Ok if succedded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteFolder(DeleteFolderRequest request)
        {
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;

            ItemsManager.Instance.DeleteFolder(request.FolderId, request.UnlinkAllowedStatus, moduleMode);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>
        /// Attempts to delete a file.
        /// </summary>
        /// <param name="request">The file deletion request, <see cref="DeleteFileRequest"/>.</param>
        /// <returns>OK if succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteFile(DeleteFileRequest request)
        {
            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;

            ItemsManager.Instance.DeleteFile(request.FileId, moduleMode, groupId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>
        /// Performs file search in a folder.
        /// </summary>
        /// <param name="folderId">The id of the folder.</param>
        /// <param name="search">The search term.</param>
        /// <param name="pageIndex">The page index to get.</param>
        /// <param name="pageSize">How many items per page.</param>
        /// <param name="sorting">How to sort the results.</param>
        /// <param name="culture">The culture requested.</param>
        /// <returns>A list of the found resources together with the total count of found resources.</returns>
        [HttpGet]
        public HttpResponseMessage Search(int folderId, string search, int pageIndex, int pageSize, string sorting, string culture)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
            {
                throw new Exception(LocalizationHelper.GetString("FolderDoesNotExist.Error"));
            }

            var groupId = this.FindGroupId(this.Request);
            var moduleId = this.Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;

            var searchResults = SearchController.Instance.SearchFolderContent(moduleId, folder, true, search, pageIndex, pageSize, sorting, moduleMode, out int totalHits);

            var items = (from file in searchResults
                         select this.GetItemViewModel(file)).ToArray();

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                items,
                totalCount = totalHits,
            });
        }

        /// <summary>
        /// Gets details about a file.
        /// </summary>
        /// <param name="fileId">The id of the file to get the details from.</param>
        /// <returns>Detailed information about the file.</returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetFileDetails(int fileId)
        {
            var file = FileManager.Instance.GetFile(fileId);
            if (file == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { });
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!FolderPermissionController.CanViewFolder((FolderInfo)folder))
            {
                return this.Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToReadFileProperties.Error") });
            }

            var createdBy = file.CreatedByUser(this.PortalSettings.PortalId);
            var lastModifiedBy = file.LastModifiedByUser(this.PortalSettings.PortalId);

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                fileId = file.FileId,
                fileName = file.FileName,
                title = file.Title,
                description = file.Description ?? string.Empty,
                size = string.Format(new FileSizeFormatProvider(), "{0:fs}", file.Size),
                createdOnDate = file.CreatedOnDate.ToShortDateString(),
                createdBy = createdBy != null ? createdBy.Username : string.Empty,
                lastModifiedOnDate = file.LastModifiedOnDate.ToShortDateString(),
                lastModifiedBy = lastModifiedBy != null ? lastModifiedBy.Username : string.Empty,
                url = FileManager.Instance.GetUrl(file),
            });
        }

        /// <summary>
        /// Attempts to save new details about a file.
        /// </summary>
        /// <param name="fileDetails">The new file details, <see cref="FileDetailsRequest"/>.</param>
        /// <returns>OK if the request succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveFileDetails(FileDetailsRequest fileDetails)
        {
            var file = FileManager.Instance.GetFile(fileDetails.FileId);
            if (file == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { message = "File doesn't exist." });
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                return this.Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToManageFileProperties.Error") });
            }

            ItemsManager.Instance.SaveFileDetails(file, fileDetails);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>
        /// Gets details about a folder.
        /// </summary>
        /// <param name="folderId">The id of the folder from which to get the details.</param>
        /// <returns>Detailed information about the folder.</returns>
        [HttpGet]
        public HttpResponseMessage GetFolderDetails(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);

            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                return this.Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error") });
            }

            var createdBy = folder.CreatedByUser(this.PortalSettings.PortalId);
            var lastModifiedBy = folder.LastModifiedByUser(this.PortalSettings.PortalId);

            return this.Request.CreateResponse(HttpStatusCode.OK, new
            {
                folderId = folder.FolderID,
                folderName = folder.FolderName,
                createdOnDate = folder.CreatedOnDate.ToShortDateString(),
                createdBy = createdBy != null ? createdBy.Username : string.Empty,
                lastModifiedOnDate = folder.LastModifiedOnDate.ToShortDateString(),
                lastModifiedBy = lastModifiedBy != null ? lastModifiedBy.Username : string.Empty,
                type = FolderMappingController.Instance.GetFolderMapping(folder.FolderMappingID).MappingName,
                isVersioned = folder.IsVersioned,
                permissions = new FolderPermissions(true, folder.FolderPermissions),
            });
        }

        /// <summary>
        /// Attempts to save new details about a folder.
        /// </summary>
        /// <param name="folderDetails">The new folder details, <see cref="FolderDetailsRequest"/>.</param>
        /// <returns>OK if the request succeeded.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveFolderDetails(FolderDetailsRequest folderDetails)
        {
            var folder = FolderManager.Instance.GetFolder(folderDetails.FolderId);
            if (folder == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound, new { message = "Folder doesn't exist." });
            }

            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                return this.Request.CreateResponse(
                    HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error") });
            }

            ItemsManager.Instance.SaveFolderDetails(folder, folderDetails);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        /// <summary>
        /// Gets available sorting options.
        /// </summary>
        /// <returns>A list of values and labels to populate the search sorting dropdown.</returns>
        [HttpGet]
        public HttpResponseMessage GetSortOptions()
        {
            var sortOptions = new[]
            {
                new { value = "LastModifiedOnDate", label = LocalizationHelper.GetString("LastModifiedOnDate") },
                new { value = "CreatedOnDate", label = LocalizationHelper.GetString("CreatedOnDate") },
                new { value = "ItemName", label= LocalizationHelper.GetString("ItemName") },
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, sortOptions);
        }

        private static string GetFileIconUrl(string extension)
        {
            if (!string.IsNullOrEmpty(extension) && File.Exists(HttpContext.Current.Server.MapPath(IconController.IconURL("Ext" + extension, "32x32", "Standard"))))
            {
                return IconController.IconURL("Ext" + extension, "32x32", "Standard");
            }

            return IconController.IconURL("ExtFile", "32x32", "Standard");
        }

        private static string GetFolderIconUrl(int portalId, int folderMappingId)
        {
            var url = Globals.ApplicationPath + "/" + Constants.ModulePath + "images/icon-asset-manager-{0}-folder.png";

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folderMappingId);
            var name = folderMapping != null && File.Exists(HttpContext.Current.Server.MapPath(folderMapping.ImageUrl))
                       ? folderMapping.FolderProviderType.Replace("FolderProvider", string.Empty)
                       : "standard";

            return string.Format(url, name.ToLower());
        }

        private int FindGroupId(HttpRequestMessage request)
        {
            var headers = request.Headers;
            int groupId;
            int.TryParse(headers.GetValues("GroupId").FirstOrDefault(), out groupId);
            return groupId;
        }

        private object GetItemViewModel(object item)
        {
            var folder = item as IFolderInfo;
            if (folder != null)
            {
                return new
                {
                    isFolder = true,
                    itemId = folder.FolderID,
                    itemName = folder.FolderName,
                    iconUrl = GetFolderIconUrl(this.PortalSettings.PortalId, folder.FolderMappingID),
                };
            }

            var thumbnailsManager = ThumbnailsManager.Instance;
            var file = item as IFileInfo;
            return new
            {
                isFolder = false,
                itemId = file.FileId,
                itemName = file.FileName,
                path = FileManager.Instance.GetUrl(file),
                iconUrl = GetFileIconUrl(file.Extension),
                thumbnailAvailable = thumbnailsManager.ThumbnailAvailable(file.FileName),
                thumbnailUrl = thumbnailsManager.ThumbnailUrl(this.ActiveModule.ModuleID, file.FileId, 110, 110),
            };
        }

        private ModuleInstanceContext GetModuleContext()
        {
            IModuleControl moduleControl = this.modulePipeline.CreateModuleControl(this.ActiveModule) as IModuleControl;
            return new ModuleInstanceContext(moduleControl);
        }
    }
}
