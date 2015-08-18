// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web;
using System.Web.Routing;
using System.Web.UI;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public class ModuleRequestContext
    {
        public Page DnnPage { get; set; }

        public HttpContextBase HttpContext { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public ModuleApplication ModuleApplication { get; set; }

        public RouteData RouteData { get; set; }
    }
}