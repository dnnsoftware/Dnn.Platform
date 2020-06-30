// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    using System;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// LicenseConfig - A class that represents Install/DotNetNuke.Install.Config/LicenseActivation.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class LicenseConfig
    {
        public string AccountEmail { get; set; }

        public string InvoiceNumber { get; set; }

        public string WebServer { get; set; }

        public string LicenseType { get; set; }

        public bool TrialRequest { get; set; }
    }
}
