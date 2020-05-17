// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class ProfileModuleViewBase<TModel> : ModuleView<TModel>, IProfileModule where TModel : class, new()
    {
        protected INavigationManager NavigationManager { get; }
        public ProfileModuleViewBase()
        {
            NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region IProfileModule Members

        public abstract bool DisplayModule { get; }

        public int ProfileUserId
        {
            get
            {
                int UserId = Null.NullInteger;
                if (!string.IsNullOrEmpty(Request.Params["UserId"]))
                {
                    UserId = Int32.Parse(Request.Params["UserId"]);
                }
                return UserId;
            }
        }

        #endregion

        #region Protected Properties

        protected bool IsUser
        {
            get
            {
                return ProfileUserId == ModuleContext.PortalSettings.UserId;
            }
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
                redirectUrl = TestableGlobals.Instance.GetPortalDomainName(PortalSettings.Current.PortalAlias.HTTPAlias, Request, true) + "/" + Globals.glbDefaultPage;
            }

            return redirectUrl;
        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
            if (ProfileUserId == Null.NullInteger &&
                            (ModuleContext.PortalSettings.ActiveTab.TabID == ModuleContext.PortalSettings.UserTabId
                                || ModuleContext.PortalSettings.ActiveTab.ParentId == ModuleContext.PortalSettings.UserTabId))
            {
                //Clicked on breadcrumb - don't know which user
                Response.Redirect(Request.IsAuthenticated
                                      ? NavigationManager.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID, "", "UserId=" + ModuleContext.PortalSettings.UserId.ToString(CultureInfo.InvariantCulture))
                                      : GetRedirectUrl(), true);
            }

            base.OnInit(e);
        }

        #endregion
    }
}
