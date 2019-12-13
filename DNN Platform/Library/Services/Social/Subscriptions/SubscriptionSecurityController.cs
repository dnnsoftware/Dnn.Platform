using System;

using DotNetNuke.Common.Utilities;
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
            return TabController.Instance.GetTab(subscription.TabId, subscription.PortalId, false);
        }

        private static ModuleInfo GetModuleFromSubscription(Subscription subscription)
        {
            return ModuleController.Instance.GetModule(subscription.ModuleId, Null.NullInteger, true);
        }

        private static UserInfo GetUserFromSubscription(Subscription subscription)
        {
            return UserController.Instance.GetUser(subscription.PortalId, subscription.UserId);
        }
        #endregion
    }
}
