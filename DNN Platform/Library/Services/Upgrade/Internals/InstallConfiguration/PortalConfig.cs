#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PortalConfig - A class that represents Install/DotNetNuke.Install.Config/Portals/Portal
    /// </summary>
    /// -----------------------------------------------------------------------------    
        
    public class PortalConfig
    {
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
   
        public PortalConfig()
        {
            PortAliases = new List<string>();
        }
    }
}
