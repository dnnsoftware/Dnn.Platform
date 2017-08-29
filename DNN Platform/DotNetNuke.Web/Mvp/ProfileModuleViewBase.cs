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
using System.Globalization;

using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public abstract class ProfileModuleViewBase<TModel> : ModuleView<TModel>, IProfileModule where TModel : class, new()
    {
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
                                      ? Globals.NavigateURL(ModuleContext.PortalSettings.ActiveTab.TabID, "", "UserId=" + ModuleContext.PortalSettings.UserId.ToString(CultureInfo.InvariantCulture))
                                      : GetRedirectUrl(), true);
            }

            base.OnInit(e);
        }

        #endregion
    }
}
