using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("delete-page", "Deletes the specified page", new string[]{
        "id",
        "name"
    })]
    public class DeletePage : BaseConsoleCommand, IConsoleCommand
    {
        private const string FLAG_NAME = "name";
        private const string FLAG_PARENTID = "parentid";
        private const string FLAG_ID = "id";

        public string ValidationMessage { get; private set; }
        public int? PageId { get; private set; }
        public string PageName { get; private set; }
        public int? ParentId { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            Initialize(args, portalSettings, userInfo, activeTabId);
            StringBuilder sbErrors = new StringBuilder();

            if (args.Length == 2)
            {
                int tmpId = 0;
                if (!int.TryParse(args[1], out tmpId))
                {
                    sbErrors.Append("No valid Page ID specified; ");
                }
                else
                {
                    PageId = tmpId;
                }
            }
            else
            {
                int tmpId = 0;
                if (HasFlag(FLAG_ID))
                {
                    if (!int.TryParse(Flag(FLAG_ID), out tmpId))
                    {
                        sbErrors.Append("You must specify a valid number for Page ID; ");
                    }
                    else
                    {
                        PageId = tmpId;
                    }
                }
            }

            if (PageId <= 0)
            {
                sbErrors.AppendFormat("Page ID must be greater than 0; ");
            }

            if (HasFlag(FLAG_PARENTID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_PARENTID), out tmpId))
                {
                    if (tmpId > 0)
                    {
                        ParentId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("--{0} must be greater than 0", FLAG_PARENTID);
                    }
                }
            }

            PageName = Flag(FLAG_NAME);

            if (!PageId.HasValue && string.IsNullOrEmpty(PageName) && !ParentId.HasValue)
            {
                sbErrors.Append("You must specify either a Page ID, Page Name or Parent ID");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(ValidationMessage);
        }

        public ConsoleResultModel Run()
        {
            TabController tc = new TabController();
            List<PageModel> lst = new List<PageModel>();
            List<TabInfo> tabs = new List<TabInfo>();

            if (PageId.HasValue)
            {
                var tab = tc.GetTab((int)PageId, PortalId);
                if (tab != null)
                {
                    tabs.Add(tab);
                }

            }
            else if (ParentId.HasValue && !string.IsNullOrEmpty(PageName))
            {
                // delete tab with a particular page name and parent 
                TabInfo tab = tc.GetTabByName(PageName, PortalId, (int)ParentId);
                if (tab != null)
                    tabs.Add(tab);

            }
            else if (ParentId.HasValue)
            {
                // delete all first-level children of parent pageId
                tabs = TabController.GetTabsByParent((int)ParentId, PortalId);
            }
            else
            {
                // delete a page with a particular page name
                var tab = tc.GetTabByName(PageName, PortalId);
                if (tab != null)
                    tabs.Add(tab);
            }

            StringBuilder sbErrors = new StringBuilder();
            foreach (TabInfo tab in tabs)
            {
                var _with1 = tab;
                if (_with1.HasChildren)
                {
                    sbErrors.AppendFormat("Detected a page ({0}) with child pages. Delete or move those first. ", tab.TabID);
                }
                else if (_with1.IsSuperTab)
                {
                    sbErrors.AppendFormat("Cannot delete a Host page ({0}) ", tab.TabID);
                }
                else if (_with1.IsDeleted)
                {
                    sbErrors.AppendFormat("Cannot delete a page ({0}) that has already been deleted. ", tab.TabID);
                }
                else if (_with1.TabPath.StartsWith("//Admin", StringComparison.InvariantCulture))
                {
                    sbErrors.AppendFormat("Cannot delete an Admin page ({0}). ", tab.TabID);
                }
                else
                {
                    // good to delete
                    lst.Add(PageModel.FromDnnTabInfo(tab));
                }
            }

            if (sbErrors.Length > 0)
            {
                sbErrors.Append(" No changes have been made.");
                return new ConsoleErrorResultModel(sbErrors.ToString());
            }

            // notify user if the tab wasn't found
            if (lst.Count == 0)
            {
                return new ConsoleErrorResultModel("No page found to delete.");
            }

            var sbResults = new StringBuilder();
            foreach (PageModel tabToDelete in lst)
            {
                // delete the pages
                try
                {
                    tc.SoftDeleteTab(tabToDelete.TabId, PortalSettings);
                    sbResults.AppendFormat("Successfully deleted '{0}' ({1}).\n", tabToDelete.Name, tabToDelete.TabId);
                }
                catch (Exception ex)
                {
                    Exceptions.LogException(ex);
                    sbErrors.AppendFormat("An error occurred while deleting the page with a Page ID of '{0}'. See the DNN Event Viewer for details", tabToDelete.TabId);
                }
            }

            return new ConsoleResultModel(sbResults.ToString());
        }


    }
}