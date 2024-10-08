﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    /// <summary>determine whereabouts in the page the script (and fallback script when CDN is enabled) is emitted.</summary>
    public enum ScriptLocation
    {
        PageHead = 0,
        BodyTop = 1,
        BodyBottom = 2,
    }
}
