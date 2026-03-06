// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;
    using System.Collections.Generic;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Logging;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;

    /// <summary>A deployment created via the API.</summary>
    internal sealed class RemoteDeployment : Deployment
    {
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="RemoteDeployment"/> class.</summary>
        /// <param name="apiUserManager">The API user manager.</param>
        /// <param name="sessionManager">The session manager.</param>
        /// <param name="eventLogManager">The event log manager.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="session">The session.</param>
        /// <param name="ipAddress">The IP address.</param>
        /// <param name="apiTokenId">The API token ID.</param>
        public RemoteDeployment(
            APIUserManager apiUserManager,
            SessionManager sessionManager,
            EventLogManager eventLogManager,
            IEventLogger eventLogger,
            IApplicationStatusInfo appStatus,
            IServiceProvider serviceProvider,
            Session session,
            string ipAddress,
            int apiTokenId)
            : base(sessionManager, eventLogManager, appStatus, serviceProvider, session, ipAddress)
        {
            this.eventLogger = eventLogger;

            // Retrieve our API user.
            this.APIUser = apiUserManager.GetByApiTokenId(apiTokenId);

            // Did we find an API user?
            if (this.APIUser == null)
            {
                throw new InvalidOperationException("API user not found, cannot continue. Shouldn't have been able to get here.");
            }
        }

        private APIUser APIUser { get; set; }

        /// <inheritdoc />
        protected override void LogAnyFailures(List<InstallJob> jobs)
        {
            foreach (InstallJob job in jobs)
            {
                foreach (string failure in job.Failures)
                {
                    string log = $"(IP: {this.IPAddress} | APIUserID: {this.APIUser.APIUserId}) {failure}";

                    this.eventLogger.AddLog("BulkInstall", log, EventLogType.HOST_ALERT);
                }
            }
        }
    }
}
