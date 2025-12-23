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

        /// <summary>
        /// Gets the directive type from a directive name string.
        /// </summary>
        /// <param name="directiveName">The directive name to get the type for.</param>
        /// <returns>The directive type.</returns>
        /// <exception cref="ArgumentException">Thrown when the directive name is unknown.</exception>
        public static CspDirectiveType GetDirectiveType(string directiveName)
        {
            if (string.IsNullOrWhiteSpace(directiveName))
            {
                throw new ArgumentException("Directive name cannot be null or empty", nameof(directiveName));
            }

            return directiveName.ToLowerInvariant() switch
            {
                "default-src" => CspDirectiveType.DefaultSrc,
                "script-src" => CspDirectiveType.ScriptSrc,
                "style-src" => CspDirectiveType.StyleSrc,
                "img-src" => CspDirectiveType.ImgSrc,
                "connect-src" => CspDirectiveType.ConnectSrc,
                "font-src" => CspDirectiveType.FontSrc,
                "object-src" => CspDirectiveType.ObjectSrc,
                "media-src" => CspDirectiveType.MediaSrc,
                "frame-src" => CspDirectiveType.FrameSrc,
                "base-uri" => CspDirectiveType.BaseUri,
                "plugin-types" => CspDirectiveType.PluginTypes,
                "sandbox" => CspDirectiveType.SandboxDirective,
                "form-action" => CspDirectiveType.FormAction,
                "frame-ancestors" => CspDirectiveType.FrameAncestors,
                "report-uri" => CspDirectiveType.ReportUri,
                "report-to" => CspDirectiveType.ReportTo,
                "upgrade-insecure-requests" => CspDirectiveType.UpgradeInsecureRequests,
                _ => throw new ArgumentException($"Unknown directive name: {directiveName}")
            };
        }

        /// <summary>
        /// Tries to get the directive type from a directive name string.
        /// </summary>
        /// <param name="directiveName">The directive name to get the type for.</param>
        /// <param name="directiveType">The directive type, or default if parsing failed.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryGetDirectiveType(string directiveName, out CspDirectiveType directiveType)
        {
            directiveType = default;

            try
            {
                directiveType = GetDirectiveType(directiveName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
