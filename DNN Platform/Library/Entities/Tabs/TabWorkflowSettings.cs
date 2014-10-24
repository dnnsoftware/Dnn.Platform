using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs
{
    class TabWorkflowSettings : ServiceLocator<ITabWorkflowSettings, TabWorkflowSettings>, ITabWorkflowSettings
    {
        #region Public Methods
        public bool IsWorkflowEnabled(int portalId, int tabId)
        {
            if (portalId == Null.NullInteger)
            {
                return false;
            }

            var isWorkflowEnabledForPortal = WorkflowSettings.Instance.IsWorkflowEnabled(portalId);

            var tabInfo = TabController.Instance.GetTab(tabId, portalId);
            var isWorkflowEnabledForTab = !TabController.Instance.IsHostOrAdminPage(tabInfo);

            return isWorkflowEnabledForPortal && isWorkflowEnabledForTab;
        }
        #endregion

        #region Service Locator
        protected override Func<ITabWorkflowSettings> GetFactory()
        {
            return () => new TabWorkflowSettings();
        }
        #endregion
    }
}
