// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    /// <summary>
    /// Represents different types of Content Security Policy directives.
    /// </summary>
    public enum CspDirectiveType
    {
        /// <summary>
        /// Directive that defines the default policy for unspecified resource types.
        /// </summary>
        DefaultSrc,

        /// <summary>
        /// Directive that controls authorized script sources.
        /// </summary>
        ScriptSrc,

        /// <summary>
        /// Directive that controls authorized style sources.
        /// </summary>
        StyleSrc,

        /// <summary>
        /// Directive that controls authorized image sources.
        /// </summary>
        ImgSrc,

        /// <summary>
        /// Directive that controls authorized connection destinations.
        /// </summary>
        ConnectSrc,

        /// <summary>
        /// Directive that controls authorized font sources.
        /// </summary>
        FontSrc,

        /// <summary>
        /// Directive that controls authorized object sources.
        /// </summary>
        ObjectSrc,

        /// <summary>
        /// Directive that controls authorized media sources.
        /// </summary>
        MediaSrc,

        /// <summary>
        /// Directive that controls authorized frame sources.
        /// </summary>
        FrameSrc,

        /// <summary>
        /// Directive that restricts URLs that can be used in the document's base URI.
        /// </summary>
        BaseUri,

        /// <summary>
        /// Directive that restricts the types of plugins that can be loaded.
        /// </summary>
        PluginTypes,

        /// <summary>
        /// Directive that enables a sandbox for the requested resource.
        /// </summary>
        SandboxDirective,

        /// <summary>
        /// Directive that restricts URLs that can be used as form targets.
        /// </summary>
        FormAction,

        /// <summary>
        /// Directive that specifies the parents authorized to embed a page in a frame.
        /// </summary>
        FrameAncestors,

        /// <summary>
        /// Directive that specifies the URI where violation reports should be sent.
        /// </summary>
        ReportUri,

        /// <summary>
        /// Directive that specifies where to send violation reports in JSON format.
        /// </summary>
        ReportTo,

        /// <summary>
        /// Directive that specifies UpgradeInsecureRequests.
        /// </summary>
        UpgradeInsecureRequests,
}
}
