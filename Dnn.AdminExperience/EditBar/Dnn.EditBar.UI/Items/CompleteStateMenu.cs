// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;

    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;

    /// <summary>A <see cref="WorkflowBaseMenuItem"/> for the complete state menu item.</summary>
    /// <param name="contentController">The content controller.</param>
    /// <param name="workflowEngine">The workflow engine.</param>
    [Serializable]
    public class CompleteStateMenu(IContentController contentController, IWorkflowEngine workflowEngine)
        : WorkflowBaseMenuItem(contentController, workflowEngine)
    {
        /// <summary>Initializes a new instance of the <see cref="CompleteStateMenu"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
        public CompleteStateMenu()
            : this(null, null)
        {
        }

        /// <inheritdoc />
        public override string Name { get; } = "CompleteState";

        /// <inheritdoc />
        public override string Text => this.IsWorkflowEnabled ? (this.IsFirstState == true ? "Submit" : "Approve") : string.Empty; // (1) "Submit" -> (2..last-2) "Approve" -> (last) HIDDEN

        /// <inheritdoc />
        public override string CssClass => string.Empty;

        /// <inheritdoc />
        public override string Template { get; } = string.Empty;

        /// <inheritdoc />
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc />
        public override string Loader { get; } = "CompleteState";

        /// <inheritdoc />
        public override int Order { get; } = 78;

        /// <inheritdoc />
        public override bool Visible() => base.Visible()
            && this.IsLastState == false // not the last 'Published' state
            && this.IsPriorState == false // not the state that is before last 'Published' state
            && (this.IsDraftWithPermissions == true || this.IsReviewOrOtherIntermediateStateWithPermissions == true);
    }
}
