using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Module
{
    [ConsoleCommand("list-modules", "Lists modules on current page", new[]{

        "name",
        "title",
        "all",
        "pageid"
    })]
    public class ListModules : ConsoleCommandBase
    {

        private const string FlagName = "name";
        private const string FlagTitle = "title";
        private const string FlagAll = "all";
        private const string FlagPageid = "pageid";
        private const string FlagDeleted = "deleted";
        //private const string FLAG_MODULENAME = "modulename"
        //private const string FLAG_PAGE = "page"
        //private const string FLAG_PAGENAME = "pagename"


        public int? PageId { get; private set; }
        public int? ModuleId { get; private set; }
        public string ModuleName { get; private set; }
        public string ModuleTitle { get; private set; }
        public bool All { get; private set; }
        public bool? Deleted { get; private set; }
        //public string PageName { get; }


        public void Init(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagPageid))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagPageid), out tmpId))
                {
                    PageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("When used, the --{0} flag must be an integer; ", FlagPageid);
                }
            }
            else
            {
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    // attempt to parse first arg as the page ID
                    var tmpId = 0;
                    if (int.TryParse(args[1], out tmpId))
                    {
                        PageId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("No valid Page ID found. Pass it using the --{0} flag or by sending it as the first argument after the command name", FlagPageid);
                    }
                }
            }

            if (!PageId.HasValue) PageId = TabId;


            ModuleName = Flag(FlagName);
            ModuleTitle = Flag(FlagTitle);
            All = HasFlag(FlagAll);
            if (HasFlag(FlagDeleted))
            {
                var tmp = false;
                if (bool.TryParse(Flag(FlagDeleted), out tmp))
                {
                    Deleted = tmp;
                }
                else
                {
                    if (Flag(FlagDeleted, null) == null)
                    {
                        // user specified deleted flag with no value. Default to True
                        Deleted = true;
                    }
                }
            }

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {

            var tab = PortalSettings.ActiveTab;
            List<ModuleInfoModel> lst = null;

            if (Deleted.HasValue)
            {
                // If deleted is true, then user wants modules from recycle bin
                // Since we're dealing with module instance models rather than module info models,
                // don't follwo the normal output flow.
                var newList = GetDeletedModules(PortalId, (bool)Deleted, ModuleName, ModuleTitle);
                return new ConsoleResultModel($"{newList.Count} module{(newList.Count != 1 ? "s" : string.Empty)} found") { Data = newList };

            }
            else if (All)
            {
                // if --all passed then, since it gets all modules in the portal, that takes precendence
                lst = GetModulesInPortal(ModuleName);
            }
            else if (ModuleId.HasValue)
            {
                lst = GetModuleById();
            }
            else
            {
                lst = GetModulesOnPage((int)PageId, ModuleName);
            }

            if (lst.Count > 0)
            {
                if (!string.IsNullOrEmpty(ModuleTitle))
                {
                    var lstFiltered = from o in lst
                                      where o.Title.ToLowerInvariant().Contains(ModuleTitle.ToLowerInvariant())
                                      select o;
                    lst = lstFiltered.ToList();
                }
            }
            // ensure an empty value is passed back
            if (lst == null)
                lst = new List<ModuleInfoModel>();

            return new ConsoleResultModel($"{lst.Count} module{(lst.Count != 1 ? "s" : string.Empty)} found") { Data = lst };
        }



        private List<ModuleInfoModel> ListFromDictionary(Dictionary<int, ModuleInfo> dict)
        {
            var lst = new List<ModuleInfoModel>();
            foreach (var kvp in dict)
            {
                lst.Add(ModuleInfoModel.FromDnnModuleInfo(kvp.Value));
            }
            return lst;
        }

        private List<ModuleInfoModel> GetModulesOnPage(int tabId, string nameFilter = null)
        {
            var tab = TabController.Instance.GetTab(tabId, PortalId, true);
            return GetModulesOnPage(tab, nameFilter);
        }

        private List<ModuleInfoModel> GetModulesOnPage(TabInfo tab, string nameFilter = null)
        {
            // get the DNN tab
            var lst = ListFromDictionary(tab.ChildModules);

            if (string.IsNullOrEmpty(nameFilter))
            {
                return lst;
            }

            var lstQuery = from o in lst
                           where o.ModuleName.ToLowerInvariant() == nameFilter.Trim().ToLowerInvariant()
                           select o;
            var lstOut = new List<ModuleInfoModel>();
            foreach (var mim in lstQuery)
            {
                lstOut.Add(mim);
            }

            return lstOut;
        }

        private List<ModuleInfoModel> GetModulesInPortal(string nameFilter = null)
        {
            var lst = ModuleController.Instance.GetModules(PortalId);
            var lstOut = new List<ModuleInfoModel>();

            if (string.IsNullOrEmpty(nameFilter))
            {
                foreach (ModuleInfo mi in lst)
                {
                    lstOut.Add(ModuleInfoModel.FromDnnModuleInfo(mi));
                }
            }
            else
            {
                var lstQuery = from ModuleInfo o in lst
                               where o.DesktopModule.ModuleName.ToLowerInvariant().Contains(nameFilter.Trim().ToLowerInvariant())
                               select o;
                foreach (var mi in lstQuery)
                {
                    lstOut.Add(ModuleInfoModel.FromDnnModuleInfo(mi));
                }
            }

            return lstOut;
        }

        private List<ModuleInstanceModel> GetDeletedModules(int portalId, bool isDeleted, string nameFilter = null, string titleFilter = null)
        {
            var lst = ModuleController.Instance.GetAllModules();
            var qry = from ModuleInfo m in lst
                      where m.IsDeleted == isDeleted && m.PortalID == portalId
                      select m;

            var lstMim = new List<ModuleInstanceModel>();

            string searchName = null;
            if (!string.IsNullOrEmpty(nameFilter))
                searchName = nameFilter.Replace("*", ".*");
            string searchTitle = null;
            if (!string.IsNullOrEmpty(titleFilter))
                searchTitle = titleFilter.Replace("*", ".*");

            foreach (var modInfo in qry)
            {
                var bMatches = true;
                if (!string.IsNullOrEmpty(searchName))
                {
                    bMatches = bMatches & Regex.IsMatch(modInfo.DesktopModule.ModuleName, searchName);
                }
                if (!string.IsNullOrEmpty(searchTitle))
                {
                    bMatches = bMatches & Regex.IsMatch(modInfo.ModuleTitle, searchTitle);
                }
                if (bMatches)
                    lstMim.Add(ModuleInstanceModel.FromDnnModuleInfo(modInfo));
            }

            return lstMim;
        }

        private List<ModuleInfoModel> GetModuleById()
        {
            var mc = new ModuleController();
            var mi = mc.GetModule((int)ModuleId, (int)PageId, false);
            var lst = new List<ModuleInfoModel>();
            if (mi != null)
                lst.Add(ModuleInfoModel.FromDnnModuleInfo(mi));
            return lst;
        }

    }
}