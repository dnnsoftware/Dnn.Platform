// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// HostSettingConfig - A class that represents Install/DotNetNuke.Install.Config/Settings.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class FolderTypeSettingConfig
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public bool Encrypt { get; set; }
    }
}
