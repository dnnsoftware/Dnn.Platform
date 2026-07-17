// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Controls
{
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Modules.Html;
    using DotNetNuke.Modules.Html.Models;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
    using Microsoft.Extensions.DependencyInjection;

    public class MyWorkControl : RazorModuleControlBase, IActionable, IPageContributor
    {
        private readonly INavigationManager navigationManager;

        public MyWorkControl()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public override string ControlName => "MyWork";

        public override string ResourceName => this.ControlName + ".ascx.resx";

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    string.Empty,
                    this.EditUrl(),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                return actions;
            }
        }

        public void ConfigurePage(PageConfigurationContext context)
        {
            context.ClientResourceController
                .CreateStylesheet("~/DesktopModules/HTML/edit.css")
                .Register();
            context.ClientResourceController
                .CreateStylesheet("~/Portals/_default/Skins/_default/WebControlSkin/Default/GridView.default.css")
                .Register();
        }

        public override IRazorModuleResult Invoke()
        {
            var objHtmlTextUsers = new HtmlTextUserController();
            var lst = objHtmlTextUsers.GetHtmlTextUser(this.UserInfo.UserID).Cast<HtmlTextUserInfo>();

            return this.View(new MyWorkModel()
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
            });
        }
    }
}
