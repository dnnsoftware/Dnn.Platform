using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Web.Models
{
    [Serializable]
    public class TabModule
    {
        public TabInfo TabInfo { get; set; }

        public ModuleInfo ModuleInfo { get; set; }

        public string ModuleVersion { get; set; }
    }
}