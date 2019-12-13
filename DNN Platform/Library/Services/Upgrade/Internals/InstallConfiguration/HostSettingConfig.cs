// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// HostSettingConfig - A class that represents Install/DotNetNuke.Install.Config/Settings
    /// </summary>
    /// -----------------------------------------------------------------------------    

    public class HostSettingConfig
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsSecure { get; set; }
    }
}
