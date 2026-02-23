// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Logging;
    using Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Web.Api;

    using Newtonsoft.Json;

    /// <summary>A web API controller for <see cref="Session"/>.</summary>
    /// <param name="sessionManager">The session manager.</param>
    /// <param name="eventLogManager">The event log manager.</param>
    /// <param name="appStatus">The application status.</param>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class SessionController(SessionManager sessionManager, EventLogManager eventLogManager, IApplicationStatusInfo appStatus) : DnnApiController
    {
        private readonly SessionManager sessionManager = sessionManager;
        private readonly EventLogManager eventLogManager = eventLogManager;
        private readonly IApplicationStatusInfo appStatus = appStatus;

        /// <summary>Create a new session.</summary>
        /// <returns>A response with the <see cref="Session"/>.</returns>
        [HttpPost]
        public HttpResponseMessage Create()
        {
            var session = this.sessionManager.CreateSession();

            var responseBody = new
            {
                Session = new SessionDto(session),
                MaxUploadFileSize = Config.GetMaxUploadSize(this.appStatus),
            };
            return this.Request.CreateResponse(HttpStatusCode.OK, responseBody);
        }

        /// <summary>Gets a session by its <paramref name="sessionGuid"/>.</summary>
        /// <param name="sessionGuid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response with either the <see cref="Session"/> or <see langword="null"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Get(string sessionGuid)
        {
            var session = this.sessionManager.GetSession(sessionGuid);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Session = new SessionDto(session), });
        }

        /// <summary>Adds package files to the session.</summary>
        /// <param name="sessionGuid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response indicating success.</returns>
        /// <exception cref="HttpResponseException">The request is not a multipart MIME type.</exception>
        [HttpPost]
        public async Task<HttpResponseMessage> AddPackages(string sessionGuid)
        {
            if (!this.sessionManager.SessionExists(sessionGuid))
            {
                // Session doesn't exist.
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Does the request contain multipart/form-data?
                if (!this.Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                // Receive files.
                MultipartMemoryStreamProvider provider = await this.Request.Content.ReadAsMultipartAsync();

                foreach (HttpContent file in provider.Contents)
                {
                    string filename = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                    if (!string.Equals(Path.GetExtension(filename), ".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        this.eventLogManager.Log("INVALID_PACKAGE", EventLogSeverity.Warning, $"Attempted to upload {filename}, only zip archives are allowed");
                        return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Package files must be zip archives");
                    }

                    using MemoryStream ms = new MemoryStream(await file.ReadAsByteArrayAsync());
                    this.sessionManager.AddPackage(sessionGuid, ms, filename);
                }
            }
            catch (Exception ex)
            {
                this.eventLogManager.Log("SESSION_EXCEPTION", EventLogSeverity.Warning, ex);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return this.Request.CreateResponse(HttpStatusCode.Created);
        }

        /// <summary>Gets the summary of a session.</summary>
        /// <param name="sessionGuid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response with a sorted list of <see cref="InstallJob"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Summary(string sessionGuid)
        {
            if (!this.sessionManager.SessionExists(sessionGuid))
            {
                // Session doesn't exist.
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the session.
                Session sessionObj = this.sessionManager.GetSession(sessionGuid);

                // Create a deploy operation.
                Deployment deployOperation = new Deployment(this.sessionManager, this.eventLogManager, this.appStatus, sessionObj, ipAddress);

                var summary = deployOperation.Summary();

                return this.Request.CreateResponse(HttpStatusCode.OK, new { InstallJobs = summary.Values, });
            }
            catch (Exception ex)
            {
                this.eventLogManager.Log("SESSION_EXCEPTION", EventLogSeverity.Warning, ex);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>Starts the installation for the session.</summary>
        /// <param name="sessionGuid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        public HttpResponseMessage Install(string sessionGuid)
        {
            if (!this.sessionManager.SessionExists(sessionGuid))
            {
                // Session doesn't exist.
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the session.
                Session sessionObj = this.sessionManager.GetSession(sessionGuid);

                // Create a deploy operation.
                Deployment deployOperation = new Deployment(this.sessionManager, this.eventLogManager, this.appStatus, sessionObj, ipAddress);

                deployOperation.Deploy();

                return this.Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                this.eventLogManager.Log("SESSION_EXCEPTION", EventLogSeverity.Warning, ex);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        private sealed class SessionDto(Session session)
        {
            public DateTime LastUsed => session.LastUsed;

            public string SessionGuid => session.SessionGuid;

            public SessionStatus Status => session.Status;

            public IList<InstallJob> Response =>
                string.IsNullOrWhiteSpace(session.Response)
                    ? Array.Empty<InstallJob>()
                    : JsonConvert.DeserializeObject<SortedList<string, InstallJob>>(session.Response).Values;
        }
    }
}
