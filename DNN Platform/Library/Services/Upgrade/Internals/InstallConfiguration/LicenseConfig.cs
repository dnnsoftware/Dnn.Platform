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
    /// LicenseConfig - A class that represents Install/DotNetNuke.Install.Config/LicenseActivation
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
