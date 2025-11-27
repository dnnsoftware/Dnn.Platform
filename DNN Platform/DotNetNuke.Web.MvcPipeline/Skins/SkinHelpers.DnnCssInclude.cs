// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Mvc;

    using ClientDependency.Core;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class SkinHelpers
    {
        public static IHtmlString DnnCssInclude(this HtmlHelper<PageModel> helper, string filePath, string pathNameAlias = "", int priority = 100, bool addTag = false, string name = "", string version = "", bool forceVersion = false, string forceProvider = "", bool forceBundle = false, string cssMedia = "")
        {
            var ss = GetClientResourcesController()
                .CreateStylesheet(filePath, pathNameAlias)
                .SetPriority(priority);
            if (!string.IsNullOrEmpty(forceProvider))
            {
                ss.SetProvider(forceProvider);
            }
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(version))
            {
                ss.SetNameAndVersion(name, version, forceVersion);
            }
            if (!string.IsNullOrEmpty(cssMedia))
            {
                ss.SetMedia(cssMedia);
            }
            ss.Register();

            if (addTag || helper.ViewContext.HttpContext.IsDebuggingEnabled)
            {
                return new MvcHtmlString(string.Format("<!--CDF({0}|{1}|{2}|{3})-->", ClientDependencyType.Css, filePath, forceProvider, priority));
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
