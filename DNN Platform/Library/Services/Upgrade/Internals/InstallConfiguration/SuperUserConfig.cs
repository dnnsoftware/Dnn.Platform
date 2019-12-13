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
    /// SuperUserConfig - A class that represents Install/DotNetNuke.Install.Config/SuperUser
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
