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

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    [SupportedModules("DotNetNuke.Modules.DigitalAssets")]    
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DamExceptionFilter]
    public class ContentServiceController : DnnApiController
    {
        protected static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentServiceController));

        public ContentServiceController()
        {
            var f = new Factory();
            DigitalAssetsController = f.DigitalAssetsController;
        }

        protected IDigitalAssetsController DigitalAssetsController { get; private set; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetFolderContent(GetFolderContentRequest r)
        {
            var moduleId = Request.FindModuleId();
            var p = DigitalAssetsController.GetFolderContent(moduleId, r.FolderId, r.StartIndex, r.NumItems, r.SortExpression);
            return Request.CreateResponse(HttpStatusCode.OK, p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage SearchFolderContent(SearchFolderContentRequest r)
        {
            var moduleId = Request.FindModuleId();
            var p = DigitalAssetsController.SearchFolderContent(moduleId, r.FolderId, r.Pattern, r.StartIndex, r.NumItems, r.SortExpression);
            return Request.CreateResponse(HttpStatusCode.OK, p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage DeleteItems(DeleteItemsRequest request)
        {
            var notDeletedItems = DigitalAssetsController.DeleteItems(request.Items);
            return Request.CreateResponse(HttpStatusCode.OK, notDeletedItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UnlinkFolder(UnlinkFolderRequest request)
        {
            DigitalAssetsController.UnlinkFolder(request.FolderId);
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetMappedSubfoldersCount(MappedPathSubFoldersCountRequest request)
        {
            var mappedSubfoldersCount = DigitalAssetsController.GetMappedSubFoldersCount(request.Items, PortalSettings.PortalId);
            return Request.CreateResponse(HttpStatusCode.OK, mappedSubfoldersCount);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage RenameFile(RenameFileRequest request)
        {
	        try
	        {
		        var itemViewModel = DigitalAssetsController.RenameFile(request.FileId, request.NewFileName);
		        return Request.CreateResponse(HttpStatusCode.OK, itemViewModel);
	        }
	        catch (FileAlreadyExistsException ex)
	        {
				return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
	        }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage CopyFile(CopyMoveItemRequest request)
        {
            var copyFileResponse = DigitalAssetsController.CopyFile(request.ItemId, request.DestinationFolderId, request.Overwrite);
            return Request.CreateResponse(HttpStatusCode.OK, copyFileResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage MoveFile(CopyMoveItemRequest request)
        {
            var copyMoveFileResponse = DigitalAssetsController.MoveFile(request.ItemId, request.DestinationFolderId, request.Overwrite);
            return Request.CreateResponse(HttpStatusCode.OK, copyMoveFileResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage MoveFolder(CopyMoveItemRequest request)
        {
            var copyMoveFolderResponse = DigitalAssetsController.MoveFolder(request.ItemId, request.DestinationFolderId, request.Overwrite);
            return Request.CreateResponse(HttpStatusCode.OK, copyMoveFolderResponse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetSubFolders(GetSubFolderRequest request)
        {
            var moduleId = Request.FindModuleId();
            var subFolders = DigitalAssetsController.GetFolders(moduleId, request.FolderId).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, subFolders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage RenameFolder(RenameFolderRequest request)
        {
            DigitalAssetsController.RenameFolder(request.FolderId, request.NewFolderName);
            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage CreateNewFolder(CreateNewFolderRequest request)
        {
            var folder = DigitalAssetsController.CreateFolder(request.FolderName, request.ParentFolderId,
                request.FolderMappingId, request.MappedName);
            return Request.CreateResponse(HttpStatusCode.OK, folder);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SynchronizeFolder(SynchronizeFolderRequest request)
        {
            DigitalAssetsController.SyncFolderContent(request.FolderId, request.Recursive);
            return Request.CreateResponse(HttpStatusCode.OK, "Success");            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage UnzipFile(UnzipFileRequest request)
        {
            var model = DigitalAssetsController.UnzipFile(request.FileId, request.Overwrite);
            return Request.CreateResponse(HttpStatusCode.OK, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetUrl(GetUrlRequest request)
        {
            var url = DigitalAssetsController.GetUrl(request.FileId);
            return Request.CreateResponse(HttpStatusCode.OK, url);
        }
    }
}