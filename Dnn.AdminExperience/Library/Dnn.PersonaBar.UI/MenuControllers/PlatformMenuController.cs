// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.MenuControllers
{
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Controllers;
    using Dnn.PersonaBar.Library.Model;
    using DotNetNuke.Application;

    /// <summary>The <see cref="IMenuItemController"/> for the DNN Platform logout action.</summary>
    public class PlatformMenuController : IMenuItemController
    {
        /// <inheritdoc/>
        public void UpdateParameters(MenuItem menuItem)
        {
        }

        /// <inheritdoc/>
        public bool Visible(MenuItem menuItem)
        {
            return DotNetNukeContext.Current.Application.SKU == "DNN";
        }

        /// <inheritdoc/>
        public IDictionary<string, object> GetSettings(MenuItem menuItem)
        {
            return null;
        }
    }
}
