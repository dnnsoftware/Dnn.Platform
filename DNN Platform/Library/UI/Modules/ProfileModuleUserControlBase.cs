﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        #region IProfileModule Members

        public abstract bool DisplayModule { get; }

        public int ProfileUserId
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Request.Params["UserId"]))
                {
                    return Int32.Parse(this.Request.Params["UserId"]);
                }

                return UserController.Instance.GetCurrentUserInfo().UserID;
            }
        }

        #endregion

        #region Protected Properties

        protected bool IsUser
        {
            get { return this.ProfileUserId == this.ModuleContext.PortalSettings.UserId; }
        }

        protected UserInfo ProfileUser
        {
            get { return UserController.GetUserById(this.ModuleContext.PortalId, this.ProfileUserId); }
        }

        #endregion

        #region Private Methods

        private string GetRedirectUrl()
        {
            //redirect user to default page if not specific the home tab, do this action to prevent loop redirect.
            var homeTabId = this.ModuleContext.PortalSettings.HomeTabId;
            string redirectUrl;

            if (homeTabId > Null.NullInteger)
            {
                redirectUrl = TestableGlobals.Instance.NavigateURL(homeTabId);
            }
            else
            {
                redirectUrl = TestableGlobals.Instance.GetPortalDomainName(PortalSettings.Current.PortalAlias.HTTPAlias, this.Request, true) +
                              "/" + Globals.glbDefaultPage;
            }

            return redirectUrl;
        }

        #endregion

        #region Protected Methods

        protected override void OnInit(EventArgs e)
        {
			if (string.IsNullOrEmpty(this.Request.Params["UserId"]) &&
                            (this.ModuleContext.PortalSettings.ActiveTab.TabID == this.ModuleContext.PortalSettings.UserTabId
                                || this.ModuleContext.PortalSettings.ActiveTab.ParentId == this.ModuleContext.PortalSettings.UserTabId))
            {
                try
                {
                    //Clicked on breadcrumb - don't know which user
                    this.Response.Redirect(this.Request.IsAuthenticated
                                          ? this.NavigationManager.NavigateURL(this.ModuleContext.PortalSettings.ActiveTab.TabID, "", "UserId=" + this.ModuleContext.PortalSettings.UserId.ToString(CultureInfo.InvariantCulture))
                                          : this.GetRedirectUrl(), true);
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
