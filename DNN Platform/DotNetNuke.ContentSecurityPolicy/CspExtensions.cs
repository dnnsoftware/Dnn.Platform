// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
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
            csp.DefaultSource.AddSelf();
            csp.ScriptSource.AddSelf();
            csp.ScriptSource.AddInline();
            csp.ScriptSource.AddEval();
            csp.StyleSource.AddSelf();
            csp.ImgSource.AddSelf();
            csp.FontSource.AddSelf();
            csp.FrameSource.AddSelf();
            csp.FormAction.AddSelf();
            csp.ConnectSource.AddSelf();
            csp.BaseUriSource.AddSelf();
        }
    }
}
