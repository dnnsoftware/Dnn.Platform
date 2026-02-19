// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
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
    using DotNetNuke.Web.Api;

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

            return this.Request.CreateResponse(HttpStatusCode.OK, session);
        }

        /// <summary>Gets a session by its <paramref name="guid"/>.</summary>
        /// <param name="guid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response with either the <see cref="Session"/> or <see langword="null"/>.</returns>
        [HttpGet]
#pragma warning disable CA1720 // Identifier contains type name
        public HttpResponseMessage Get(string guid)
#pragma warning restore CA1720 // Identifier contains type name
        {
            var session = this.sessionManager.GetSession(guid);

            return this.Request.CreateResponse(HttpStatusCode.OK, session);
        }

        /// <summary>Adds a package file to the session.</summary>
        /// <param name="guid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response indicating success.</returns>
        /// <exception cref="HttpResponseException">The request is not a multipart MIME type.</exception>
        [HttpPost]
#pragma warning disable CA1720 // Identifier contains type name
        public async Task<HttpResponseMessage> AddPackage(string guid)
#pragma warning restore CA1720 // Identifier contains type name
        {
            if (!this.sessionManager.SessionExists(guid))
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

                // TODO: Add filtering so that non .zip archives are not added.
                foreach (HttpContent file in provider.Contents)
                {
                    string filename = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

                    using MemoryStream ms = new MemoryStream(await file.ReadAsByteArrayAsync());
                    this.sessionManager.AddPackage(guid, ms, filename);
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
        /// <param name="guid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response with a sorted list of <see cref="InstallJob"/>.</returns>
        [HttpGet]
#pragma warning disable CA1720 // Identifier contains type name
        public HttpResponseMessage Summary(string guid)
#pragma warning restore CA1720 // Identifier contains type name
        {
            if (!this.sessionManager.SessionExists(guid))
            {
                // Session doesn't exist.
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the session.
                Session sessionObj = this.sessionManager.GetSession(guid);

                // Create a deploy operation.
                Deployment deployOperation = new Deployment(this.sessionManager, this.eventLogManager, this.appStatus, sessionObj, ipAddress);

                var summary = deployOperation.Summary();

                return this.Request.CreateResponse(HttpStatusCode.OK, summary);
            }
            catch (Exception ex)
            {
                this.eventLogManager.Log("SESSION_EXCEPTION", EventLogSeverity.Warning, ex);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        /// <summary>Starts the installation for the session.</summary>
        /// <param name="guid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response indicating success.</returns>
        [HttpGet]
#pragma warning disable CA1720 // Identifier contains type name
        public HttpResponseMessage Install(string guid)
#pragma warning restore CA1720 // Identifier contains type name
        {
            if (!this.sessionManager.SessionExists(guid))
            {
                // Session doesn't exist.
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Invalid session.");
            }

            try
            {
                // Get the users ip address.
                string ipAddress = HttpContext.Current.Request.UserHostAddress;

                // Get the session.
                Session sessionObj = this.sessionManager.GetSession(guid);

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
    }
}
