#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
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

using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.ExportImport.Components;
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Services;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Dnn.ExportImport.Services
{
    [DnnAuthorize(StaticRoles = "Administrators")]
    public class ExportImportController : DnnApiController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Export(ExportDto exportDto)
        {

#if DEBUG
            //TODO: This code is here for testing only.
            UsersExportService.Instance.ExportData(0);
#endif
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            if (!isHostUser && exportDto.PortalId != PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            var controller = new ExportController();
            var operationId = controller.QueueOperation(PortalSettings.UserId, exportDto);
            return Request.CreateResponse(HttpStatusCode.OK, new {refId = operationId});
        }

#if DEBUG
        //TODO: This code is here for testing only.
        [HttpGet]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Read()
        {
            UsersExportService.Instance.ImportData(0);
            return Request.CreateResponse(HttpStatusCode.OK);
        }
#endif

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Import(ImportDto importDto)
        {
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            if (!isHostUser && importDto.PortalId != PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            var controller = new ImportController();
            var operationId = controller.QueueOperation(PortalSettings.UserId, importDto);
            return Request.CreateResponse(HttpStatusCode.OK, new {refId = operationId});
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ProgressStatus(int operationId)
        {
            //TODO: implement
            return Request.CreateResponse(HttpStatusCode.OK, "10%");
        }
    }
}