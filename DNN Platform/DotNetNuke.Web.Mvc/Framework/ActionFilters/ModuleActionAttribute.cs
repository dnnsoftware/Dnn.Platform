#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DNN Corporation
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
using System.Web.Mvc;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    /// <summary>
    /// The ModuleActionAttribute is used to define a single ModuleAction.  It can be applied at the Controller level if all 
    /// Action methods should have the same ModuleAction, but it is more likely to be used at the Action method level.
    /// </summary>
    public class ModuleActionAttribute  : ActionFilterAttribute
    {
        public ModuleActionAttribute()
        {
            SecurityAccessLevel = SecurityAccessLevel.Edit;
        }

        /// <summary>
        /// The ControlKey property is the key for the module control
        /// </summary>
        public string ControlKey { get; set; }

        /// <summary>
        /// The Icon property is the url for the Icon to be used in the Module Actions menu.  An empty string will mean that the Edit "pencil" icon is used.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The SecurityAccessLevel is an enum property that sets the security level for the module action.
        /// </summary>
        public SecurityAccessLevel SecurityAccessLevel { get; set; }

        /// <summary>
        /// The Title property is the title for the Module Action.  This property is only used if the TitleKey is left blank.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The TitleKey property is the localization key for the title for the Module Action.
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

            controller.ModuleActions.Add(-1,
                                (!String.IsNullOrEmpty(TitleKey)) ? controller.LocalizeString(TitleKey) : Title,
                                ModuleActionType.AddContent,
                                "",
                                Icon,
                                controller.ModuleContext.EditUrl(ControlKey),
                                false,
                                SecurityAccessLevel,
                                true,
                                false);
        }
    }
}
