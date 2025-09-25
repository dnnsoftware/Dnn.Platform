namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules;

    public class ModuleActionsModel
    {
        // public ModuleInstanceContext ModuleContext { get; internal set; }
        public ModuleInfo ModuleContext { get; set; }

        public bool SupportsQuickSettings { get; set; }

        public bool DisplayQuickSettings { get; set; }

        public object QuickSettingsModel { get; set; }

        public string CustomActionsJSON { get; set; }

        public string AdminActionsJSON { get; set; }

        public string Panes { get; set; }

        public string CustomText { get; set; }

        public string AdminText { get; set; }

        public string MoveText { get; set; }

        public bool SupportsMove { get; set; }

        public bool IsShared { get; set; }

        public string ModuleTitle { get; set; }

        public Dictionary<string, string> ActionScripts { get; set; }
    }
}
