// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Logging;
    using Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.BulkInstall.Encryption;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>A web API controller for use by the Deploy Client.</summary>
    /// <param name="sessionManager">The session manager.</param>
    /// <param name="apiUserManager">The API user manager.</param>
    /// <param name="eventLogManager">The event log manager.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="serviceProvider">The DI container.</param>
    /// <param name="apiTokenController">The API token controller.</param>
    [InWhitelist]
    [ApiTokenAuthorize(APIUserManager.BulkInstallApiTokenScopeKey, "~/DesktopModules/BulkInstall/App_LocalResources/BulkInstall.resx", ApiTokenScope.Host)]
    public class RemoteController(SessionManager sessionManager, APIUserManager apiUserManager, EventLogManager eventLogManager, IEventLogger eventLogger, IApplicationStatusInfo appStatus, IServiceProvider serviceProvider, IApiTokenController apiTokenController) : DnnApiController
    {
        private readonly SessionManager sessionManager = sessionManager;
        private readonly APIUserManager apiUserManager = apiUserManager;
        private readonly EventLogManager eventLogManager = eventLogManager;
        private readonly IEventLogger eventLogger = eventLogger;
        private readonly IApplicationStatusInfo appStatus = appStatus;
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly IApiTokenController apiTokenController = apiTokenController;

        /// <summary>Creates a new session.</summary>
        /// <returns>A response with the <see cref="Session"/>.</returns>
        [HttpGet]
        public HttpResponseMessage CreateSession()
        {
            Session session = this.sessionManager.CreateSession();

            return this.Request.CreateResponse(HttpStatusCode.OK, session);
        }

        /// <summary>Gets a session by its <paramref name="sessionGuid"/>.</summary>
        /// <param name="sessionGuid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response with either the <see cref="Session"/> or <see langword="null"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetSession(string sessionGuid)
        {
            Session session = this.sessionManager.GetSession(sessionGuid);

            return this.Request.CreateResponse(HttpStatusCode.OK, session);
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

                // Get the api key from the header.
                var apiKey = this.apiTokenController.GetCurrentThreadApiToken();

                // Get the api user.
                APIUser apiUser = this.apiUserManager.FindAndPrepare(apiKey.ApiTokenId, this.Request.Headers.Authorization.Parameter);

                // Receive files.
                MultipartMemoryStreamProvider provider = await this.Request.Content.ReadAsMultipartAsync();

                foreach (HttpContent file in provider.Contents)
                {
                    string filename = file.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);

                    using MemoryStream ms = new MemoryStream(await file.ReadAsByteArrayAsync());
                    using Stream ds = Crypto.Decrypt(ms, apiUser.EncryptionKey);
                    this.sessionManager.AddPackage(sessionGuid, ds, filename);
                }
            }
            catch (Exception ex)
            {
                this.eventLogManager.Log("REMOTE_EXCEPTION", EventLogSeverity.Warning, ex);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return this.Request.CreateResponse(HttpStatusCode.Created);
        }

        /// <summary>Starts the installation for the session.</summary>
        /// <param name="sessionGuid">The public identifier for the <see cref="Session"/>.</param>
        /// <returns>A response indicating success.</returns>
        [HttpGet]
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

                // Get the api key from the header.
                var apiKey = this.apiTokenController.GetCurrentThreadApiToken();

                // Get the session.
                Session sessionObj = this.sessionManager.GetSession(sessionGuid);

                // Create a deployment operation.
                RemoteDeployment deployOperation = new RemoteDeployment(this.apiUserManager, this.sessionManager, this.eventLogManager, this.eventLogger, this.appStatus, this.serviceProvider, sessionObj, ipAddress, apiKey.ApiTokenId);

                // Deploy.
                deployOperation.Deploy();
            }
            catch (Exception ex)
            {
                this.eventLogManager.Log("REMOTE_EXCEPTION", EventLogSeverity.Warning, ex);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, "Operation started.");
        }
    }
}
