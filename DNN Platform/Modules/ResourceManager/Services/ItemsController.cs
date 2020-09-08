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

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Assets;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Api;
    using Dnn.Modules.ResourceManager.Components;
    using Dnn.Modules.ResourceManager.Helpers;
    using Dnn.Modules.ResourceManager.Services.Attributes;
    using Dnn.Modules.ResourceManager.Services.Dto;
    using CreateNewFolderRequest = Dnn.Modules.ResourceManager.Services.Dto.CreateNewFolderRequest;

    /// <summary>
    /// Expose any services via this class. You can keep services in separate classes or all together in one service class
    /// </summary>
    [ResourceManagerExceptionFilter]
    [DnnExceptionFilter]
    [SupportedModules("ResourceManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ItemsController : DnnApiController
    {
        private readonly IFolderMappingController _folderMappingController = FolderMappingController.Instance;
        private readonly IModuleControlPipeline modulePipeline;

        public ItemsController(IModuleControlPipeline modulePipeline)
        {
            this.modulePipeline = modulePipeline;
        }

        [HttpGet]
        public HttpResponseMessage GetFolderContent(int folderId, int startIndex, int numItems, string sorting)
        {
            ContentPage p;
            var groupId = FindGroupId(Request);
            var moduleId = Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;
            var permissionsManager = PermissionsManager.Instance;

            p = ItemsManager.Instance.GetFolderContent(folderId, startIndex, numItems, sorting, moduleMode);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                folder = new
                {
                    folderId = p.Folder.FolderID,
                    folderName = p.Folder.FolderName,
                    folderMappingId = p.Folder.FolderMappingID,
                    folderPath = p.Folder.FolderPath,
                    folderParentId = p.Folder.ParentID
                },
                items = p.Items.Select(GetItemViewModel),
                totalCount = p.TotalCount,
                hasAddFilesPermission = permissionsManager.HasAddFilesPermission(moduleMode, folderId),
                hasAddFoldersPermission = permissionsManager.HasAddFoldersPermission(moduleMode, folderId),
                hasDeletePermission = permissionsManager.HasDeletePermission(moduleMode, folderId),
                hasManagePermission = permissionsManager.HasManagePermission(moduleMode, folderId)
            });
        }

        [HttpGet]
        public HttpResponseMessage ThumbnailDownLoad([FromUri] ThumbnailDownloadRequest item)
        {
            var file = FileManager.Instance.GetFile(item.FileId);
            if (file == null || !PermissionsManager.Instance.HasGetFileContentPermission(file.FolderId))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { Message = "File doesn't exist." });
            }

            var thumbnailsManager = ThumbnailsManager.Instance;
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            var thumbnail = thumbnailsManager.GetThumbnailContent(file, item.Width, item.Height, true);
            result.Content = thumbnail.Content;
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(thumbnail.ContentType);
            return result;
        }

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

        [HttpGet]
        public HttpResponseMessage GetFolderMappings()
        {
            var isSuperTab = PortalSettings.ActiveTab != null && PortalSettings.ActiveTab.IsSuperTab;

            var mappings = FolderMappingController.Instance.GetFolderMappings(
                isSuperTab && UserInfo.IsSuperUser ? Null.NullInteger : PortalSettings.PortalId);
            var moduleContext = GetModuleContext();

            var r = from m in mappings
                    select new
                    {
                        m.FolderMappingID,
                        m.MappingName,
                        m.FolderProviderType,
                        IsDefault =
                        (m.MappingName == "Standard" || m.MappingName == "Secure" || m.MappingName == "Database"),
                        editUrl = UserInfo.IsAdmin ?
                            moduleContext.EditUrl(
                                "ItemID",
                                m.FolderMappingID.ToString(),
                                "EditFolderMapping",
                                "mid",
                                this.ActiveModule.ModuleID.ToString())
                            :
                            "",
                    };

            return Request.CreateResponse(HttpStatusCode.OK, r);
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        public HttpResponseMessage GetAddFolderTypeUrl()
        {
            var moduleContext = GetModuleContext();
            return Request.CreateResponse(
                HttpStatusCode.OK,
                moduleContext.EditUrl(
                    "ItemID",
                    "-1",
                    "EditFolderMapping",
                    "mid",
                    this.ActiveModule.ModuleID.ToString()));
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Admin)]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveFolderType([FromBody] int folderMappingId)
        {
            this._folderMappingController.DeleteFolderMapping(PortalSettings.PortalId, folderMappingId);
            return Request.CreateResponse(HttpStatusCode.OK, folderMappingId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateNewFolder(CreateNewFolderRequest request)
        {
            var groupId = FindGroupId(Request);
            var moduleId = Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;
            var parentFolder = FolderManager.Instance.GetFolder(request.ParentFolderId);
            var folderMappingId = string.IsNullOrWhiteSpace(parentFolder.FolderPath)
                ? request.FolderMappingId
                : parentFolder.FolderMappingID;

            var folder = ItemsManager.Instance.CreateNewFolder(request.FolderName, request.ParentFolderId, folderMappingId, request.MappedName, moduleMode);

            return Request.CreateResponse(HttpStatusCode.OK,
                new
                {
                    folder.FolderID,
                    folder.FolderName,
                    IconUrl = GetFolderIconUrl(PortalSettings.PortalId, folder.FolderMappingID),
                    folder.FolderMappingID
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteFolder(DeleteFolderRequest request)
        {
            var groupId = FindGroupId(Request);
            var moduleId = Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;

            ItemsManager.Instance.DeleteFolder(request.FolderId, request.UnlinkAllowedStatus, moduleMode);
            return Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteFile(DeleteFileRequest request)
        {
            var groupId = FindGroupId(Request);
            var moduleId = Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;

            ItemsManager.Instance.DeleteFile(request.FileId, moduleMode, groupId);
            return Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpGet]
        public HttpResponseMessage Search(int folderId, string search, int pageIndex, int pageSize, string sorting, string culture)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);
            if (folder == null)
            {
                throw new Exception(LocalizationHelper.GetString("FolderDoesNotExist.Error"));
            }

            var groupId = FindGroupId(Request);
            var moduleId = Request.FindModuleId();
            var moduleMode = new SettingsManager(moduleId, groupId).Mode;

            var searchResults = SearchController.Instance.SearchFolderContent(moduleId, folder, true, search, pageIndex, pageSize, sorting, moduleMode, out int totalHits);

            var items = (from file in searchResults
                         select GetItemViewModel(file)).ToArray();

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                items,
                totalCount = totalHits
            });
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetFileDetails(int fileId)
        {
            var file = FileManager.Instance.GetFile(fileId);
            if (file == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { });
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!FolderPermissionController.CanViewFolder((FolderInfo)folder))
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToReadFileProperties.Error") });
            }

            var createdBy = file.CreatedByUser(PortalSettings.PortalId);
            var lastModifiedBy = file.LastModifiedByUser(PortalSettings.PortalId);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                fileId = file.FileId,
                fileName = file.FileName,
                title = file.Title,
                description = file.Description ?? string.Empty,
                size = string.Format(new FileSizeFormatProvider(), "{0:fs}", file.Size),
                createdOnDate = file.CreatedOnDate.ToShortDateString(),
                createdBy = createdBy != null ? createdBy.Username : "",
                lastModifiedOnDate = file.LastModifiedOnDate.ToShortDateString(),
                lastModifiedBy = lastModifiedBy != null ? lastModifiedBy.Username : "",
                url = FileManager.Instance.GetUrl(file)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveFileDetails(FileDetailsRequest fileDetails)
        {
            var file = FileManager.Instance.GetFile(fileDetails.FileId);
            if (file == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { message = "File doesn't exist." });
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToManageFileProperties.Error") });
            }

            ItemsManager.Instance.SaveFileDetails(file, fileDetails);

            return Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpGet]
        public HttpResponseMessage GetFolderDetails(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);

            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error") });
            }

            var createdBy = folder.CreatedByUser(PortalSettings.PortalId);
            var lastModifiedBy = folder.LastModifiedByUser(PortalSettings.PortalId);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                folderId = folder.FolderID,
                folderName = folder.FolderName,
                createdOnDate = folder.CreatedOnDate.ToShortDateString(),
                createdBy = createdBy != null ? createdBy.Username : "",
                lastModifiedOnDate = folder.LastModifiedOnDate.ToShortDateString(),
                lastModifiedBy = lastModifiedBy != null ? lastModifiedBy.Username : "",
                type = FolderMappingController.Instance.GetFolderMapping(folder.FolderMappingID).MappingName,
                isVersioned = folder.IsVersioned,
                permissions = new FolderPermissions(true, folder.FolderPermissions)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveFolderDetails(FolderDetailsRequest folderDetails)
        {
            var folder = FolderManager.Instance.GetFolder(folderDetails.FolderId);
            if (folder == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, new { message = "Folder doesn't exist." });
            }

            if (!FolderPermissionController.CanManageFolder((FolderInfo)folder))
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError,
                    new { message = LocalizationHelper.GetString("UserHasNoPermissionToManageFolder.Error") });
            }

            ItemsManager.Instance.SaveFolderDetails(folder, folderDetails);

            return Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpGet]
        public HttpResponseMessage GetSortOptions()
        {
            var sortOptions = new[]
            {
                new {value = "LastModifiedOnDate", label = LocalizationHelper.GetString("LastModifiedOnDate")},
                new {value = "CreatedOnDate", label = LocalizationHelper.GetString("CreatedOnDate")},
                new {value = "ItemName", label= LocalizationHelper.GetString("ItemName")}
            };
            return Request.CreateResponse(HttpStatusCode.OK, sortOptions);
        }

        #region Private Methods

        private int FindGroupId(HttpRequestMessage request)
        {
            var headers = request.Headers;
            int groupId;
            int.TryParse(headers.GetValues("GroupId").FirstOrDefault(), out groupId);
            return groupId;
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
                       ? folderMapping.FolderProviderType.Replace("FolderProvider", "")
                       : "standard";

            return string.Format(url, name.ToLower());
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
                    iconUrl = GetFolderIconUrl(PortalSettings.PortalId, folder.FolderMappingID)
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
                thumbnailUrl = thumbnailsManager.ThumbnailUrl(ActiveModule.ModuleID, file.FileId, 110, 110)
            };
        }

        private ModuleInstanceContext GetModuleContext()
        {
            IModuleControl moduleControl = modulePipeline.CreateModuleControl(this.ActiveModule) as IModuleControl;
            return new ModuleInstanceContext(moduleControl);
        }
        #endregion
    }
}
