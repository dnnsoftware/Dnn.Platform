// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities.Internal;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Installer.Blocker
{
    /// <summary>
    /// This class ...
    /// </summary>
    public class InstallBlocker : ServiceLocator<IInstallBlocker, InstallBlocker>, IInstallBlocker
    {
        #region Constants

        const string installBlockerFile = "\\installBlocker.lock";
        #endregion

        #region Members
        private bool fileCreated = false;
        #endregion

        #region Public Methods

        public void RegisterInstallBegining()
        {          
            if(!fileCreated)  
                File.Create(Globals.ApplicationMapPath + installBlockerFile);
            fileCreated = true;
        }

        public bool IsInstallInProgress()
        {
            return fileCreated || File.Exists(Globals.ApplicationMapPath + installBlockerFile);
        }

        public void RegisterInstallEnd()
        {
            var retryable = new RetryableAction(() =>
            {
                if (IsInstallInProgress() && fileCreated)
                {
                    File.Delete(Globals.ApplicationMapPath + installBlockerFile);
                }
            }, "Deleting lock file", 60, TimeSpan.FromSeconds(1));

            retryable.TryIt();
            fileCreated = false;
        }

        #endregion

        #region Private Methods
        #endregion

        #region Service Locator
        protected override Func<IInstallBlocker> GetFactory()
        {
            return () => new InstallBlocker();
        }
        #endregion
    }
}
