#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
        public InstallConfig()
        {
            Portals = new List<PortalConfig>();
            Scripts = new List<string>();
            Settings = new List<HostSettingConfig>();
        }
    }


}
