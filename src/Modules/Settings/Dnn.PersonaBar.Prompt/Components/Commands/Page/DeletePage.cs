using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using Dnn.PersonaBar.Prompt.Components.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;

namespace Dnn.PersonaBar.Prompt.Components.Commands.Page
{
    [ConsoleCommand("delete-page", "Deletes the specified page", new[]{
        "id",
        "name"
    })]
    public class DeletePage : ConsoleCommandBase
    {
        private const string FlagName = "name";
        private const string FlagParentid = "parentid";
        private const string FlagId = "id";


        public int? PageId { get; private set; }
        public string PageName { get; private set; }
        public int? ParentId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
            var sbErrors = new StringBuilder();

            if (args.Length == 2)
            {
                var tmpId = 0;
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
                var tmpId = 0;
                if (HasFlag(FlagId))
                {
                    if (!int.TryParse(Flag(FlagId), out tmpId))
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

            if (HasFlag(FlagParentid))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagParentid), out tmpId))
                {
                    if (tmpId > 0)
                    {
                        ParentId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("--{0} must be greater than 0", FlagParentid);
                    }
                }
            }

            PageName = Flag(FlagName);

            if (!PageId.HasValue && string.IsNullOrEmpty(PageName) && !ParentId.HasValue)
            {
                sbErrors.Append("You must specify either a Page ID, Page Name or Parent ID");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var tc = new TabController();
            var lst = new List<PageModel>();
            var tabs = new List<TabInfo>();

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
                var tab = tc.GetTabByName(PageName, PortalId, (int)ParentId);
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

            var sbErrors = new StringBuilder();
            foreach (var tab in tabs)
            {
                if (tab.TabID == PortalSettings.HomeTabId)
                {
                    sbErrors.AppendFormat("Cannot delete the Home page ({0}) ", tab.TabID);
                }
                else if (tab.HasChildren)
                {
                    sbErrors.AppendFormat("Detected a page ({0}) with child pages. Delete or move those first. ", tab.TabID);
                }
                else if (tab.IsSuperTab)
                {
                    sbErrors.AppendFormat("Cannot delete a Host page ({0}) ", tab.TabID);
                }
                else if (tab.IsDeleted)
                {
                    sbErrors.AppendFormat("Cannot delete a page ({0}) that has already been deleted. ", tab.TabID);
                }
                else if (tab.TabPath.StartsWith("//Admin", StringComparison.InvariantCulture))
                {
                    sbErrors.AppendFormat("Cannot delete an Admin page ({0}). ", tab.TabID);
                }
                else
                {
                    // good to delete
                    lst.Add(new PageModel(tab));
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
            foreach (var tabToDelete in lst)
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