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

    /// <summary>
    /// Skin helper methods for including JavaScript files via the DNN Client Dependency Framework.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Registers a JavaScript file with the DNN Client Dependency Framework.
        /// Optionally emits a CDF debug comment when running in debug mode or when <paramref name="addTag"/> is <c>true</c>.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="filePath">The JavaScript file path.</param>
        /// <param name="pathNameAlias">The path alias (for example, <c>SharedScripts</c>).</param>
        /// <param name="priority">The client dependency priority.</param>
        /// <param name="addTag">If set to <c>true</c>, emits a CDF debug comment.</param>
        /// <param name="name">Optional logical name for the script.</param>
        /// <param name="version">Optional version associated with the script.</param>
        /// <param name="forceVersion">If set to <c>true</c>, forces the specified version.</param>
        /// <param name="forceProvider">Optional provider name to force.</param>
        /// <param name="forceBundle">Unused parameter retained for API compatibility.</param>
        /// <param name="defer">If set to <c>true</c>, marks the script as deferred.</param>
        /// <returns>An empty HTML string or a CDF debug comment when requested.</returns>
        public static IHtmlString DnnJsInclude(this HtmlHelper<PageModel> helper, string filePath, string pathNameAlias = "", int priority = 100, bool addTag = false, string name = "", string version = "", bool forceVersion = false, string forceProvider = "", bool forceBundle = false, bool defer = false)
        {
            // var htmlAttibs = new { nonce = helper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString(), defer = defer ? "defer" : string.Empty };
            // todo CSP - implement nonce support
            // htmlAttibs.Add("nonce", helper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());
            var script = HtmlHelpers.GetClientResourcesController(helper)
                .CreateScript(filePath, pathNameAlias)
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
