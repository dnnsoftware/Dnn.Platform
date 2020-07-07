// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Blocker
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities.Internal;
    using DotNetNuke.Framework;

    /// <summary>
    /// This class ...
    /// </summary>
    public class InstallBlocker : ServiceLocator<IInstallBlocker, InstallBlocker>, IInstallBlocker
    {
        private const string installBlockerFile = "\\installBlocker.lock";
        private bool fileCreated = false;

        public void RegisterInstallBegining()
        {
            if (!this.fileCreated)
            {
                File.Create(Globals.ApplicationMapPath + installBlockerFile);
            }

            this.fileCreated = true;
        }

        public bool IsInstallInProgress()
        {
            return this.fileCreated || File.Exists(Globals.ApplicationMapPath + installBlockerFile);
        }

        public void RegisterInstallEnd()
        {
            var retryable = new RetryableAction(
                () =>
            {
                if (this.IsInstallInProgress() && this.fileCreated)
                {
                    File.Delete(Globals.ApplicationMapPath + installBlockerFile);
                }
            }, "Deleting lock file", 60, TimeSpan.FromSeconds(1));

            retryable.TryIt();
            this.fileCreated = false;
        }

        protected override Func<IInstallBlocker> GetFactory()
        {
            return () => new InstallBlocker();
        }
    }
}
