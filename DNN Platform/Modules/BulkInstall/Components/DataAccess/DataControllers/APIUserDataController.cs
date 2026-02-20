// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.DataAccess.DataControllers
{
    using System.Collections.Generic;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;

    /// <summary>The data controller for <see cref="APIUser"/>.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public sealed class APIUserDataController(IHostSettings hostSettings)
    {
        private readonly IHostSettings hostSettings = hostSettings;

        /// <summary>Creates an <see cref="APIUser"/>.</summary>
        /// <param name="apiUser">The user.</param>
        public void Create(APIUser apiUser)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<APIUser>();

            repo.Insert(apiUser);
        }

        /// <summary>Gets all users.</summary>
        /// <returns>A sequence of <see cref="APIUser"/>.</returns>
        public IEnumerable<APIUser> Get()
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<APIUser>();

            return repo.Get();
        }

        /// <summary>Retrieves a single <see cref="APIUser"/> by its ID.</summary>
        /// <param name="apiUserId">The API user ID.</param>
        /// <returns>The user or <see langword="null"/>.</returns>
        public APIUser Get(int apiUserId)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<APIUser>();

            return repo.GetById<int>(apiUserId);
        }

        /// <summary>Retrieves a single <see cref="APIUser"/> by its API key.</summary>
        /// <param name="apiKey">The API key.</param>
        /// <returns>The user or <see langword="null"/>.</returns>
        public APIUser Get(string apiKey)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            return context.ExecuteSingleOrDefault<APIUser>(System.Data.CommandType.StoredProcedure, "{databaseOwner}[{objectQualifier}BulkInstall_APIUserByAPIKey]", apiKey);
        }

        /// <summary>Updates the passed <see cref="APIUser"/>.</summary>
        /// <param name="apiUser">The new user information.</param>
        public void Update(APIUser apiUser)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<APIUser>();

            repo.Update(apiUser);
        }

        /// <summary>Deletes the passed <see cref="APIUser"/>.</summary>
        /// <param name="apiUser">The user to delete.</param>
        public void Delete(APIUser apiUser)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<APIUser>();

            repo.Delete(apiUser);
        }
    }
}
