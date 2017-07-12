using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Models;
namespace Dnn.PersonaBar.Prompt.Commands.Page
{
    [ConsoleCommand("restore-page", "Restores a previously deleted page", new string[]{
        "id",
        "name",
        "parentid"
    })]
    public class RestorePage : ConsoleCommandBase
    {
        private const string FLAG_NAME = "name";
        private const string FLAG_PARENTID = "parentid";
        private const string FLAG_ID = "id";


        public int? PageId { get; private set; }
        public string PageName { get; private set; }
        public int? ParentId { get; private set; }

        public override void Init(string[] args, PortalSettings portalSettings, UserInfo userInfo, int activeTabId)
        {
            base.Init(args, portalSettings, userInfo, activeTabId);
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
                    ParentId = tmpId;
            }

            PageName = Flag(FLAG_NAME);

            if (!PageId.HasValue && string.IsNullOrEmpty(PageName) && !ParentId.HasValue)
            {
                sbErrors.Append("You must specify either a Page ID, Page Name, or Parent ID");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var tc = TabController.Instance;
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
            else if (!string.IsNullOrEmpty(PageName))
            {
                TabInfo tab = null;
                if (ParentId.HasValue)
                {
                    tab = tc.GetTabByName(PageName, PortalId, (int)ParentId);
                }
                else
                {
                    tab = tc.GetTabByName(PageName, PortalId);
                }

                if (tab != null)
                    tabs.Add(tab);
            }
            else
            {
                // must be parent ID only
                tabs = TabController.GetTabsByParent((int)ParentId, PortalId);
            }

            StringBuilder sbResults = new StringBuilder();
            if (tabs.Count > 0)
            {
                foreach (TabInfo tab in tabs)
                {
                    if (tab.IsDeleted)
                    {
                        try
                        {
                            tc.RestoreTab(tab, PortalSettings);
                            sbResults.AppendFormat("Successfully restored '{0}' ({1}).\n", tab.TabName, tab.TabID);
                        }
                        catch (Exception ex)
                        {
                            DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                            sbResults.AppendFormat("An error occurred while restoring page ({0}). See the DNN Event Viewer for details. ", tab.TabID);
                        }
                    }
                }
            }
            else
            {
                return new ConsoleErrorResultModel("No page found to restorePage.");
            }

            return new ConsoleResultModel(sbResults.ToString());
        }


    }
}