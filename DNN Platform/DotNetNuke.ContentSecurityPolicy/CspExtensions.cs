// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Provides extension methods for <see cref="IContentSecurityPolicy"/>.
    /// </summary>
    public static class CspExtensions
    {
        /// <summary>
        /// Adds script source directives to the specified <see cref="IContentSecurityPolicy"/> instance
        /// to enable support for WebForms, including 'self', 'unsafe-inline', and 'unsafe-eval'.
        /// </summary>
        /// <param name="csp">The content security policy to modify.</param>
        public static void AddWebformsSupport(this IContentSecurityPolicy csp)
        {
            csp.AddBaseSupport();
            csp.ScriptSource.AddInline();
            csp.ScriptSource.AddEval();
        }

        /// <summary>
        /// Adds script source directives to the specified <see cref="IContentSecurityPolicy"/> instance
        /// to enable support for MVC pipeline.
        /// </summary>
        /// <param name="csp">The content security policy to modify.</param>
        public static void AddMVCSupport(this IContentSecurityPolicy csp)
        {
            csp.AddBaseSupport();
            csp.ScriptSource.AddNonce(csp.Nonce);
        }

        /// <summary>
        /// Add host to source if absolute url <see cref="SourceCspContributor"/> instance.
        /// </summary>
        /// <param name="source">The source where to add the host.</param>
        /// /// <param name="url">The url.</param>
        public static void AddUrlOrigin(this SourceCspContributor source, string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var origin = uri.GetLeftPart(UriPartial.Authority);
                source.AddHost(origin);
            }
        }

        private static void AddBaseSupport(this IContentSecurityPolicy csp)
        {
            csp.DefaultSource.AddSelf();
            csp.ScriptSource.AddSelf();
            csp.StyleSource.AddSelf();
            csp.ImgSource.AddSelf();
            csp.FontSource.AddSelf();
            csp.FrameSource.AddSelf();
            csp.FormAction.AddSelf();
            csp.ConnectSource.AddSelf();
            csp.FrameAncestors.AddSelf();
            csp.ObjectSource.AddNone();
            csp.BaseUriSource.AddSelf();
        }
    }
}
