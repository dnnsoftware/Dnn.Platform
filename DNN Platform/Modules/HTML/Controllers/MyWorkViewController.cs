// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    public class MyWorkViewController : ModuleViewControllerBase
    {
        private readonly INavigationManager navigationManager;

        public MyWorkViewController()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public override string ControlPath => "~/DesktopModules/Html/";

        public override string ID => "MyWork";

        protected override object ViewModel()
        {
            var objHtmlTextUsers = new HtmlTextUserController();
            var lst = objHtmlTextUsers.GetHtmlTextUser(this.UserInfo.UserID).Cast<HtmlTextUserInfo>();
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/DesktopModules/HTML/edit.css");
            MvcClientResourceManager.RegisterStyleSheet(this.ControllerContext, "~/Portals/_default/Skins/_default/WebControlSkin/Default/GridView.default.css");
            return new MyWorkModel()
            {
                LocalResourceFile = this.LocalResourceFile,
                ModuleId = this.ModuleId,
                TabId = this.TabId,
                RedirectUrl = this.navigationManager.NavigateURL(),
                HtmlTextUsers = lst.Select(u => new HtmlTextUserModel()
                {
                    Url = this.navigationManager.NavigateURL(u.TabID),
                    ModuleID = u.ModuleID,
                    ModuleTitle = u.ModuleTitle,
                    StateName = u.StateName,
                }).ToList(),
            };
        }
    }
}
