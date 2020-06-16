// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
    using DotNetNuke.Modules.DigitalAssets.Services.Models;
    using DotNetNuke.Security;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;

    [SupportedModules("DotNetNuke.Modules.DigitalAssets")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DamExceptionFilter]
    public class ContentServiceController : DnnApiController
    {
        protected static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentServiceController));

        public ContentServiceController()
        {
            var f = new Factory();
            this.DigitalAssetsController = f.DigitalAssetsController;
        }

        protected IDigitalAssetsController DigitalAssetsController { get; private set; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetFolderContent(GetFolderContentRequest r)
        {
            var moduleId = this.Request.FindModuleId();
            var p = this.DigitalAssetsController.GetFolderContent(moduleId, r.FolderId, r.StartIndex, r.NumItems, r.SortExpression);
            return this.Request.CreateResponse(HttpStatusCode.OK, p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SearchFolderContent(SearchFolderContentRequest r)
        {
            var moduleId = this.Request.FindModuleId();
            var p = this.DigitalAssetsController.SearchFolderContent(moduleId, r.FolderId, r.Pattern, r.StartIndex, r.NumItems, r.SortExpression);
            return this.Request.CreateResponse(HttpStatusCode.OK, p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteItems(DeleteItemsRequest request)
        {
            var notDeletedItems = this.DigitalAssetsController.DeleteItems(request.Items);
            return this.Request.CreateResponse(HttpStatusCode.OK, notDeletedItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UnlinkFolder(UnlinkFolderRequest request)
        {
            this.DigitalAssetsController.UnlinkFolder(request.FolderId);
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetMappedSubfoldersCount(MappedPathSubFoldersCountRequest request)
        {
            var mappedSubfoldersCount = this.DigitalAssetsController.GetMappedSubFoldersCount(request.Items, this.PortalSettings.PortalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, mappedSubfoldersCount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RenameFile(RenameFileRequest request)
        {
            try
            {
                var itemViewModel = this.DigitalAssetsController.RenameFile(request.FileId, request.NewFileName);
                return this.Request.CreateResponse(HttpStatusCode.OK, itemViewModel);
            }
            catch (FileAlreadyExistsException ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CopyFile(CopyMoveItemRequest request)
        {
            var copyFileResponse = this.DigitalAssetsController.CopyFile(request.ItemId, request.DestinationFolderId, request.Overwrite);
            return this.Request.CreateResponse(HttpStatusCode.OK, copyFileResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MoveFile(CopyMoveItemRequest request)
        {
            var copyMoveFileResponse = this.DigitalAssetsController.MoveFile(request.ItemId, request.DestinationFolderId, request.Overwrite);
            return this.Request.CreateResponse(HttpStatusCode.OK, copyMoveFileResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MoveFolder(CopyMoveItemRequest request)
        {
            var copyMoveFolderResponse = this.DigitalAssetsController.MoveFolder(request.ItemId, request.DestinationFolderId, request.Overwrite);
            return this.Request.CreateResponse(HttpStatusCode.OK, copyMoveFolderResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetSubFolders(GetSubFolderRequest request)
        {
            var moduleId = this.Request.FindModuleId();
            var subFolders = this.DigitalAssetsController.GetFolders(moduleId, request.FolderId).ToList();
            return this.Request.CreateResponse(HttpStatusCode.OK, subFolders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RenameFolder(RenameFolderRequest request)
        {
            this.DigitalAssetsController.RenameFolder(request.FolderId, request.NewFolderName);
            return this.Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateNewFolder(CreateNewFolderRequest request)
        {
            var folder = this.DigitalAssetsController.CreateFolder(request.FolderName, request.ParentFolderId,
                request.FolderMappingId, request.MappedName);
            return this.Request.CreateResponse(HttpStatusCode.OK, folder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SynchronizeFolder(SynchronizeFolderRequest request)
        {
            this.DigitalAssetsController.SyncFolderContent(request.FolderId, request.Recursive);
            return this.Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UnzipFile(UnzipFileRequest request)
        {
            var model = this.DigitalAssetsController.UnzipFile(request.FileId, request.Overwrite);
            return this.Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetUrl(GetUrlRequest request)
        {
            var url = this.DigitalAssetsController.GetUrl(request.FileId);
            return this.Request.CreateResponse(HttpStatusCode.OK, url);
        }
    }
}
