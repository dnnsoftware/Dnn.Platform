#region Copyright
// DNN® and DotNetNuke® - http://www.DNNSoftware.com
// Copyright ©2002-2019
// by DNN Corp
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
