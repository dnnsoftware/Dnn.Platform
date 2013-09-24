#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Modules.DigitalAssets.Components.Controllers.Models;
using DotNetNuke.Modules.DigitalAssets.Services.Models;
using DotNetNuke.Security;
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
        public HttpResponseMessage GetFolderContent(GetFolderContentRequest r)
        {
            var p = DigitalAssetsController.GetFolderContent(r.FolderId, r.StartIndex, r.NumItems, r.SortExpression);
            return Request.CreateResponse(HttpStatusCode.OK, p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage SearchFolderContent(SearchFolderContentRequest r)
        {
            var p = DigitalAssetsController.SearchFolderContent(r.FolderId, r.Pattern, r.StartIndex, r.NumItems, r.SortExpression);
            return Request.CreateResponse(HttpStatusCode.OK, p);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage DeleteItems(DeleteItemsRequest request)
        {
            var notDeletedItems = DigitalAssetsController.DeleteItems(from i in request.Items select new ItemBaseViewModel { ItemID = i.ItemId, IsFolder = i.IsFolder });
            return Request.CreateResponse(HttpStatusCode.OK, notDeletedItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public HttpResponseMessage RenameFile(RenameFileRequest request)
        {
            var itemViewModel = DigitalAssetsController.RenameFile(request.FileId, request.NewFileName);
            return Request.CreateResponse(HttpStatusCode.OK, itemViewModel);
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
            var subFolders = DigitalAssetsController.GetFolders(request.FolderId);
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
            var folder = DigitalAssetsController.CreateFolder(request.FolderName, request.ParentFolderId, request.FolderMappingId, request.MappedName);
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
            DigitalAssetsController.UnzipFile(request.FileId, request.Overwrite);
            return Request.CreateResponse(HttpStatusCode.OK, "Success");
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