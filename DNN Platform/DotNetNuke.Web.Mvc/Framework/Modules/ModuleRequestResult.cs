// DNN® and DotNetNuke® - http://www.DNNSoftware.com
// Copyright ©2002-2019
// by DNN Corp
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Mvc;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public class ModuleRequestResult
    {
        public ActionResult ActionResult { get; set; }

        public ControllerContext ControllerContext { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public ModuleActionCollection ModuleActions { get; set; }

        public ModuleApplication ModuleApplication { get; set; }
    }
}