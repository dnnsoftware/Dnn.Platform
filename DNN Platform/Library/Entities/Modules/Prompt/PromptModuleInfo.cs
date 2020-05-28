// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Linq;
using System.Text;

namespace DotNetNuke.Entities.Modules.Prompt
{
    public class PromptModuleInfo
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
        public static PromptModuleInfo FromDnnModuleInfo(ModuleInfo dnnModule, bool? deleted = null)
        {
            var mim = new PromptModuleInfo
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
            var addedTo = ModuleController.Instance.GetTabModulesByModule(mim.ModuleId);
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
