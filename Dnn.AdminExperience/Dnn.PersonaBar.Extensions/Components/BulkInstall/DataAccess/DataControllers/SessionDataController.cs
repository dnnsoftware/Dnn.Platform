// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.DataControllers
{
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;

    /// <summary>The data controller for <see cref="Session"/>.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public sealed class SessionDataController(IHostSettings hostSettings)
    {
        private readonly IHostSettings hostSettings = hostSettings;

        /// <summary>Gets the <see cref="Session"/> by its <see cref="Session.SessionGuid"/>.</summary>
        /// <param name="sessionGuid">The session GUID.</param>
        /// <returns>The session or <see langword="null"/>.</returns>
        public Session FindByGuid(string sessionGuid)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Session>();

            return repo.Find("WHERE Guid = @0", sessionGuid).FirstOrDefault<Session>();
        }

        /// <summary>Creates the <see cref="Session"/>.</summary>
        /// <param name="session">The session to create.</param>
        public void Create(Session session)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Session>();

            repo.Insert(session);
        }

        /// <summary>Updates a <see cref="Session"/>.</summary>
        /// <param name="session">The new session data.</param>
        public void Update(Session session)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<Session>();

            repo.Update(session);
        }
    }
}
