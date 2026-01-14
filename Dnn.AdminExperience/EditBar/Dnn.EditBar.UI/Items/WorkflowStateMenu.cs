// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System;
    using System.Linq;

    [Serializable]
    public class WorkflowStateMenu : WorkflowBaseMenuItem
    {
        /// <inheritdoc/>
        public override string Name { get; } = "WorkflowState";

        /// <inheritdoc/>
        public override string Text => this.Workflow != null ? $"<small>{this.Workflow.WorkflowName}:</small><br>{this.RenderStates()}" : string.Empty;

        /// <inheritdoc/>
        public override string CssClass => string.Empty;

        /// <inheritdoc/>
        public override string Template { get; } = string.Empty;

        /// <inheritdoc/>
        public override string Parent { get; } = Library.Constants.LeftMenu;

        /// <inheritdoc/>
        public override string Loader { get; } = "WorkflowState";

        /// <inheritdoc/>
        public override int Order { get; } = 77;

        // render list of workflow states from first state to current workflow state
        private string RenderStates()
        {
            if (this.WorkflowState == null)
            {
                return string.Empty;
            }

            var currentState = $"<strong>{this.WorkflowState.StateName}</strong>";
            if (this.IsFirstState == true || this.IsLastState == true)
            {
                return currentState;
            }

            // return "Draft > Review > Review2" for example
            var pastStates = this.Workflow.States.Where(s => s.Order < this.WorkflowState.Order).OrderBy(s => s.Order).ToList();
            var pastStatesHtml = string.Join(" &gt; ", pastStates.Select(s => $"{s.StateName}"));
            return string.Join(" &gt; ", pastStatesHtml, currentState);
        }
    }
}
