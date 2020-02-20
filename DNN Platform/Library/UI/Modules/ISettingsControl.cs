// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.UI.Modules
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Namespace: DotNetNuke.UI.Modules
    /// Class	 : ISettingsControl
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// ISettingsControl provides a common Interface for Module Settings Controls
    /// </summary>
    /// -----------------------------------------------------------------------------
    public interface ISettingsControl : IModuleControl
    {
        void LoadSettings();

        void UpdateSettings();
    }
}
