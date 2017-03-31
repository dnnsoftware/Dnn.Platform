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

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Components.Dto;
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
            var isHostUser = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
            if (!isHostUser && exportDto.PortalId != PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            var controller = new ExportController();
            var jobId = controller.QueueOperation(PortalSettings.UserId, exportDto);

            return Request.CreateResponse(HttpStatusCode.OK, new { jobId });
        }

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
            string message;
            if (controller.VerifyImportPackage(importDto.PackageId, null, out message))
            {
                var jobId = controller.QueueOperation(PortalSettings.UserId, importDto);
                return Request.CreateResponse(HttpStatusCode.OK, new { jobId });
            }
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
        }

        [HttpGet]
        public HttpResponseMessage VerifyImportPackage(string packageId)
        {
            var controller = new ImportController();
            string message;
            var summary = new ImportExportSummary();
            var isValid = controller.VerifyImportPackage(packageId, summary, out message);
            return isValid
                ? Request.CreateResponse(HttpStatusCode.OK, summary)
                : Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
        }

        /// <summary>
        /// Get list of packages to import
        /// </summary>
        /// <param name="keyword">Keyword to search the import package. This will look into the package name and description</param>
        /// <param name="order">Order by which the packages list should be sorted. Allowed values: newest, oldest, name</param>
        /// <param name="pageIndex">Page index to get</param>
        /// <param name="pageSize">Page size. Should not be more than 100.</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetImportPackages(string keyword = "", string order = "newest", int pageIndex = 0,
            int pageSize = 10)
        {
            var controller = new ImportController();
            var packages = controller.GetImportPackages(keyword, order, pageIndex, pageSize).ToList();
            return Request.CreateResponse(HttpStatusCode.OK, packages);
        }

        // this is POST so users can't cancel using a simple browser link
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CancelProcess([FromUri] int jobId)
        {
            var controller = new BaseController();
            var cancelStatus = controller.CancelJob(PortalSettings.PortalId, jobId);
            return Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
        }

        // this is POST so users can't remove a job using a browser link
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveJob([FromUri] int jobId)
        {
            var controller = new BaseController();
            var cancelStatus = controller.RemoveJob(PortalSettings.PortalId, jobId);
            return Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
        }

        [HttpGet]
        public HttpResponseMessage ProgressStatus(int jobId)
        {
            //TODO: implement
            return Request.CreateResponse(HttpStatusCode.OK, new { percentage = "10%" });
        }

        [HttpGet]
        public HttpResponseMessage LastJobTime(int portal, JobType jobType)
        {
            if (!UserInfo.IsSuperUser && portal != PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            if (portal < 0)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    Localization.GetString("InvalidPortal", Constants.SharedResources));

            var controller = new BaseController();
            var lastTime = controller.GetLastJobTime(portal, jobType);
            return Request.CreateResponse(HttpStatusCode.OK, new { lastTime });
        }

        [HttpGet]
        public HttpResponseMessage AllJobs(int portal, int? pageSize = 10, int? pageIndex = 0, int? jobType = null,
            string keywords = null)
        {
            if (!UserInfo.IsSuperUser && portal != PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            var controller = new BaseController();
            var jobs = controller.GetAllJobs(portal, PortalSettings.PortalId, pageSize, pageIndex, jobType, keywords);
            return Request.CreateResponse(HttpStatusCode.OK, jobs);
        }

        [HttpGet]
        public HttpResponseMessage JobDetails(int jobId)
        {
            var controller = new BaseController();
            var job = controller.GetJobDetails(UserInfo.IsSuperUser ? -1 : PortalSettings.PortalId, jobId);
            return job != null
                ? Request.CreateResponse(HttpStatusCode.OK, job)
                : Request.CreateResponse(HttpStatusCode.BadRequest,
                    new { message = Localization.GetString("JobNotExist", Constants.SharedResources) });
        }
    }
}