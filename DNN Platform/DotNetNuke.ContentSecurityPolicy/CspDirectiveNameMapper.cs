// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Utility class for converting directive types to their string representations.
    /// </summary>
    public static class CspDirectiveNameMapper
    {
        /// <summary>
        /// Gets the directive name string.
        /// </summary>
        /// <param name="directiveType">The directive type to get the name for.</param>
        /// <returns>The directive name string.</returns>
        public static string GetDirectiveName(CspDirectiveType directiveType)
        {
            return directiveType switch
            {
                CspDirectiveType.DefaultSrc => "default-src",
                CspDirectiveType.ScriptSrc => "script-src",
                CspDirectiveType.StyleSrc => "style-src",
                CspDirectiveType.ImgSrc => "img-src",
                CspDirectiveType.ConnectSrc => "connect-src",
                CspDirectiveType.FontSrc => "font-src",
                CspDirectiveType.ObjectSrc => "object-src",
                CspDirectiveType.MediaSrc => "media-src",
                CspDirectiveType.FrameSrc => "frame-src",
                CspDirectiveType.BaseUri => "base-uri",
                CspDirectiveType.PluginTypes => "plugin-types",
                CspDirectiveType.SandboxDirective => "sandbox",
                CspDirectiveType.FormAction => "form-action",
                CspDirectiveType.FrameAncestors => "frame-ancestors",
                CspDirectiveType.ReportUri => "report-uri",
                CspDirectiveType.ReportTo => "report-to",
                CspDirectiveType.UpgradeInsecureRequests => "upgrade-insecure-requests",
                _ => throw new ArgumentException("Unknown directive type")
            };
        }
    }
}
