#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Globalization;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowSettings : ServiceLocator<IWorkflowSettings, WorkflowSettings>, IWorkflowSettings
    {
        #region Constants
        private const string DefaultTabWorkflowKey = "DefaultTabWorkflowKey";
        private const string WorkflowEnableKey = "WorkflowEnabledKey";
        #endregion

        #region Public Methods
        public int GetDefaultTabWorkflowId(int portalId)
        {
            var workflowId = PortalController.GetPortalSettingAsInteger(DefaultTabWorkflowKey, portalId, Null.NullInteger);
            if (workflowId == Null.NullInteger)
            {
                workflowId = SystemWorkflowManager.Instance.GetDirectPublishWorkflow(portalId).WorkflowID;
                PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
            }
            return workflowId;
        }

        public void SetDefaultTabWorkflowId(int portalId, int workflowId)
        {
            PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
        }

        public void SetWorkflowEnabled(int portalId, bool enabled)
        {
            Requires.NotNegative("portalId", portalId);
            PortalController.UpdatePortalSetting(portalId, WorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture), true);
        }

        public bool IsWorkflowEnabled(int portalId, int tabId)
        {
            if (portalId == Null.NullInteger)
            {
                return false;
            }

            var isWorkflowEnabledForPortal =
                Convert.ToBoolean(PortalController.GetPortalSetting(WorkflowEnableKey, portalId, Boolean.TrueString));
            var isWorkflowEnabledForTab = true;
                            // TODO: uncomment when merging with development branch:
                            // tabId == Null.NullInteger 
                            //                    || !TabController.Instance.IsHostOrAdminPage(tabId, portalId);

            return isWorkflowEnabledForPortal && isWorkflowEnabledForTab;
        }
        #endregion

        #region Service Locator
        protected override Func<IWorkflowSettings> GetFactory()
        {
            return () => new WorkflowSettings();
        }
        #endregion
    }
}
