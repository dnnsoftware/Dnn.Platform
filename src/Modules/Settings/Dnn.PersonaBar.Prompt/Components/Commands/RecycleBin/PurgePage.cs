using System;
using System.Collections.Generic;
using System.Text;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Library.Prompt.Attributes;
using Dnn.PersonaBar.Library.Prompt.Models;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;

namespace Dnn.PersonaBar.Prompt.Components.Commands.RecycleBin
{
    [ConsoleCommand("purge-page", "Permanently deletes a page from the DNN Recycle Bin", new[]{
        "id",
        "parentid"
    })]
    public class PurgePage : ConsoleCommandBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PurgePage));

        private const string FlagId = "id";
        private const string FlagParentid = "parentid";


        int? PageId { get; set; }
        int? ParentId { get; set; }

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

            if (HasFlag(FlagParentid))
            {
                var tmpId = 0;
                if (int.TryParse(Flag(FlagParentid), out tmpId))
                {
                    if (tmpId == -1 || tmpId > 0)
                    {
                        ParentId = tmpId;
                    }
                    else
                    {
                        sbErrors.AppendFormat("--{0} must be greater than zero (0) or -1", FlagParentid);
                    }
                }
            }

            if (!PageId.HasValue && !ParentId.HasValue)
            {
                sbErrors.Append("You must specify a Page ID or a Parent ID");
            }
            ValidationMessage = sbErrors.ToString();
        }

        public override ConsoleResultModel Run()
        {
            var tc = new TabController();
            var tabs = new List<TabInfo>();

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
            var sbResults = new StringBuilder();
            if (tabs.Count > 0)
            {
                foreach (var tab in tabs)
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
                            Logger.Error(ex);
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