// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Controls
{
    using System.Collections.Generic;
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
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Razor;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Resources;
    using Microsoft.Extensions.DependencyInjection;

    public class MyWorkControl : RazorModuleControlBase, IActionable, IResourcable
    {
        private readonly INavigationManager navigationManager;

        public MyWorkControl()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

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

        public ModuleResources ModuleResources
        {
            get
            {
                return new ModuleResources()
                {
                    StyleSheets = new List<ModuleStyleSheet>()
                    {
                        new ModuleStyleSheet()
                        {
                            FilePath = "~/DesktopModules/HTML/edit.css",
                        },
                        new ModuleStyleSheet()
                        {
                            FilePath = "~/Portals/_default/Skins/_default/WebControlSkin/Default/GridView.default.css",
                        },
                    },
                };
            }
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
