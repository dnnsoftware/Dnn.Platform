#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Services.Social.Subscriptions
{
    /// <summary>
    /// This controller provides permission info about the User Subscription.
    /// </summary>
    public class SubscriptionSecurityController : ServiceLocator<ISubscriptionSecurityController, SubscriptionSecurityController>, ISubscriptionSecurityController
    {
        protected override Func<ISubscriptionSecurityController> GetFactory()
        {
            return () => new SubscriptionSecurityController();
        }

        public bool HasPermission(Subscription subscription)
        {
            var userInfo = GetUserFromSubscription(subscription);

            var moduleInfo = GetModuleFromSubscription(subscription);
            if (moduleInfo != null && !moduleInfo.InheritViewPermissions)
            {
                return HasUserModuleViewPermission(userInfo, moduleInfo);
            }

            var tabInfo = GetTabFromSubscription(subscription);
            if (tabInfo != null)
            {
                return HasUserTabViewPermission(userInfo, tabInfo);
            }

            return true;
        }

        #region Private Static Methods
        private static bool HasUserModuleViewPermission(UserInfo userInfo, ModuleInfo moduleInfo)
        {
            var portalSettings = new PortalSettings(moduleInfo.PortalID);
            return PortalSecurity.IsInRoles(userInfo, portalSettings, moduleInfo.ModulePermissions.ToString("VIEW"));
        }

        private static bool HasUserTabViewPermission(UserInfo userInfo, TabInfo tabInfo)
        {
            var portalSettings = new PortalSettings(tabInfo.PortalID);
            return PortalSecurity.IsInRoles(userInfo, portalSettings, tabInfo.TabPermissions.ToString("VIEW"));
        }

        private static TabInfo GetTabFromSubscription(Subscription subscription)
        {
            return new TabController().GetTab(subscription.TabId, subscription.PortalId, false);
        }

        private static ModuleInfo GetModuleFromSubscription(Subscription subscription)
        {
            return new ModuleController().GetModule(subscription.ModuleId);
        }

        private static UserInfo GetUserFromSubscription(Subscription subscription)
        {
            return new UserController().GetUser(subscription.PortalId, subscription.UserId);
        }
        #endregion
    }
}
