#region Usings

using System;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// HostSettingConfig - A class that represents Install/DotNetNuke.Install.Config/Settings
    /// </summary>
    /// -----------------------------------------------------------------------------    

    public class FolderTypeSettingConfig
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Encrypt { get; set; }
    }
}
