using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace Dnn.PersonaBar.Prompt.Commands.Module
{
    [ConsoleCommand("list-modules", "Lists modules on current page", new string[]{

        "name",
        "title",
        "all",
        "pageid"
    })]
    public class ListModules : BaseConsoleCommand, IConsoleCommand
    {

        private const string FLAG_NAME = "name";
        private const string FLAG_TITLE = "title";
        private const string FLAG_ALL = "all";
        private const string FLAG_PAGEID = "pageid";
        private const string FLAG_DELETED = "deleted";
        //private const string FLAG_MODULENAME = "modulename"
        //private const string FLAG_PAGE = "page"
        //private const string FLAG_PAGENAME = "pagename"

        public string ValidationMessage { get; private set; }
        public int? PageId { get; private set; }
        public int? ModuleId { get; private set; }
        public string ModuleName { get; private set; }
        public string ModuleTitle { get; private set; }
        public bool All { get; private set; }
        public bool? Deleted { get; private set; }
        //public string PageName { get; }


        public void Init(string[] args, DotNetNuke.Entities.Portals.PortalSettings portalSettings, DotNetNuke.Entities.Users.UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_PAGEID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_PAGEID), out tmpId))
                {
                    PageId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("When used, the --{0} flag must be an integer; ", FLAG_PAGEID);
                }
            }
            else
            {
                if (args.Length >= 2 && !IsFlag(args[1]))
                {
                    // attempt to parse first arg as the page ID
                    int tmpId = 0;
                    if (int.TryParse(args[1], out tmpId))
                    {
                        PageId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("No valid Page ID found. Pass it using the --{0} flag or by sending it as the first argument after the command name", FLAG_PAGEID);
                    }
                }
            }

            if (!PageId.HasValue) PageId = base.TabId;


            ModuleName = Flag(FLAG_NAME);
            ModuleTitle = Flag(FLAG_TITLE);
            All = HasFlag(FLAG_ALL);
            if (HasFlag(FLAG_DELETED))
            {
                bool tmp = false;
                if (bool.TryParse(Flag(FLAG_DELETED), out tmp))
                {
                    Deleted = tmp;
                }
                else
                {
                    if (Flag(FLAG_DELETED, null) == null)
                    {
                        // user specified deleted flag with no value. Default to True
                        Deleted = true;
                    }
                }
            }

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {

            var tab = PortalSettings.ActiveTab;
            List<ModuleInfoModel> lst = null;

            if (Deleted.HasValue)
            {
                // If deleted is true, then user wants modules from recycle bin
                // Since we're dealing with module instance models rather than module info models,
                // don't follwo the normal output flow.
                var newList = GetDeletedModules(PortalId, (bool)Deleted, ModuleName, ModuleTitle);
                return new ConsoleResultModel(string.Format("{0} module{1} found", 
                    newList.Count, 
                    (newList.Count != 1 ? "s" : string.Empty))) { data = newList };

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

            return new ConsoleResultModel(string.Format("{0} module{1} found", lst.Count, (lst.Count != 1 ? "s" : string.Empty))) { data = lst };
        }



        private List<ModuleInfoModel> ListFromDictionary(Dictionary<int, ModuleInfo> dict)
        {
            List<ModuleInfoModel> lst = new List<ModuleInfoModel>();
            foreach (KeyValuePair<int, ModuleInfo> kvp in dict)
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
            List<ModuleInfoModel> lstOut = new List<ModuleInfoModel>();
            foreach (ModuleInfoModel mim in lstQuery)
            {
                lstOut.Add(mim);
            }

            return lstOut;
        }

        private List<ModuleInfoModel> GetModulesInPortal(string nameFilter = null)
        {
            ArrayList lst = ModuleController.Instance.GetModules(PortalId);
            List<ModuleInfoModel> lstOut = new List<ModuleInfoModel>();

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
                foreach (ModuleInfo mi in lstQuery)
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

            List<ModuleInstanceModel> lstMim = new List<ModuleInstanceModel>();

            string searchName = null;
            if (!string.IsNullOrEmpty(nameFilter))
                searchName = nameFilter.Replace("*", ".*");
            string searchTitle = null;
            if (!string.IsNullOrEmpty(titleFilter))
                searchTitle = titleFilter.Replace("*", ".*");

            foreach (ModuleInfo modInfo in qry)
            {
                bool bMatches = true;
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