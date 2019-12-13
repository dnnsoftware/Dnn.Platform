#region Usings

using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// HostSettingConfig - A class that represents Install/DotNetNuke.Install.Config/Settings
    /// </summary>
    /// -----------------------------------------------------------------------------    

    public class FolderTypeConfig
    {
        public string Name { get; set; }
        public string Provider { get; set; }
        public IList<FolderTypeSettingConfig> Settings { get; set; }

        public FolderTypeConfig()
        {
            Settings = new List<FolderTypeSettingConfig>();
        }
    }
}
