// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using Microsoft.Extensions.DependencyInjection;

    public class HtmlModuleControl : ModuleControlBase, IActionable
    {
        private readonly INavigationManager navigationManager;

        // private bool editorEnabled;
        // private int workflowID;
        public HtmlModuleControl()
        {
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
            this.ControlPath = "DesktopModules/HTML";
            this.ID = "HtmlModule.ascx";
        }

        /// <summary>  Gets moduleActions is an interface property that returns the module actions collection for the module.</summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                // add the Edit Text action
                var actions = new ModuleActionCollection();
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    string.Empty,
                    this.EditUrl(/*"mvcpage", "yes"*/),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                // add mywork to action menu
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString("MyWork.Action", this.LocalResourceFile),
                    "MyWork.Action",
                    string.Empty,
                    "view.gif",
                    this.EditUrl(/*"mvcpage", "yes",*/ "MyWork"),
                    false,
                    SecurityAccessLevel.Edit,
                    true,
                    false);

                return actions;
            }
        }
    }
}
