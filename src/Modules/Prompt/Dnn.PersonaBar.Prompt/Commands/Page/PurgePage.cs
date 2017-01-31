using Dnn.PersonaBar.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Common;
using Dnn.PersonaBar.Prompt.Interfaces;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("purge-page", "Permanently deletes a page from the DNN Recycle Bin", new string[]{
        "id",
        "parentid"
    })]
    public class PurgePage : BaseConsoleCommand, IConsoleCommand
    {

        private const string FLAG_ID = "id";
        private const string FLAG_PARENTID = "parentid";

        public string ValidationMessage { get; private set; }
        public int? PageId { get; private set; }
        public int? ParentId { get; private set; }

        public void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Initialize(args, portalSettings, userInfo, activeTabId);
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

            if (HasFlag(FLAG_PARENTID))
            {
                int tmpId = 0;
                if (int.TryParse(Flag(FLAG_PARENTID), out tmpId))
                {
                    if (tmpId == -1 || tmpId > 0)
                    {
                        ParentId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("--{0} must be greater than zero (0) or -1", FLAG_PARENTID);
                    }
                }
            }

            if (!PageId.HasValue && !ParentId.HasValue)
            {
                sbErrors.Append("You must specify a Page ID or a Parent ID");
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
            List<PageInfoModel> lst = new List<PageInfoModel>();
            List<TabInfo> tabs = new List<TabInfo>();

            if (PageId.HasValue)
            {
                var tab = tc.GetTab((int)PageId, PortalId);
                if (tab != null)
                {
                    tabs.Add(tab);
                }
            }
            else
            {
                // must be parent ID
                tabs = TabController.GetTabsByParent((int)ParentId, PortalId);
            }

            // hard-delete deleted tabs only
            StringBuilder sbResults = new StringBuilder();
            if (tabs.Count > 0)
            {
                foreach (TabInfo tab in tabs)
                {
                    if (tab.IsDeleted)
                    {
                        try
                        {
                            tc.DeleteTab(tab.TabID, PortalId);
                            sbResults.AppendFormat("Successfully purged '{0}' ({1}).\n", tab.TabName, tab.TabID);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                            sbResults.AppendFormat("An unexpected error occurred while purging page '{0}' ({1}).\n", tab.TabName, tab.TabID);
                        }
                    }
                    else
                    {
                        sbResults.AppendFormat("Cannot purge page '{0}' ({1}) because it has not been deleted. Try delete-page first.\n", tab.TabName, tab.TabID);
                    }
                }
            }
            else
            {
                sbResults.Append("No pages were found to purge.");
            }

            return new ConsoleResultModel(sbResults.ToString());
        }


    }
}