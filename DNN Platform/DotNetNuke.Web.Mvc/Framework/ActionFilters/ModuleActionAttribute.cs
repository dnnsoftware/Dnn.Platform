// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    /// <summary>
    /// The ModuleActionAttribute is used to define a single ModuleAction.  It can be applied at the Controller level if all
    /// Action methods should have the same ModuleAction, but it is more likely to be used at the Action method level.
    /// </summary>
    public class ModuleActionAttribute : ActionFilterAttribute
    {
        public ModuleActionAttribute()
        {
            this.SecurityAccessLevel = SecurityAccessLevel.Edit;
        }

        /// <summary>
        /// Gets or sets the ControlKey property is the key for the module control.
        /// </summary>
        public string ControlKey { get; set; }

        /// <summary>
        /// Gets or sets the Icon property is the url for the Icon to be used in the Module Actions menu.  An empty string will mean that the Edit "pencil" icon is used.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the SecurityAccessLevel is an enum property that sets the security level for the module action.
        /// </summary>
        public SecurityAccessLevel SecurityAccessLevel { get; set; }

        /// <summary>
        /// Gets or sets the Title property is the title for the Module Action.  This property is only used if the TitleKey is left blank.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the TitleKey property is the localization key for the title for the Module Action.
        /// </summary>
        public string TitleKey { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            if (controller.ModuleActions == null)
            {
                controller.ModuleActions = new ModuleActionCollection();
            }

            controller.ModuleActions.Add(
                -1,
                (!string.IsNullOrEmpty(this.TitleKey)) ? controller.LocalizeString(this.TitleKey) : this.Title,
                ModuleActionType.AddContent,
                string.Empty,
                this.Icon,
                controller.ModuleContext.EditUrl(this.ControlKey),
                false,
                this.SecurityAccessLevel,
                true,
                false);
        }
    }
}
