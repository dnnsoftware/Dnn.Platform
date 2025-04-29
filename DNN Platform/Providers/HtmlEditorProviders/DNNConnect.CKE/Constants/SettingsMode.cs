// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Constants;

/// <summary>Settings Mode.</summary>
public enum SettingsMode
{
    /// <summary>The default settings mode.</summary>
    Default = -2,

    /// <summary>The host settings mode.</summary>
    Host = -1,

    /// <summary>The portal settings mode.</summary>
    Portal = 0,

    /// <summary>The page settings mode.</summary>
    Page = 1,

    /// <summary>The module instance settings mode.</summary>
    ModuleInstance = 2,
}
