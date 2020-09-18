// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Model;

    public interface IMenuItemController
    {
        /// <summary>
        /// Update menu item parameters.
        /// </summary>
        /// <param name="menuItem"></param>
        void UpdateParameters(MenuItem menuItem);

        /// <summary>
        /// whether the menu item visible in current context.
        /// </summary>
        /// <param name="menuItem"></param>
        /// <returns></returns>
        bool Visible(MenuItem menuItem);

        /// <summary>
        /// get menu settings.
        /// </summary>
        /// <param name="menuItem"></param>
        /// <returns></returns>
        IDictionary<string, object> GetSettings(MenuItem menuItem);
    }
}
