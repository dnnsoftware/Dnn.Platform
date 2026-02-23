// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters;

    using DotNetNuke.Web.Api;

    /// <summary>A web API controller for <see cref="APIUser"/>.</summary>
    /// <param name="apiUserManager">The API user manager.</param>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class APIUserController(APIUserManager apiUserManager) : DnnApiController
    {
        private readonly APIUserManager apiUserManager = apiUserManager;

        /// <summary>Gets all <see cref="APIUser"/> instances.</summary>
        /// <returns>A response with a list of <see cref="APIUser"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            List<APIUser> apiUsers = this.apiUserManager.GetAll().ToList();

            // Loop and remove sensitive information.
            foreach (APIUser apiUser in apiUsers)
            {
                apiUser.EncryptedEncryptionKey = null;
                apiUser.Salt = null;
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Users = apiUsers, });
        }

        /// <summary>Creates a new <see cref="APIUser"/>.</summary>
        /// <param name="name">The label.</param>
        /// <param name="bypass">Whether the user can bypass the IP address allow list.</param>
        /// <param name="expiresOn">The date/time on which the API user's token expires.</param>
        /// <returns>A response with the new <see cref="APIUser"/>.</returns>
        [HttpPost]
        public HttpResponseMessage Create(string name, bool bypass = false, DateTime? expiresOn = null)
        {
            // Check we have a name.
            if (string.IsNullOrEmpty(name))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            // Create user.
            APIUser apiUser = this.apiUserManager.Create(name, bypass, expiresOn ?? DateTime.UtcNow.AddYears(1), this.UserInfo.UserID);

            apiUser.EncryptedEncryptionKey = null;
            apiUser.Salt = null;

            return this.Request.CreateResponse(HttpStatusCode.Created, new { User = apiUser, });
        }

        /// <summary>Deletes an <see cref="APIUser"/>.</summary>
        /// <param name="id">The API user ID.</param>
        /// <returns>A response indicating success.</returns>
        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            APIUser apiUser = this.apiUserManager.GetById(id);

            if (apiUser == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, "API user not found.");
            }

            try
            {
                this.apiUserManager.Delete(apiUser);
            }
            catch (Exception)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Failed to delete API user.");
            }

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
