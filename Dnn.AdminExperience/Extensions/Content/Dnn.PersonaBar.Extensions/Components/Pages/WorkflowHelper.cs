using System;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.TabVersions;

namespace Dnn.PersonaBar.Pages.Components
{
    public static class WorkflowHelper
    {
        public static DateTime GetTabLastPublishedOn(TabInfo tab)
        {
            if (tab.HasBeenPublished)
            {
                var versions = TabVersionController.Instance.GetTabVersions(tab.TabID);
                if (versions != null)
                {
                    var lastPublishedVersion =
                        versions.Where(v => v.IsPublished).OrderByDescending(v => v.Version).FirstOrDefault();
                    return lastPublishedVersion?.LastModifiedOnDate ?? tab.LastModifiedOnDate;
                }
                else
                {
                    return tab.LastModifiedOnDate;
                }
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        public static int GetTabWorkflowId(TabInfo tab)
        {
            return tab.StateID == Null.NullInteger
                ? TabWorkflowSettings.Instance.GetDefaultTabWorkflowId(PortalSettings.Current.PortalId)
                : WorkflowStateManager.Instance.GetWorkflowState(tab.StateID).WorkflowID;
        }

        public static bool IsWorkflowCompleted(TabInfo tab)
        {
            //If tab exists but ContentItem not, then we create it
            if (tab.ContentItemId == Null.NullInteger && tab.TabID != Null.NullInteger)
            {
                TabController.Instance.CreateContentItem(tab);
                TabController.Instance.UpdateTab(tab);
            }

            return WorkflowEngine.Instance.IsWorkflowCompleted(tab);
        }
    }
}