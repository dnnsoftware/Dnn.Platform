using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("list-pages", "Retrieves a list of pages based on the specified criteria", new string[]{
        "parentid",
        "deleted",
        "name",
        "title",
        "path",
        "skin"
    })]
    public class ListPages : BaseConsoleCommand, IConsoleCommand
    {
        private const string FLAG_PARENTID = "parentid";
        private const string FLAG_DELETED = "deleted";
        private const string FLAG_NAME = "name";
        private const string FLAG_TITLE = "title";
        private const string FLAG_PATH = "path";
        private const string FLAG_SKIN = "skin";
        private const string FLAG_VISIBLE = "visible";

        public string ValidationMessage { get; private set; }
        public int? ParentId { get; private set; }
        public bool? Deleted { get; private set; }
        public string PageName { get; private set; }
        public string PageTitle { get; private set; }
        public string PagePath { get; private set; }
        public string PageSkin { get; private set; }
        public bool? PageVisible { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (HasFlag(FLAG_PARENTID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_PARENTID), out tmpId))
                {
                    ParentId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("When specifying --{0}, you must supply a valid numeric Page (Tab) ID", FLAG_PARENTID);
                }
            }
            else if (args.Length > 1 && !IsFlag(args[1]))
            {
                int tmpId = 0;
                if (int.TryParse(args[1], out tmpId))
                {
                    ParentId = tmpId;
                }
            }

            if (HasFlag(FLAG_DELETED))
            {
                // if flag is specified but has no value, default to it being true
                if (string.IsNullOrEmpty(Flag(FLAG_DELETED)))
                {
                    Deleted = true;
                }
                else
                {
                    bool tmpDeleted = false;
                    if (bool.TryParse(Flag(FLAG_DELETED), out tmpDeleted))
                    {
                        Deleted = tmpDeleted;
                    }
                    else
                    {
                        sbErrors.AppendFormat("When specifying the --{0} flag, you must pass a true or false value or leave it empty (to default to true)", FLAG_DELETED);
                    }
                }
            }

            if (HasFlag(FLAG_VISIBLE))
            {
                bool tmp = false;
                if (bool.TryParse(Flag(FLAG_VISIBLE), out tmp))
                {
                    PageVisible = tmp;
                }
                else if (Flag(FLAG_VISIBLE, null) == null)
                {
                    // default to true
                    PageVisible = true;
                }
                else
                {
                    sbErrors.AppendFormat("When specifying the --{0} flag, you must pass a true or false value or leave it empty (to default to true)", FLAG_VISIBLE);
                }
            }

            if (HasFlag(FLAG_NAME))
                PageName = Flag(FLAG_NAME);
            if (HasFlag(FLAG_TITLE))
                PageTitle = Flag(FLAG_TITLE);
            if (HasFlag(FLAG_PATH))
                PagePath = Flag(FLAG_PATH);
            if (HasFlag(FLAG_SKIN))
                PageSkin = Flag(FLAG_SKIN);

            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            TabController tc = new TabController();
            List<PageModelBase> lst = new List<PageModelBase>();
            List<PageModelBase> lstOut = new List<PageModelBase>();

            if (ParentId.HasValue)
            {
                lst = GetPagesByParentId((int)ParentId);
            }
            else
            {
                TabCollection tabs = tc.GetTabsByPortal(PortalId);
                foreach (KeyValuePair<int, TabInfo> kvp in tabs)
                {
                    lst.Add(new PageModelBase(kvp.Value));
                }
            }

            // filter results if needed
            if (Deleted.HasValue)
            {
                var query = from page in lst
                            where page.IsDeleted == Deleted
                            select page;
                List<PageModelBase> filteredList = query.ToList();
                lst = filteredList;
            }


            bool bSearchTitle = false;
            string searchTitle = null;
            bool bSearchName = false;
            string searchName = null;
            bool bSearchPath = false;
            string searchPath = null;
            bool bSearchSkin = false;
            string searchSkin = null;
            bool bMatchVisibility = PageVisible.HasValue;

            if (!string.IsNullOrEmpty(PageName))
            {
                searchName = PageName.Replace("*", ".*");
                bSearchName = true;
            }
            if (!string.IsNullOrEmpty(PageTitle))
            {
                searchTitle = PageTitle.Replace("*", ".*");
                bSearchTitle = true;
            }
            if (!string.IsNullOrEmpty(PagePath))
            {
                searchPath = PagePath.Replace("*", ".*");
                bSearchPath = true;
            }
            if (!string.IsNullOrEmpty(PageSkin))
            {
                searchSkin = PageSkin.Replace("*", ".*");
                bSearchSkin = true;
            }

            if (bSearchTitle || bSearchName || bSearchPath || bSearchSkin || bMatchVisibility)
            {
                bool bIsMatch = false;
                foreach (PageModelBase pim in lst)
                {
                    bIsMatch = true;
                    if (bSearchTitle)
                    {
                        bIsMatch = bIsMatch & Regex.IsMatch(pim.Title, searchTitle, RegexOptions.IgnoreCase);
                    }
                    if (bSearchName)
                    {
                        bIsMatch = bIsMatch & Regex.IsMatch(pim.Name, searchName, RegexOptions.IgnoreCase);
                    }
                    if (bSearchPath)
                    {
                        bIsMatch = bIsMatch & Regex.IsMatch(pim.Path, searchPath, RegexOptions.IgnoreCase);
                    }
                    if (bSearchSkin)
                    {
                        bIsMatch = bIsMatch & Regex.IsMatch(pim.Skin, searchSkin, RegexOptions.IgnoreCase);
                    }
                    if (bMatchVisibility)
                    {
                        bIsMatch = bIsMatch & (pim.IncludeInMenu == PageVisible);
                    }
                    if (bIsMatch)
                        lstOut.Add(pim);
                }
            }
            else
            {
                lstOut = lst;
            }

            var msg = string.Format("{0} page{1} found", lstOut.Count, (lstOut.Count != 1 ? "s" : ""));
            return new ConsoleResultModel(msg) {
                data = lstOut,
                fieldOrder = new string[] {
                    "TabId", "ParentId", "Name", "Title", "Skin", "Path", "IncludeInMenu", "IsDeleted" }
            };
        }

        private List<PageModelBase> GetPagesByParentId(int parentId)
        {
            var lstTabs = TabController.GetTabsByParent(parentId, PortalId);
            List<PageModelBase> lstOut = new List<PageModelBase>();

            foreach (TabInfo tab in lstTabs)
            {
                lstOut.Add(new PageModelBase(tab));
            }

            return lstOut;
        }

    }
}