// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;

    [Serializable]
    public class CompleteWorkflowMenu : WorkflowBaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name { get; } = "CompleteWorkflow";

        /// <inheritdoc/>
        public override string Text => "Publish";

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template { get; } = string.Empty;

        /// <inheritdoc/>
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader { get; } = "CompleteWorkflow";

        /// <inheritdoc/>
        public override int Order { get; } = 79;

        /// <inheritdoc/>
        public override bool Visible() => base.Visible()
            && !this.IsDirectPublishWorkflow
            && (this.IsReviewOrOtherIntermediateStateWithPermissions == true
                || (this.IsPriorState == true && this.IsDraftWithPermissions == true) // for Save Draft workflow
                || (this.IsLastState == true && this.HasUnpublishVersion && this.HasDraftPermission == true)); // handles other workflow scenarios (not Direct Publish)
    }
}
