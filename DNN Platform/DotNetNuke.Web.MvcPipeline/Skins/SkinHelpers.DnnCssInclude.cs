// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System.Web;
    using System.Web.Mvc;

    using ClientDependency.Core;
    using ClientDependency.Core.Mvc;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString DnnCssInclude(this HtmlHelper<PageModel> helper, string filePath, string pathNameAlias = "", int priority = 100, bool addTag = false, string name = "", string version = "", bool forceVersion = false, string forceProvider = "", bool forceBundle = false, string cssMedia = "")
        {
            // TODO: ClientDependency Core is deprecated and will not load.
            helper.RequiresCss(filePath, pathNameAlias, priority);            

            if (addTag || helper.ViewContext.HttpContext.IsDebuggingEnabled)
            {
                return new MvcHtmlString(string.Format("<!--CDF({0}|{1}|{2}|{3})-->", ClientDependencyType.Css, filePath, forceProvider, priority));
            }

            return new MvcHtmlString(string.Empty);
        }

        public static IHtmlString DnnCssInclude(this HtmlHelper<PageModel> helper, string bundleName, string[] filePaths, string pathNameAlias = "", int priority = 100, bool addTag = false, string name = "", string version = "", bool forceVersion = false, string forceProvider = "", bool forceBundle = false, string cssMedia = "")
        {
            // ClientDependency.Core.BundleManager.CreateCssBundle(
            //    bundleName,
            //    filePaths.Select(p => new CssFile(p) { PathNameAlias = pathNameAlias, Priority = priority }).ToArray());

            // helper.RequiresCssBundle(bundleName);
            if (addTag || helper.ViewContext.HttpContext.IsDebuggingEnabled)
            {
                return new MvcHtmlString(string.Format("<!--CDF({0}|{1}|{2}|{3})-->", ClientDependencyType.Css, string.Join(",", filePaths), forceProvider, priority));
            }

            return new MvcHtmlString(string.Empty);
        }

        public static IHtmlString DnnCssIncludeDefaultStylesheet(this HtmlHelper<PageModel> helper, string pathNameAlias = "", int priority = 100, bool addTag = false, string name = "", string version = "", bool forceVersion = false, string forceProvider = "", bool forceBundle = false, string cssMedia = "")
        {
            var filePath = string.Concat(Common.Globals.ApplicationPath, "/Resources/Shared/stylesheets/dnndefault/10.0.0/default.css");
            MvcClientResourceManager.RegisterDefaultStylesheet(helper.ViewContext, filePath);

            if (addTag || helper.ViewContext.HttpContext.IsDebuggingEnabled)
            {
                return new MvcHtmlString(string.Format("<!--CDF({0}|{1}|{2}|{3})-->", ClientDependencyType.Css, filePath, forceProvider, priority));
            }

            return new MvcHtmlString(string.Empty);
        }
    }
}
