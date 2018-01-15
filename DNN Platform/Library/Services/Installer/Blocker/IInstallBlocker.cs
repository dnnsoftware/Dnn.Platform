#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

namespace DotNetNuke.Services.Installer.Blocker
{
    /// <summary>
    /// This interface ...
    /// </summary>
    public interface IInstallBlocker
    {
        void RegisterInstallBegining();
        void RegisterInstallEnd();
        bool IsInstallInProgress();
    }
}
