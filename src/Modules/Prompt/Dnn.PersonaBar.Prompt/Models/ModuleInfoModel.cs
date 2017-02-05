using System.Text;

namespace Dnn.PersonaBar.Prompt.Models
{
    public class ModuleInfoModel
    {
        public string __ModuleId;   // command link
        public int ModuleId;
        public string Title;
        public string __ModuleName; // command link
        public string ModuleName;
        public string FriendlyName;
        public int ModuleDefId;
        public int TabModuleId;

        public string AddedToPages;
        public static ModuleInfoModel FromDnnModuleInfo(DotNetNuke.Entities.Modules.ModuleInfo dnnModule)
        {
            ModuleInfoModel mim = new ModuleInfoModel
            {
                ModuleId = dnnModule.ModuleID,
                Title = dnnModule.ModuleTitle,
                FriendlyName = dnnModule.DesktopModule.FriendlyName,
                ModuleName = dnnModule.DesktopModule.ModuleName,
                TabModuleId = dnnModule.TabModuleID,
                ModuleDefId = dnnModule.ModuleDefID
            };
            // assign command links
            mim.__ModuleId = $"get-module {mim.ModuleId}";
            mim.__ModuleName = $"list-modules --name '{mim.ModuleName}' --all";

            // get a list of all pages this module is added to
            var addedTo = DotNetNuke.Entities.Modules.ModuleController.Instance.GetTabModulesByModule(mim.ModuleId);
            StringBuilder sbAddedTo = new StringBuilder();
            foreach (DotNetNuke.Entities.Modules.ModuleInfo modInfo in addedTo)
            {
                if (sbAddedTo.Length > 0)
                    sbAddedTo.Append(", ");
                sbAddedTo.Append(modInfo.TabID);
            }
            mim.AddedToPages = sbAddedTo.ToString();
            return mim;
        }
    }
}