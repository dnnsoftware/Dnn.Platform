// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.EventListeners
{
    /// <summary>SkinEventType provides a custom enum for skin event types.</summary>
    public enum SkinEventType
    {
        /// <summary>The initialization of the skin control.</summary>
        OnSkinInit = 0,

        /// <summary>The load event of the skin control.</summary>
        OnSkinLoad = 1,

        /// <summary>The pre-render event of the skin control.</summary>
        OnSkinPreRender = 2,

        /// <summary>The unload event of the skin control.</summary>
        OnSkinUnLoad = 3,
    }
}
