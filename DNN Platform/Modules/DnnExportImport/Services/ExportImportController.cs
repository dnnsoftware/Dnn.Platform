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
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Components.Dto;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.ExportImport.Services
{
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Host)]
    public class ExportImportController : DnnApiController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Export(ExportDto exportDto)
        {
            var controller = new ExportController();
            var operationId = controller.QueueOperation(exportDto);
            return Request.CreateResponse(HttpStatusCode.OK, new { refId = operationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Import(ImportDto importDto)
        {
            var controller = new ImportController();
            var operationId = controller.QueueOperation(importDto);
            return Request.CreateResponse(HttpStatusCode.OK, new { refId = operationId });
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ProgressStatus(string operationRef)
        {
            //TODO: implement
            return Request.CreateResponse(HttpStatusCode.OK, "10%");
        }
    }
}