// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.HttpModules
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.HttpModules.UrlRewrite;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>An HTTP module to wire up URL rewriting.</summary>
    public class UrlRewriteModule : IHttpModule
    {
        private readonly UrlRewriterBase urlRewriter;

        /// <summary>Initializes a new instance of the <see cref="UrlRewriteModule"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with UrlRewriterBase. Scheduled removal in v12.0.0.")]
        public UrlRewriteModule()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="UrlRewriteModule"/> class.</summary>
        /// <param name="urlRewriter">The URL rewriter.</param>
        public UrlRewriteModule(UrlRewriterBase urlRewriter)
        {
            this.urlRewriter = urlRewriter ?? GetUrlRewriterInstance(Globals.GetCurrentServiceProvider());
        }

        /// <summary>Gets the HttpModule module name.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ModuleName => "UrlRewriteModule";

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public void Init(HttpApplication application)
        {
            application.BeginRequest += this.urlRewriter.RewriteUrl;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>Gets an instance of the URL rewriter.</summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A <see cref="UrlRewriterBase"/> instance.</returns>
        internal static UrlRewriterBase GetUrlRewriterInstance(IServiceProvider serviceProvider)
        {
            var providerToUse = Common.Utilities.Config.GetFriendlyUrlProvider();
            var createAdvancedRewriter = providerToUse == "advanced";
            return createAdvancedRewriter
                ? ActivatorUtilities.GetServiceOrCreateInstance<AdvancedUrlRewriter>(serviceProvider)
                : ActivatorUtilities.GetServiceOrCreateInstance<BasicUrlRewriter>(serviceProvider);
        }
    }
}
