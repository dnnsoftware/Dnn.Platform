// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;
    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Personalization;

    /// <summary>
    /// Menu item to exit edit mode.
    /// </summary>
    [Serializable]
    public class ExitEditModeMenu : BaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name => "ExitEditMode";

        /// <inheritdoc/>
        public override string Text => "ExitEditMode";

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template => string.Empty;

        /// <inheritdoc/>
        public override string Parent => Constants.RightMenu;

        /// <inheritdoc/>
        public override string Loader => "ExitEditMode";

        /// <inheritdoc/>
        public override int Order => 100;

        /// <inheritdoc/>
        public override bool Visible()
        {
            return Personalization.GetUserMode() == PortalSettings.Mode.Edit;
        }
    }
}
