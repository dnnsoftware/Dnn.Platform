#region Usings

using System;
using System.Globalization;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.UI.Modules
{
    public abstract class ProfileModuleUserControlBase : ModuleUserControlBase, IProfileModule
    {
        protected INavigationManager NavigationManager { get; }
        public ProfileModuleUserControlBase()
        {
            NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region IProfileModule Members

        public abstract bool DisplayModule { get; }

        public int ProfileUserId
        {
            get
            {
                if (!string.IsNullOrEmpty(Request.Params["UserId"]))
                {
                    return Int32.Parse(Request.Params["UserId"]);
                }

                return UserController.Instance.GetCurrentUserInfo().UserID;
            }
        }

        #endregion

        #region Protected Properties

        protected bool IsUser
        {
            get { return ProfileUserId == ModuleContext.PortalSettings.UserId; }
        }

        protected UserInfo ProfileUser
        {
            get { return UserController.GetUserById(ModuleContext.PortalId, ProfileUserId); }
        }

        #endregion

        #region Private Methods

        private string GetRedirectUrl()
        {
            //redirect user to default page if not specific the home tab, do this action to prevent loop redirect.
            var homeTabId = ModuleContext.PortalSettings.HomeTabId;
            string redirectUrl;

            if (homeTabId > Null.NullInteger)
            {
                redirectUrl = TestableGlobals.Instance.NavigateURL(homeTabId);
            }
            else
            {
                redirectUrl = TestableGlobals.Instance.GetPortalDomainName(PortalSettings.Current.PortalAlias.HTTPAlias, Request, true) +
                              "/" + Globals.glbDefaultPage;
            }

            return redirectUrl;
        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
			if (string.IsNullOrEmpty(Request.Params["UserId"]) &&
                            (ModuleContext.PortalSettings.ActiveTab.TabID == ModuleContext.PortalSettings.UserTabId
                                || ModuleContext.PortalSettings.ActiveTab.ParentId == ModuleContext.PortalSettings.UserTabId))
            {
                try
                {
                    //Clicked on breadcrumb - don't know which user
                    Response.Redirect(Request.IsAuthenticated
                                          ? NavigationManager.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID, "", "UserId=" + ModuleContext.PortalSettings.UserId.ToString(CultureInfo.InvariantCulture))
                                          : GetRedirectUrl(), true);
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
            }

            base.OnInit(e);
        }

        #endregion
    }
}
