﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Controllers;
using Dnn.ExportImport.Components.Dto;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace Dnn.ExportImport.Services
{
    [RequireHost]
    public class ExportImportController : DnnApiController
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Export(ExportDto exportDto)
        {
            var controller = new ExportController();
            var jobId = controller.QueueOperation(PortalSettings.UserId, exportDto);

            return Request.CreateResponse(HttpStatusCode.OK, new { jobId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Import(ImportDto importDto)
        {
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
            summary.ConvertToLocal(UserInfo);
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
            int total;
            var packages = controller.GetImportPackages(out total, keyword, order, pageIndex, pageSize).ToList();
            packages.ForEach(package => package.ConvertToLocal(UserInfo));
            return Request.CreateResponse(HttpStatusCode.OK, new { packages, total });
        }

        // this is POST so users can't cancel using a simple browser link
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CancelProcess([FromUri] int jobId)
        {
            var controller = new BaseController();
            var cancelStatus = controller.CancelJob(UserInfo.IsSuperUser ? -1 : PortalSettings.PortalId, jobId);
            return Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
        }

        // this is POST so users can't remove a job using a browser link
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RemoveJob([FromUri] int jobId)
        {
            var controller = new BaseController();
            var cancelStatus = controller.RemoveJob(UserInfo.IsSuperUser ? -1 : PortalSettings.PortalId, jobId);
            return Request.CreateResponse(
                cancelStatus ? HttpStatusCode.OK : HttpStatusCode.BadRequest, new { success = cancelStatus });
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
            return Request.CreateResponse(HttpStatusCode.OK,
                new { lastTime = Util.GetDateTimeString(lastTime) });
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
            jobs?.ConvertToLocal(UserInfo);
            return Request.CreateResponse(HttpStatusCode.OK, jobs);
        }

        [HttpGet]
        public HttpResponseMessage JobDetails(int jobId)
        {
            var controller = new BaseController();
            var job = controller.GetJobDetails(UserInfo.IsSuperUser ? -1 : PortalSettings.PortalId, jobId);
            job?.ConvertToLocal(UserInfo);
            return job != null
                ? Request.CreateResponse(HttpStatusCode.OK, job)
                : Request.CreateResponse(HttpStatusCode.BadRequest,
                    new { message = Localization.GetString("JobNotExist", Constants.SharedResources) });
        }
    }
}
