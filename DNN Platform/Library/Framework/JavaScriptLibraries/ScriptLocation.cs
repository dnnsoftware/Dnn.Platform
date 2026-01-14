// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    /// <summary>determine whereabouts in the page the script (and fallback script when CDN is enabled) is emitted.</summary>
    public enum ScriptLocation
    {
        /// <summary>Add near the bottom of the page's <c>head</c> element.</summary>
        PageHead = 0,

        /// <summary>Add near the top of the page's <c>body</c> element.</summary>
        BodyTop = 1,

        /// <summary>Add near the bottom of the page's <c>body</c> element.</summary>
        BodyBottom = 2,
    }
}
