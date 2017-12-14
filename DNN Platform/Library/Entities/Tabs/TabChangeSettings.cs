#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs.Dto;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs
{
    public class TabChangeSettings : ServiceLocator<ITabChangeSettings, TabChangeSettings>, ITabChangeSettings
    {

        #region Public Methods
        public bool IsChangeControlEnabled(int portalId, int tabId)
        {
            if (portalId == Null.NullInteger)
            {
                return false;
            }
            var isVersioningEnabled =  TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId);
            var isWorkflowEnable = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalId, tabId);
            return isVersioningEnabled || isWorkflowEnable;
        }

        public ChangeControlState GetChangeControlState(int portalId, int tabId)
        {
            return new ChangeControlState
            {
                PortalId = portalId,
                TabId = tabId,
                IsVersioningEnabledForTab = TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId),
                IsWorkflowEnabledForTab = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalId, tabId)
            };
        }

        #endregion

        #region Service Locator
        protected override Func<ITabChangeSettings> GetFactory()
        {
            return () => new TabChangeSettings();
        }
        #endregion
    }
}
