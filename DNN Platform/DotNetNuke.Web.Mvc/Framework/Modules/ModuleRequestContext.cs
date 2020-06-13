// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System.Web;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.UI.Modules;

    public class ModuleRequestContext
    {
        public Page DnnPage { get; set; }

        public HttpContextBase HttpContext { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public ModuleApplication ModuleApplication { get; set; }

        public RouteData RouteData { get; set; }
    }
}
