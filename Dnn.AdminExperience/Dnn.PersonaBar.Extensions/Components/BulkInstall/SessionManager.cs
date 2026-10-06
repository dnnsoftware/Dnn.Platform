// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall
{
    using System;
    using System.IO;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.DataControllers;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;
    using DotNetNuke.Abstractions.Application;

    /// <summary>The <see cref="Session"/> manager.</summary>
    /// <param name="dataController">The data controller.</param>
    /// <param name="appStatus">The application status.</param>
    public sealed class SessionManager(SessionDataController dataController, IApplicationStatusInfo appStatus)
    {
        private readonly IApplicationStatusInfo appStatus = appStatus;
        private readonly SessionDataController dataController = dataController;

        /// <summary>Creates a session.</summary>
        /// <returns>A new <see cref="Session"/> instance.</returns>
        public Session CreateSession()
        {
            string directory = new DirectoryInfo(this.AvailableSessionDirectory()).Name;

            Session session = new Session(directory);

            this.dataController.Create(session);

            return session;
        }

        /// <summary>Gets the <see cref="Session"/> by its <see cref="Session.SessionGuid"/>.</summary>
        /// <param name="sessionGuid">The session GUID.</param>
        /// <returns>The session or <see langword="null"/>.</returns>
        public Session GetSession(string sessionGuid)
        {
            return this.dataController.FindByGuid(sessionGuid);
        }

        /// <summary>Updates a <see cref="Session"/>.</summary>
        /// <param name="session">The new session data.</param>
        public void UpdateSession(Session session)
        {
            this.dataController.Update(session);
        }

        /// <summary>Gets a value indicating whether a <see cref="Session"/> exists with the specified <paramref name="sessionGuid"/>.</summary>
        /// <param name="sessionGuid">The session GUID.</param>
        /// <returns><see langword="true"/> if the session exists, otherwise <see langword="false"/>.</returns>
        public bool SessionExists(string sessionGuid)
        {
            Session session = this.dataController.FindByGuid(sessionGuid);

            if (session == null)
            {
                return false;
            }

            session.LastUsed = DateTime.Now;

            this.dataController.Update(session);

            return true;
        }

        /// <summary>Adds a package to a session's folder.</summary>
        /// <param name="sessionGuid">The session GUID.</param>
        /// <param name="packageStream">A stream of the package file.</param>
        /// <param name="filename">The package's file name.</param>
        /// <exception cref="ArgumentException">No session exists with the provided <paramref name="sessionGuid"/>.</exception>
        public void AddPackage(string sessionGuid, Stream packageStream, string filename)
        {
            Session session = this.dataController.FindByGuid(sessionGuid);

            if (session == null)
            {
                throw new ArgumentException($"No session exists with guid: {sessionGuid}", nameof(sessionGuid));
            }

            using FileStream fs = File.Create(Path.Combine(Utilities.GetModuleBasePath(this.appStatus), "Sessions", session.SessionGuid, filename));
            packageStream.CopyTo(fs);
        }

        /// <summary>Gets the path to the session folder.</summary>
        /// <param name="sessionGuid">The session GUID.</param>
        /// <returns>Absolute/mapped path to the session folder.</returns>
        /// <exception cref="ArgumentException">No session exists with the provided <paramref name="sessionGuid"/>.</exception>
        public string PathForSession(string sessionGuid)
        {
            Session session = this.dataController.FindByGuid(sessionGuid);

            if (session == null)
            {
                throw new ArgumentException($"No session exists with guid: {sessionGuid}", nameof(sessionGuid));
            }

            return Path.Combine(Utilities.GetModuleBasePath(this.appStatus), "Sessions", session.SessionGuid);
        }

        private string AvailableSessionDirectory()
        {
            string basePath = Path.Combine(Utilities.GetModuleBasePath(this.appStatus), "Sessions");
            Directory.CreateDirectory(basePath);

            // Generate a random folder in the desired path.
            string dir = Path.Combine(basePath, Utilities.RandomName());

            // Does it already exist? I doubt it.
            if (Directory.Exists(dir))
            {
                // My mistake, try again!
                return this.AvailableSessionDirectory();
            }

            // Create the folder.
            Directory.CreateDirectory(dir);

            return dir;
        }
    }
}
