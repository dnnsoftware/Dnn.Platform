// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    using System;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SuperUserConfig - A class that represents Install/DotNetNuke.Install.Config/SuperUser.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class SuperUserConfig
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public string Locale { get; set; }

        public bool UpdatePassword { get; set; }
    }
}
