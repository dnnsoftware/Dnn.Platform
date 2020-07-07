// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using System.Linq;
    using System.Text;

    public class ModuleInfoModel
    {
        public string __ModuleId { get; set; }   // command link
        public int ModuleId { get; set; }
        public string Title { get; set; }
        public string __ModuleName { get; set; } // command link
        public string ModuleName { get; set; }
        public string FriendlyName { get; set; }
        public int ModuleDefId { get; set; }
        public int TabModuleId { get; set; }
        public string AddedToPages { get; set; }

        public static ModuleInfoModel FromDnnModuleInfo(DotNetNuke.Entities.Modules.ModuleInfo dnnModule, bool? deleted = null)
        {
            var mim = new ModuleInfoModel
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
            if (deleted.HasValue)
                addedTo = addedTo.Where(x => x.IsDeleted == deleted.Value).ToList();

            var sbAddedTo = new StringBuilder();
            foreach (var modInfo in addedTo)
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
