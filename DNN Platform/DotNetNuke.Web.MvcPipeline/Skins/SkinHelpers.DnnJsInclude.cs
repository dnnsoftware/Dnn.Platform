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
    using ClientDependency.Core.Mvc;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString DnnJsInclude(this HtmlHelper<PageModel> helper, string filePath, string pathNameAlias = "", int priority = 100, bool addTag = false, string name = "", string version = "", bool forceVersion = false, string forceProvider = "", bool forceBundle = false, bool defer = false)
        {
            // var htmlAttibs = new { nonce = helper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString(), defer = defer ? "defer" : string.Empty };
            var htmlAttibs = new Dictionary<string, string>();
            //todo CSP - implement nonce support
            // htmlAttibs.Add("nonce", helper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());
            if (defer)
            {
                htmlAttibs.Add("defer", "defer");
            }

            // TODO: ClientDependency Core is deprecated
            helper.RequiresJs(filePath, pathNameAlias, priority, htmlAttibs);

            if (addTag || helper.ViewContext.HttpContext.IsDebuggingEnabled)
            {
                return new MvcHtmlString(string.Format("<!--CDF({0}|{1}|{2}|{3})-->", ClientDependencyType.Javascript, filePath, forceProvider, priority));
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
