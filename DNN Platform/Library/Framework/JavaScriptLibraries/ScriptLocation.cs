// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Framework.JavaScriptLibraries
{
    /// <summary>
    /// determine whereabouts in the page the script (and fallback script when CDN is enabled) is emitted
    /// </summary>
    public enum ScriptLocation
    {
        PageHead,
        BodyTop,
        BodyBottom
    }
}
