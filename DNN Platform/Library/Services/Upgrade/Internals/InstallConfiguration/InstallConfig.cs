#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Upgrade.Internals.InstallConfiguration
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallConfig - A class that represents DotNetNuke.Install.Config XML configuration file
    /// TODO This class may not capture all the details from the config file
    /// </summary>
    /// -----------------------------------------------------------------------------    
        
    public class InstallConfig
    {
        public IList<string> Scripts { get; set; }
        public string Version { get; set; }
        public string InstallCulture { get; set; }
        public SuperUserConfig SuperUser { get; set; }
        public ConnectionConfig Connection { get; set; }
        public LicenseConfig License { get; set; }
        public IList<PortalConfig> Portals { get; set; }
        public IList<HostSettingConfig> Settings { get; set; }

        public string FolderMappingsSettings { get; set; } 

        public bool SupportLocalization { get; set; }

        public InstallConfig()
        {
            Portals = new List<PortalConfig>();
            Scripts = new List<string>();
            Settings = new List<HostSettingConfig>();
        }
    }


}
