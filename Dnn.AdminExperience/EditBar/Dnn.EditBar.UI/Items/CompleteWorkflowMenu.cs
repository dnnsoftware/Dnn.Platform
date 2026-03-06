// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;

    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;

    /// <summary>A <see cref="WorkflowBaseMenuItem"/> for the complete workflow menu item.</summary>
    /// <param name="contentController">The content controller.</param>
    /// <param name="workflowEngine">The workflow engine.</param>
    [Serializable]
    public class CompleteWorkflowMenu(IContentController contentController, IWorkflowEngine workflowEngine)
        : WorkflowBaseMenuItem(contentController, workflowEngine)
    {
        /// <summary>Initializes a new instance of the <see cref="CompleteWorkflowMenu"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
        public CompleteWorkflowMenu()
            : this(null, null)
        {
        }

        /// <inheritdoc />
        public override string Name { get; } = "CompleteWorkflow";

        /// <inheritdoc />
        public override string Text => "Publish";

        /// <inheritdoc />
        public override string CssClass => string.Empty;

        /// <inheritdoc />
        public override string Template { get; } = string.Empty;

        /// <inheritdoc />
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc />
        public override string Loader { get; } = "CompleteWorkflow";

        /// <inheritdoc />
        public override int Order { get; } = 79;

        /// <inheritdoc />
        public override bool Visible() => base.Visible()
            && !this.IsDirectPublishWorkflow
            && (this.IsReviewOrOtherIntermediateStateWithPermissions == true
                || (this.IsPriorState == true && this.IsDraftWithPermissions == true) // for Save Draft workflow
                || (this.IsLastState == true && this.HasUnpublishVersion && this.HasDraftPermission == true)); // handles other workflow scenarios (not Direct Publish)
    }
}
