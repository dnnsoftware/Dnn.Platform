// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Collections.Generic;
    using System.Security.Policy;
    using System.Web;
    using System.Web.Mvc;

    using ClientDependency.Core;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString DnnJsInclude(this HtmlHelper<PageModel> helper, string filePath, string pathNameAlias = "", int priority = 100, bool addTag = false, string name = "", string version = "", bool forceVersion = false, string forceProvider = "", bool forceBundle = false, bool defer = false)
        {
            // var htmlAttibs = new { nonce = helper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString(), defer = defer ? "defer" : string.Empty };
            //todo CSP - implement nonce support
            // htmlAttibs.Add("nonce", helper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());

            var script = GetClientResourcesController().CreateScript(filePath, pathNameAlias)
                .SetPriority(priority);
            if (!string.IsNullOrEmpty(forceProvider))
            {
                script.SetProvider(forceProvider);
            }
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(version))
            {
                script.SetNameAndVersion(name, version, forceVersion);
            }
            if (defer)
            {
                script.SetDefer();
            }
            script.Register();

            if (addTag || helper.ViewContext.HttpContext.IsDebuggingEnabled)
            {
                return new MvcHtmlString(string.Format("<!--CDF({0}|{1}|{2}|{3})-->", ClientDependencyType.Javascript, filePath, forceProvider, priority));
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
