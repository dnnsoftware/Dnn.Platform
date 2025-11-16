// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Containers
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class ModuleHelpers
    {
        public static IHtmlString LocalizeString(this HtmlHelper htmlHelper, string key, string localResourceFile)
        {
            return MvcHtmlString.Create(Localization.GetString(key, localResourceFile));
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
