// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.UI.Modules;

    public class ModuleRequestResult
    {
        public ActionResult ActionResult { get; set; }

        public ControllerContext ControllerContext { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public ModuleActionCollection ModuleActions { get; set; }

        public ModuleApplication ModuleApplication { get; set; }
    }
}
