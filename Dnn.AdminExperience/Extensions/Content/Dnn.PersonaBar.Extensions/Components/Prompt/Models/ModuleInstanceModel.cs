namespace Dnn.PersonaBar.Prompt.Components.Models
{
    /// <summary>
    /// Similar to ModuleInfoModel, however this one has information more specific to a module's 
    /// implementation on a particular page (like the PaneName)
    /// </summary>
    public class ModuleInstanceModel
    {
        // command link
        public string __ModuleId { get; set; }
        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string Pane { get; set; }
        public string ModuleName { get; set; }
        // command link
        public string __ModuleName { get; set; }
        public string FriendlyName { get; set; }
        public int ModuleDefId { get; set; }
        public int TabModuleId { get; set; }
        public bool IsDeleted { get; set; }

        public int TabId;
        public static ModuleInstanceModel FromDnnModuleInfo(DotNetNuke.Entities.Modules.ModuleInfo dnnModule)
        {
            var mim = new ModuleInstanceModel
            {
                ModuleId = dnnModule.ModuleID,
                Title = dnnModule.ModuleTitle,
                Pane = dnnModule.PaneName,
                FriendlyName = dnnModule.DesktopModule.FriendlyName,
                ModuleName = dnnModule.DesktopModule.ModuleName,
                TabModuleId = dnnModule.TabModuleID,
                ModuleDefId = dnnModule.ModuleDefID,
                IsDeleted = dnnModule.IsDeleted,
                TabId = dnnModule.TabID,
                __ModuleId = $"get-module {dnnModule.ModuleID}",
                __ModuleName = $"list-modules '{dnnModule.ModuleID}'"
            };

            return mim;
        }
    }
}