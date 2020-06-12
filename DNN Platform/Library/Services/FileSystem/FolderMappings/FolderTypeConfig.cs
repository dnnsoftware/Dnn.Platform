

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System.Collections.Generic;

namespace DotNetNuke.Services.FileSystem
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// HostSettingConfig - A class that represents Install/DotNetNuke.Install.Config/Settings.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class FolderTypeConfig
    {
        public string Name { get; set; }

        public string Provider { get; set; }

        public IList<FolderTypeSettingConfig> Settings { get; set; }

        public FolderTypeConfig()
        {
            this.Settings = new List<FolderTypeSettingConfig>();
        }
    }
}
