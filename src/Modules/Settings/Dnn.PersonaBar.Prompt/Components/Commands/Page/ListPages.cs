using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Page
{
    [ConsoleCommand("list-pages", "Retrieves a list of pages based on the specified criteria", new[]{
        "parentid",
        "deleted",
        "name",
        "title",
        "path",
        "skin"
    })]
    public class ListPages : ConsoleCommandBase
    {
        private const string FlagParentid = "parentid";
        private const string FlagDeleted = "deleted";
        private const string FlagName = "name";
        private const string FlagTitle = "title";
        private const string FlagPath = "path";
        private const string FlagSkin = "skin";
        private const string FlagVisible = "visible";


        public int? ParentId { get; private set; }
        public bool? Deleted { get; private set; }
        public string PageName { get; private set; }
        public string PageTitle { get; private set; }
        public string PagePath { get; private set; }
        public string PageSkin { get; private set; }
        public bool? PageVisible { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (HasFlag(FlagParentid))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagParentid), out tmpId))
                {
                    ParentId = tmpId;
                }
                else
                {
                    sbErrors.AppendFormat("When specifying --{0}, you must supply a valid numeric Page (Tab) ID", FlagParentid);
                }
            }
            else if (args.Length > 1 && !IsFlag(args[1]))
            {
                var tmpId = 0;
                if (int.TryParse(args[1], out tmpId))
                {
                    ParentId = tmpId;
                }
            }

            if (HasFlag(FlagDeleted))
            {
                // if flag is specified but has no value, default to it being true
                if (string.IsNullOrEmpty(Flag(FlagDeleted)))
                {
                    Deleted = true;
                }
                else
                {
                    var tmpDeleted = false;
                    if (bool.TryParse(Flag(FlagDeleted), out tmpDeleted))
                    {
                        Deleted = tmpDeleted;
                    }
                    else
                    {
                        sbErrors.AppendFormat("When specifying the --{0} flag, you must pass a true or false value or leave it empty (to default to true)", FlagDeleted);
                    }
                }
            }

            if (HasFlag(FlagVisible))
            {
                var tmp = false;
                if (bool.TryParse(Flag(FlagVisible), out tmp))
                {
                    PageVisible = tmp;
                }
                else if (Flag(FlagVisible, null) == null)
                {
                    // default to true
                    PageVisible = true;
                }
                else
                {
                    sbErrors.AppendFormat("When specifying the --{0} flag, you must pass a true or false value or leave it empty (to default to true)", FlagVisible);
                }
            }

            if (HasFlag(FlagName))
                PageName = Flag(FlagName);
            if (HasFlag(FlagTitle))
                PageTitle = Flag(FlagTitle);
            if (HasFlag(FlagPath))
                PagePath = Flag(FlagPath);
            if (HasFlag(FlagSkin))
                PageSkin = Flag(FlagSkin);

            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var tc = new TabController();
            var lst = new List<PageModelBase>();
            var lstOut = new List<PageModelBase>();

            if (ParentId.HasValue)
            {
                lst = GetPagesByParentId((int)ParentId);
            }
            else
            {
                var tabs = tc.GetTabsByPortal(PortalId);
                foreach (var kvp in tabs)
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
                var filteredList = query.ToList();
                lst = filteredList;
            }


            var bSearchTitle = false;
            string searchTitle = null;
            var bSearchName = false;
            string searchName = null;
            var bSearchPath = false;
            string searchPath = null;
            var bSearchSkin = false;
            string searchSkin = null;
            var bMatchVisibility = PageVisible.HasValue;

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
                var bIsMatch = false;
                foreach (var pim in lst)
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

            var msg = $"{lstOut.Count} page{(lstOut.Count != 1 ? "s" : "")} found";
            return new ConsoleResultModel(msg)
            {
                Data = lstOut,
                FieldOrder = new[] {
                    "TabId", "ParentId", "Name", "Title", "Skin", "Path", "IncludeInMenu", "IsDeleted" }
            };
        }

        private List<PageModelBase> GetPagesByParentId(int parentId)
        {
            var lstTabs = TabController.GetTabsByParent(parentId, PortalId);
            var lstOut = new List<PageModelBase>();

            foreach (var tab in lstTabs)
            {
                lstOut.Add(new PageModelBase(tab));
            }

            return lstOut;
        }

    }
}