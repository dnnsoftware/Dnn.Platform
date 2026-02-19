// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;
    using System.Collections.Generic;

    using Dnn.Modules.BulkInstall.Components.DataAccess.DataControllers;
    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;

    /// <summary>Manager for <see cref="APIUser"/>.</summary>
    /// <param name="dataController">The data controller.</param>
    public sealed class APIUserManager(APIUserDataController dataController)
    {
        private readonly APIUserDataController dataController = dataController;

        /// <summary>Creates a new APIUser with the passed name.</summary>
        /// <param name="name">A label for the user.</param>
        /// <returns>The user.</returns>
        public APIUser Create(string name)
        {
            return this.Create(name, bypass: false);
        }

        /// <summary>Creates a new APIUser with the passed name.</summary>
        /// <param name="name">A label for the user.</param>
        /// <param name="bypass">Whether the user can bypass the IP allow list.</param>
        /// <returns>The user.</returns>
        public APIUser Create(string name, bool bypass)
        {
            APIUser newApiUser;

            try
            {
                newApiUser = new APIUser(name, bypass);

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
        /// <param name="apiKey">The API key.</param>
        /// <returns>The user or <see langword="null"/>.</returns>
        public APIUser GetByAPIKey(string apiKey)
        {
            return this.dataController.Get(apiKey);
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
        /// <param name="apiKey">The API key.</param>
        /// <returns>The user of <see langword="null"/>.</returns>
        public APIUser FindAndPrepare(string apiKey)
        {
            // Lookup user by api key.
            APIUser apiUser = this.dataController.Get(apiKey);

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
