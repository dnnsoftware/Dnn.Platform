// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    using System;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ConnectionConfig - A class that represents Install/DotNetNuke.Install.Config/Connection.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class ConnectionConfig
    {
        public string Server { get; set; }

        public string Database { get; set; }

        public string File { get; set; }

        public bool Integrated { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public bool RunAsDbowner { get; set; }

        public string Qualifier { get; set; }

        public string UpgradeConnectionString { get; set; }
    }
}
