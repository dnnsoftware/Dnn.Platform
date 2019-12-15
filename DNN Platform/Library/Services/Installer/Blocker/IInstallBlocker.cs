// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
