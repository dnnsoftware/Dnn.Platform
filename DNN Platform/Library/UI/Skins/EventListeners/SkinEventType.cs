// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.EventListeners;

/// <summary>SkinEventType provides a custom enum for skin event types.</summary>
public enum SkinEventType
{
    OnSkinInit = 0,
    OnSkinLoad = 1,
    OnSkinPreRender = 2,
    OnSkinUnLoad = 3,
}
