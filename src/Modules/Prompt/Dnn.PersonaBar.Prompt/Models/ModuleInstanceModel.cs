using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.PersonaBar.Prompt.Models
{
    /// <summary>
    /// Similar to ModuleInfoModel, however this one has information more specific to a module's 
    /// implementation on a particular page (like the PaneName)
    /// </summary>
    public class ModuleInstanceModel
    {
        // command link
        public string __ModuleId;
        public int ModuleId;
        public string Title;
        public string Pane;
        public string ModuleName;
        // command link
        public string __ModuleName;
        public string FriendlyName;
        public int ModuleDefId;
        public int TabModuleId;
        public bool IsDeleted;

        public int TabId;
        public static ModuleInstanceModel FromDnnModuleInfo(DotNetNuke.Entities.Modules.ModuleInfo dnnModule)
        {
            ModuleInstanceModel mim = new ModuleInstanceModel
            {
                ModuleId = dnnModule.ModuleID,
                Title = dnnModule.ModuleTitle,
                Pane = dnnModule.PaneName,
                FriendlyName = dnnModule.DesktopModule.FriendlyName,
                ModuleName = dnnModule.DesktopModule.ModuleName,
                TabModuleId = dnnModule.TabModuleID,
                ModuleDefId = dnnModule.ModuleDefID,
                IsDeleted = dnnModule.IsDeleted,
                TabId = dnnModule.TabID
            };
            mim.__ModuleId = $"get-module {dnnModule.ModuleID}";
            mim.__ModuleName = $"list-modules '{dnnModule.ModuleID}'";

            return mim;
        }
    }
}