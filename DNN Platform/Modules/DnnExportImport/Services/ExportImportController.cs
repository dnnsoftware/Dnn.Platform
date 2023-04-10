// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.ExportImport.Services
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Dto;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    [RequireHost]
    public class ExportImportController : DnnApiController
    {
        private readonly ExportController exportController;
        private readonly ImportController importController;
        private readonly BaseController baseController;

        public ExportImportController(ExportController exportController, ImportController importController, BaseController baseController)
        {
            this.exportController = exportController;
            this.importController = importController;
            this.baseController = baseController;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Export(ExportDto exportDto)
        {
            var jobId = this.exportController.QueueOperation(this.PortalSettings.UserId, exportDto);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Import(ImportDto importDto)
        {
            string message;
            if (this.importController.VerifyImportPackage(importDto.PackageId, null, out message))
            {
                var jobId = this.importController.QueueOperation(this.PortalSettings.UserId, importDto);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { jobId });
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
        }

        [HttpGet]
        public HttpResponseMessage VerifyImportPackage(string packageId)
        {
            string message;
            var summary = new ImportExportSummary();
            var isValid = this.importController.VerifyImportPackage(packageId, summary, out message);
            summary.ConvertToLocal(this.UserInfo);
            return isValid
                ? this.Request.CreateResponse(HttpStatusCode.OK, summary)
                : this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
        }

        /// <summary>Get list of packages to import.</summary>
        /// <param name="keyword">Keyword to search the import package. This will look into the package name and description.</param>
        /// <param name="order">Order by which the packages list should be sorted. Allowed values: newest, oldest, name.</param>
        /// <param name="pageIndex">Page index to get.</param>
        /// <param name="pageSize">Page size. Should not be more than 100.</param>
        /// <returns>An <see cref="HttpResponseMessage"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetImportPackages(string keyword = "", string order = "newest", int pageIndex = 0, int pageSize = 10)
        {
            int total;
            var packages = this.importController.GetImportPackages(out total, keyword, order, pageIndex, pageSize).ToList();
            packages.ForEach(package => package.ConvertToLocal(this.UserInfo));
            return this.Request.CreateResponse(HttpStatusCode.OK, new { packages, total });
        }

        // this is POST so users can't cancel using a simple browser link
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CancelProcess([FromUri] int jobId)
        {
            var cancelStatus = this.baseController.CancelJob(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, jobId);
            return this.Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
        }

        // this is POST so users can't remove a job using a browser link
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveJob([FromUri] int jobId)
        {
            var cancelStatus = this.baseController.RemoveJob(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, jobId);
            return this.Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
        }

        [HttpGet]
        public HttpResponseMessage LastJobTime(int portal, JobType jobType)
        {
            if (!this.UserInfo.IsSuperUser && portal != this.PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            if (portal < 0)
            {
                return this.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    Localization.GetString("InvalidPortal", Constants.SharedResources));
            }

            var lastTime = this.baseController.GetLastJobTime(portal, jobType);
            return this.Request.CreateResponse(
                HttpStatusCode.OK,
                new { lastTime = Util.GetDateTimeString(lastTime) });
        }

        [HttpGet]
        public HttpResponseMessage AllJobs(int portal, int? pageSize = 10, int? pageIndex = 0, int? jobType = null, string keywords = null)
        {
            if (!this.UserInfo.IsSuperUser && portal != this.PortalSettings.PortalId)
            {
                var error = Localization.GetString("NotPortalAdmin", Constants.SharedResources);
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }

            var jobs = this.baseController.GetAllJobs(portal, this.PortalSettings.PortalId, pageSize, pageIndex, jobType, keywords);
            jobs?.ConvertToLocal(this.UserInfo);
            return this.Request.CreateResponse(HttpStatusCode.OK, jobs);
        }

        [HttpGet]
        public HttpResponseMessage JobDetails(int jobId)
        {
            var job = this.baseController.GetJobDetails(this.UserInfo.IsSuperUser ? -1 : this.PortalSettings.PortalId, jobId);
            job?.ConvertToLocal(this.UserInfo);
            return job != null
                ? this.Request.CreateResponse(HttpStatusCode.OK, job)
                : this.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    new { message = Localization.GetString("JobNotExist", Constants.SharedResources) });
        }
    }
}
