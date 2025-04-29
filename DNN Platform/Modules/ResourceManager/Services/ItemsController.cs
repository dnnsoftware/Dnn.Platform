// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.Modules.ResourceManager.Services;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

using Dnn.Modules.ResourceManager.Components;
using Dnn.Modules.ResourceManager.Helpers;
using Dnn.Modules.ResourceManager.Services.Attributes;
using Dnn.Modules.ResourceManager.Services.Dto;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Security.Permissions;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Assets;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Api;

using CreateNewFolderRequest = Dnn.Modules.ResourceManager.Services.Dto.CreateNewFolderRequest;

/// <summary>Expose any services via this class. You can keep services in separate classes or all together in one service class.</summary>
[ResourceManagerExceptionFilter]
[DnnExceptionFilter]
[SupportedModules("ResourceManager")]
[DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
public class ItemsController : DnnApiController
{
    private readonly IFolderMappingController folderMappingController = FolderMappingController.Instance;
    private readonly IModuleControlPipeline modulePipeline;
    private readonly IApplicationStatusInfo applicationStatusInfo;
    private readonly Hashtable mappedPathsSupported = new Hashtable();
    private readonly IPermissionDefinitionService permissionDefinitionService;

    /// <summary>Initializes a new instance of the <see cref="ItemsController"/> class.</summary>
    /// <param name="modulePipeline">An instance of an <see cref="IModuleControlPipeline"/> used to hook into the EditUrl of the webforms folders provider settings UI.</param>
    /// <param name="applicationStatusInfo">The application status info.</param>
    /// <param name="permissionDefinitionService">The permission service.</param>
    public ItemsController(
        IModuleControlPipeline modulePipeline,
        IApplicationStatusInfo applicationStatusInfo,
        IPermissionDefinitionService permissionDefinitionService)
    {
        this.modulePipeline = modulePipeline;
        this.applicationStatusInfo = applicationStatusInfo;
        this.permissionDefinitionService = permissionDefinitionService;
    }

    /// <summary>Gets the content for a specific folder.</summary>
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

    /// <summary>Gets the module settings.</summary>
    /// <returns><see cref="SettingsManager"/>.</returns>
    [HttpGet]
    public IHttpActionResult GetSettings()
    {
        var groupId = this.FindGroupId(this.Request);
        var moduleId = this.Request.FindModuleId();
        var settings = new SettingsManager(moduleId, groupId);
        return this.Ok(settings);
    }

    /// <summary>Gets an item representation for a provided folder ID.</summary>
    /// <param name="folderId">The ID of the folder to get.</param>
    /// <returns>An Item viewmodel.</returns>
    [HttpGet]

    public IHttpActionResult GetFolderItem(int folderId)
    {
        var folder = FolderManager.Instance.GetFolder(folderId);
        var item = this.GetItemViewModel(folder);
        return this.Ok(item);
    }

    /// <summary>Syncs the folder content.</summary>
    /// <param name="folderId">The folder id.</param>
    /// <param name="numItems">The number of items.</param>
    /// <param name="sorting">The sorting.</param>
    /// <param name="recursive">If true sync recursively.</param>
    /// <returns>The http response message.</returns>
    [HttpGet]

    public HttpResponseMessage SyncFolderContent(int folderId, int numItems, string sorting, bool recursive)
    {
        var folder = FolderManager.Instance.GetFolder(folderId);
        FolderManager.Instance.Synchronize(folder.PortalID, folder.FolderPath, recursive, true);
        return this.GetFolderContent(folderId, 0, numItems, sorting);
    }

    /// <summary>Download thumbnail.</summary>
    /// <param name="item">The thumbnail download request.</param>
    /// <returns>The http repsonse message.</returns>
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

    /// <summary>Downloads a file.</summary>
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

    /// <summary>Gets a list of folder mappings.</summary>
    /// <returns>
    /// A list of folder mapping including information such as if the folder mapping is default or not
    /// and a url to edit the folder mapping using the provider settings UI.
    /// </returns>
    [HttpGet]

    public HttpResponseMessage GetFolderMappings()
    {
        var isSuperTab = this.PortalSettings.ActiveTab != null && this.PortalSettings.ActiveTab.IsSuperTab;

        var moduleContext = this.GetModuleContext();
        var mappings = FolderMappingController.Instance.GetFolderMappings(
            isSuperTab && this.UserInfo.IsSuperUser ?
                Null.NullInteger :
                this.PortalSettings.PortalId);

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

    /// <summary>Determines whether or not the current user has permissions to manage the folder types.</summary>
    /// <returns>
    /// A boolean indicating whether or not the current user has permissions to manage the folder types.
    /// </returns>
    [HttpGet]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public HttpResponseMessage CanManageFolderTypes()
    {
        var canManage = this.UserInfo.IsSuperUser || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName);
        return this.Request.CreateResponse(HttpStatusCode.OK, canManage);
    }

    /// <summary>Gets a url to add a new folder type.</summary>
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

    /// <summary>Removes a folder type.</summary>
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

    /// <summary>Attempts to create a new folder.</summary>
    /// <param name="request">The request to add a new folder, <see cref="CreateNewFolderRequest"/>.</param>
    /// <returns>Information about the new folder.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]

    public HttpResponseMessage CreateNewFolder(CreateNewFolderRequest request)
    {
        var groupId = this.FindGroupId(this.Request);
        var moduleId = this.Request.FindModuleId();
        var moduleMode = new SettingsManager(moduleId, groupId).Mode;

        var folder = ItemsManager.Instance.CreateNewFolder(request.FolderName, request.ParentFolderId, request.FolderMappingId, request.MappedName, moduleMode);

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

    /// <summary>Attempts to delete a folder.</summary>
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

    /// <summary>Attempts to delete a file.</summary>
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

    /// <summary>Performs file search in a folder.</summary>
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

    /// <summary>Gets details about a file.</summary>
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
            iconUrl = GetFileIconUrl(file.Extension),
        });
    }

    /// <summary>Attempts to save new details about a file.</summary>
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

    /// <summary>Gets details about a folder.</summary>
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
            permissions = this.permissionDefinitionService.GetFolderPermissions(folder.FolderPermissions),
        });
    }

    /// <summary>Attempts to save new details about a folder.</summary>
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

    /// <summary>Gets available sorting options.</summary>
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

    /// <summary>Attempts to move a file into a folder.</summary>
    /// <param name="moveFileRequest">The file move request, <see cref="MoveFileRequest"/>.</param>
    /// <returns>A 0 status code if succeeded.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]

    public HttpResponseMessage MoveFile(MoveFileRequest moveFileRequest)
    {
        var groupId = this.FindGroupId(this.Request);
        var moduleId = this.Request.FindModuleId();
        var moduleMode = new SettingsManager(moduleId, groupId).Mode;

        ItemsManager.Instance.MoveFile(moveFileRequest.SourceFileId, moveFileRequest.DestinationFolderId, moduleMode, groupId);
        return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
    }

    /// <summary>Attempts to move a folder into another folder.</summary>
    /// <param name="moveFolderRequest">The folder move request, <see cref="MoveFolderRequest"/>.</param>
    /// <returns>A 0 status code if succeeded.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]

    public HttpResponseMessage MoveFolder(MoveFolderRequest moveFolderRequest)
    {
        var groupId = this.FindGroupId(this.Request);
        var moduleId = this.Request.FindModuleId();
        var moduleMode = new SettingsManager(moduleId, groupId).Mode;

        ItemsManager.Instance.MoveFolder(moveFolderRequest.SourceFolderId, moveFolderRequest.DestinationFolderId, moduleMode, groupId);
        return this.Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
    }

    /// <summary>Gets the relevant icon for this requested folder.</summary>
    /// <param name="folderId">The ID of the folder for which to get the image for.</param>
    /// <returns>A string representing the full url  to the folder icon.</returns>
    [HttpGet]
    [ValidateAntiForgeryToken]

    public IHttpActionResult GetFolderIconUrl(int folderId)
    {
        var folderMappingId = FolderManager.Instance.GetFolder(folderId).FolderMappingID;
        var url = GetFolderIconUrl(this.PortalSettings.PortalId, folderMappingId);
        return this.Ok(new { url });
    }

    /// <summary>Gets a list of role groups.</summary>
    /// <returns>A collection of role groups.</returns>
    [HttpGet]
    [ValidateAntiForgeryToken]

    public IHttpActionResult GetRoleGroups()
    {
        if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
        {
            return this.Unauthorized();
        }

        var groups = RoleController.GetRoleGroups(this.PortalSettings.PortalId)
            .Cast<RoleGroupInfo>()
            .Select(RoleGroupDto.FromRoleGroupInfo);

        return this.Ok(groups);
    }

    /// <summary>Gets the roles for a role group.</summary>
    /// <returns>A collection of roles.</returns>
    [HttpGet]
    [ValidateAntiForgeryToken]

    public IHttpActionResult GetRoles()
    {
        var matchedRoles = RoleController.Instance.GetRoles(this.PortalSettings.PortalId)
            .Where(r => r.Status == RoleStatus.Approved)
            .OrderBy(r => r.RoleName)
            .Select(r => new
            {
                IsSystemRole = r.IsSystemRole,
                RoleGroupId = r.RoleGroupID,
                RoleId = r.RoleID,
                RoleName = r.RoleName,
            })
            .ToList();

        return this.Ok(matchedRoles);
    }

    /// <summary>Gets a list of users that match the search keyword.</summary>
    /// <param name="keyword">The keyword to search for.</param>
    /// <param name="count">The amount of results to return.</param>
    /// <returns>A collection of users.</returns>
    [HttpGet]
    [ValidateAntiForgeryToken]

    public IHttpActionResult GetSuggestionUsers(string keyword, int count)
    {
        try
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return this.Ok(new List<string>());
            }

            var displayMatch = keyword + "%";
            var totalRecords = 0;
            var totalRecords2 = 0;
            var matchedUsers = UserController.GetUsersByDisplayName(
                this.PortalSettings.PortalId,
                displayMatch,
                0,
                count,
                ref totalRecords,
                false,
                false);
            matchedUsers.AddRange(
                UserController.GetUsersByUserName(
                    this.PortalSettings.PortalId,
                    displayMatch,
                    0,
                    count,
                    ref totalRecords2,
                    false,
                    false));
            var finalUsers = matchedUsers
                .Cast<UserInfo>()
                .Where(x => x.Membership.Approved)
                .Select(u => new
                {
                    userId = u.UserID,
                    displayName = $"{u.DisplayName}",
                });

            return this.Ok(finalUsers.ToList().GroupBy(x => x.userId).Select(group => group.First()));
        }
        catch (Exception ex)
        {
            return this.InternalServerError(new Exception(ex.Message));
        }
    }

    /// <summary>Gets the list of possible file extensions as well as the validation code to use for uploads.</summary>
    /// <returns>An object containing the allowed file extensions and the validation code to use for uploads.</returns>
    [HttpGet]
    [ValidateAntiForgeryToken]

    public IHttpActionResult GetAllowedFileExtensions()
    {
        var allowedExtensions = FileManager.Instance.WhiteList.ToStorageString();
        var parameters = new List<object>() { allowedExtensions.Split(',').Select(i => i.Trim()).OrderBy(a => a).ToList() };
        parameters.Add(this.PortalSettings.UserInfo.UserID);
        if (!this.UserInfo.IsSuperUser)
        {
            parameters.Add(this.PortalSettings.PortalId);
        }

        var validationCode = ValidationUtils.ComputeValidationCode(parameters);

        var maxUploadFileSize = Config.GetMaxUploadSize(this.applicationStatusInfo);

        return this.Ok(new { allowedExtensions, validationCode, maxUploadFileSize });
    }

    private static string GetFileIconUrl(string extension)
    {
        if (!string.IsNullOrEmpty(extension) && File.Exists(HostingEnvironment.MapPath(IconController.IconURL("Ext" + extension, "32x32", "Standard"))))
        {
            return IconController.IconURL("Ext" + extension, "32x32", "Standard");
        }

        return IconController.IconURL("ExtFile", "32x32", "Standard");
    }

    private static string GetFolderIconUrl(int portalId, int folderMappingId)
    {
        var folderMapping = FolderMappingController.Instance.GetFolderMapping(portalId, folderMappingId);
        var svgPath = Constants.ModulePath + "images/icon-asset-manager-{0}-folder.svg";

        if (folderMapping is null)
        {
            return Globals.ApplicationPath + "/" + string.Format(svgPath, "standard");
        }

        var localSvg = string.Format(svgPath, folderMapping.FolderProviderType.Replace("FolderProvider", string.Empty).ToLowerInvariant());
        if (File.Exists(HostingEnvironment.MapPath($"~/{localSvg}")))
        {
            return Globals.ApplicationPath + "/" + localSvg;
        }

        if (File.Exists(HostingEnvironment.MapPath(folderMapping.ImageUrl)))
        {
            return VirtualPathUtility.ToAbsolute(folderMapping.ImageUrl);
        }

        return Globals.ApplicationPath + "/" + string.Format(svgPath, "standard");
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
                createdOn = folder.CreatedOnDate,
                modifiedOn = folder.LastModifiedOnDate,
                unlinkAllowedStatus = this.GetUnlinkAllowedStatus(folder),
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
            createdOn = file.CreatedOnDate,
            modifiedOn = file.LastModifiedOnDate,
            fileSize = file.Size,
        };
    }

    private string GetUnlinkAllowedStatus(IFolderInfo folder)
    {
        if (this.AreMappedPathsSupported(folder.FolderMappingID) && folder.ParentID > 0 && FolderManager.Instance.GetFolder(folder.ParentID).FolderMappingID != folder.FolderMappingID)
        {
            return "onlyUnlink";
        }

        if (this.AreMappedPathsSupported(folder.FolderMappingID))
        {
            return "true";
        }

        return "false";
    }

    private bool AreMappedPathsSupported(int folderMappingId)
    {
        if (this.mappedPathsSupported.ContainsKey(folderMappingId))
        {
            return (bool)this.mappedPathsSupported[folderMappingId];
        }

        var folderMapping = FolderMappingController.Instance.GetFolderMapping(folderMappingId);
        var folderProvider = FolderProvider.Instance(folderMapping.FolderProviderType);
        var result = folderProvider.SupportsMappedPaths;
        this.mappedPathsSupported[folderMappingId] = result;
        return result;
    }

    private ModuleInstanceContext GetModuleContext()
    {
        IModuleControl moduleControl = this.modulePipeline.CreateModuleControl(this.ActiveModule) as IModuleControl;
        return new ModuleInstanceContext(moduleControl);
    }
}
