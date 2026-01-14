// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;

    [Serializable]
    public class DiscardStateMenu : WorkflowBaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name { get; } = "DiscardState";

        /// <inheritdoc/>
        public override string Text => "Reject"; // (1) HIDDEN -> (2..last) "Reject"

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template { get; } = string.Empty;

        /// <inheritdoc/>
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader { get; } = "DiscardState";

        /// <inheritdoc/>
        public override int Order { get; } = 80;

        /// <inheritdoc/>
        public override bool Visible() => base.Visible()
            && this.IsLastState == false // not the last 'Published' state
            && this.IsFirstState == false
            && this.IsReviewOrOtherIntermediateStateWithPermissions == true;
    }
}
