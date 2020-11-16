// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Components
{
    /// <summary>
    /// Theme controller.
    /// </summary>
    public interface IDefaultPortalThemeController
    {
        /// <summary>
        /// Returns the default current portal container.
        /// </summary>
        /// <returns></returns>
        string GetDefaultPortalContainer();

        /// <summary>
        /// Returns the default current portal layout.
        /// </summary>
        /// <returns></returns>
        string GetDefaultPortalLayout();
    }
}
