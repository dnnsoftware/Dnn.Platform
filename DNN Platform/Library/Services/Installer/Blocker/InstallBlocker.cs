#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2015
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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
        #endregion

        #region Public Methods

        public void RegisterInstallBegining()
        {
            File.Create(Globals.ApplicationMapPath + installBlockerFile);
        }

        public bool IsInstallInProgress()
        {
            return File.Exists(Globals.ApplicationMapPath + installBlockerFile);
        }

        public void RegisterInstallEnd()
        {
            var retryable = new RetryableAction(() =>
            {
                if (IsInstallInProgress())
                {
                    File.Delete(Globals.ApplicationMapPath + installBlockerFile);
                }
            }, "Deleting lock file", 60, TimeSpan.FromSeconds(1));

            retryable.TryIt();
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
