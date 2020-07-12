// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    using System;
    using System.Collections.Generic;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PortalConfig - A class that represents Install/DotNetNuke.Install.Config/Portals/Portal.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class PortalConfig
    {
        public PortalConfig()
        {
            this.PortAliases = new List<string>();
        }

        public string PortalName { get; set; }

        public string AdminFirstName { get; set; }

        public string AdminLastName { get; set; }

        public string AdminUserName { get; set; }

        public string AdminPassword { get; set; }

        public string AdminEmail { get; set; }

        public string Description { get; set; }

        public string Keywords { get; set; }

        public string TemplateFileName { get; set; }

        public bool IsChild { get; set; }

        public string HomeDirectory { get; set; }

        public IList<string> PortAliases { get; set; }
    }
}
