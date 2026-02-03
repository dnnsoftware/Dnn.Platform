// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;

    using Dnn.EditBar.Library;

    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;

    [Serializable]
    public class DiscardStateMenu : WorkflowBaseMenuItem
    {
        /// <summary>Initializes a new instance of the <see cref="DiscardStateMenu"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.3. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
        public DiscardStateMenu()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DiscardStateMenu"/> class.</summary>
        /// <param name="contentController">The content controller.</param>
        /// <param name="workflowEngine">The workflow engine.</param>
        public DiscardStateMenu(IContentController contentController, IWorkflowEngine workflowEngine)
            : base(contentController, workflowEngine)
        {
        }

        /// <inheritdoc />
        public override string Name { get; } = "DiscardState";

        /// <inheritdoc />
        public override string Text => "Reject"; // (1) HIDDEN -> (2..last) "Reject"

        /// <inheritdoc />
        public override string CssClass => string.Empty;

        /// <inheritdoc />
        public override string Template { get; } = string.Empty;

        /// <inheritdoc />
        public override string Parent { get; } = Constants.LeftMenu;

        /// <inheritdoc />
        public override string Loader { get; } = "DiscardState";

        /// <inheritdoc />
        public override int Order { get; } = 80;

        /// <inheritdoc />
        public override bool Visible() => base.Visible()
            && this.IsLastState == false // not the last 'Published' state
            && this.IsFirstState == false
            && this.IsReviewOrOtherIntermediateStateWithPermissions == true;
    }
}
