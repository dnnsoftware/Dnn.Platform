// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class    : ISettingsControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ISettingsControl provides a common Interface for Module Settings Controls.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface ISettingsControl : IModuleControl
    {
        void LoadSettings();

        void UpdateSettings();
    }
}
