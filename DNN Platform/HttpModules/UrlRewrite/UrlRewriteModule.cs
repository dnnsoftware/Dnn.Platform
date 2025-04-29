// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules;

using System.Web;

using DotNetNuke.Entities.Urls;
using DotNetNuke.Framework.Providers;
using DotNetNuke.HttpModules.UrlRewrite;

public class UrlRewriteModule : IHttpModule
{
    private string providerToUse;
    private UrlRewriterBase urlRewriter;

    public string ModuleName
    {
        get { return "UrlRewriteModule"; }
    }

    /// <inheritdoc/>
    public void Init(HttpApplication application)
    {
        this.providerToUse = DotNetNuke.Common.Utilities.Config.GetFriendlyUrlProvider();

        // bind events depending on currently configured friendly url provider
        // note that the current configured friendly url provider determines what type
        // of url rewriting is required.
        switch (this.providerToUse)
        {
            case "advanced":
                var advancedRewriter = new AdvancedUrlRewriter();
                this.urlRewriter = advancedRewriter;

                // bind the rewrite event to the begin request event
                application.BeginRequest += this.urlRewriter.RewriteUrl;
                break;
            default:
                var basicRewriter = new BasicUrlRewriter();
                this.urlRewriter = basicRewriter;
                application.BeginRequest += this.urlRewriter.RewriteUrl;
                break;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
