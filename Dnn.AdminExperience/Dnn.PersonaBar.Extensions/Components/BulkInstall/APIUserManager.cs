// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.DataControllers;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>Manager for <see cref="APIUser"/>.</summary>
    /// <param name="dataController">The data controller.</param>
    /// <param name="apiTokenController">The API Token controller.</param>
    public sealed class APIUserManager(APIUserDataController dataController, IApiTokenController apiTokenController)
    {
        /// <summary>The key used to identify the Bulk Install APIs for API Token auth.</summary>
        public const string BulkInstallApiTokenScopeKey = "bulk-install";

        private readonly APIUserDataController dataController = dataController;
        private readonly IApiTokenController apiTokenController = apiTokenController;

        /// <summary>Creates a new <see cref="APIUser"/> with the specified <paramref name="name"/>.</summary>
        /// <param name="name">A label for the user.</param>
        /// <param name="bypass">Whether the user can bypass the IP allow list.</param>
        /// <param name="expiresOn">The date/time on which the API token expires.</param>
        /// <param name="createdByUserId">The ID of the user creating the <see cref="APIUser"/>.</param>
        /// <returns>The user.</returns>
        public APIUser Create(string name, bool bypass, DateTime expiresOn, int createdByUserId)
        {
            APIUser newApiUser;

            try
            {
                var tokenName = $"Bulk Install: {name}";
                var apiToken = this.apiTokenController.CreateApiToken(Null.NullInteger, tokenName, ApiTokenScope.Host, expiresOn, BulkInstallApiTokenScopeKey, createdByUserId);
                var token = this.apiTokenController.GetApiTokens(ApiTokenScope.Host, false, Null.NullInteger, createdByUserId, ApiTokenFilter.Active, BulkInstallApiTokenScopeKey, 0, 1).First();
                newApiUser = new APIUser(name, bypass, apiToken, token.ApiTokenId);

                this.dataController.Create(newApiUser);
            }
            catch (Exception)
            {
                return null;
            }

            return newApiUser;
        }

        /// <summary>Gets all <see cref="APIUser"/> instances.</summary>
        /// <returns>A sequence of <see cref="APIUser"/>.</returns>
        public IEnumerable<APIUser> GetAll()
        {
            return this.dataController.Get();
        }

        /// <summary>Retrieves a single <see cref="APIUser"/> by its ID.</summary>
        /// <param name="id">The API user ID.</param>
        /// <returns>The user or <see langword="null"/>.</returns>
        public APIUser GetById(int id)
        {
            return this.dataController.Get(id);
        }

        /// <summary>Retrieves a single <see cref="APIUser"/> by its API key.</summary>
        /// <param name="apiTokenId">The API key.</param>
        /// <returns>The user or <see langword="null"/>.</returns>
        public APIUser GetByApiTokenId(int apiTokenId)
        {
            return this.dataController.GetByApiTokenId(apiTokenId);
        }

        /// <summary>Updates the passed <see cref="APIUser"/>.</summary>
        /// <param name="apiUser">The new user information.</param>
        /// <returns>The updated user.</returns>
        public APIUser Update(APIUser apiUser)
        {
            this.dataController.Update(apiUser);

            return this.dataController.Get(apiUser.APIUserId);
        }

        /// <summary>Deletes the passed <see cref="APIUser"/>.</summary>
        /// <param name="apiUser">The user to delete.</param>
        public void Delete(APIUser apiUser)
        {
            this.dataController.Delete(apiUser);
        }

        /// <summary>Looks up an <see cref="APIUser"/> by its API key and prepares it for use.</summary>
        /// <param name="apiTokenId">The API token ID.</param>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The user of <see langword="null"/>.</returns>
        public APIUser FindAndPrepare(int apiTokenId, string apiKey)
        {
            // Lookup user by api key.
            APIUser apiUser = this.dataController.GetByApiTokenId(apiTokenId);

            // Verify and prepare for use.
            if (apiUser != null && apiUser.PrepareForUse(apiKey))
            {
                // Return api user.
                return apiUser;
            }

            // Didn't find api user or preparation failed.
            return null;
        }
    }
}
