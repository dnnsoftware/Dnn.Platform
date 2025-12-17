// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI.Items
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Dnn.EditBar.Library.Items;
    using DotNetNuke.Application;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Personalization;

    public abstract class WorkflowBaseMenuItem : BaseMenuItem
    {
        private readonly IWorkflowEngine workflowEngine = WorkflowEngine.Instance;

        internal Workflow Workflow => this.WorkflowState != null ? WorkflowManager.Instance.GetWorkflow(this.WorkflowState.WorkflowID) : null;

        internal WorkflowState WorkflowState => this.IsWorkflowEnabled ? WorkflowStateManager.Instance.GetWorkflowState(ContentItem.StateID) : null;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool IsEditMode => Personalization.GetUserMode() == PortalSettings.Mode.Edit;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool IsPlatform => DotNetNukeContext.Current.Application.SKU == "DNN";

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool IsWorkflowEnabled => IsVersioningEnabled && TabWorkflowSettings.Instance.IsWorkflowEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID);

        internal bool? IsFirstState => this.WorkflowState != null ? this.WorkflowState.StateName == this.Workflow.FirstState.StateName : null; // 'Draft'

        internal bool? IsPriorState => this.WorkflowState != null ? this.WorkflowState.StateName == this.PriorState?.StateName : null;

        internal bool? IsLastState => this.WorkflowState != null ? this.WorkflowState.StateName == this.Workflow.LastState.StateName : null; // 'Published'

        internal bool? IsDraftWithPermissions => this.IsWorkflowEnabled ? this.workflowEngine.IsWorkflowOnDraft(ContentItem) && this.HasDraftPermission : null;

        internal bool? IsReviewOrOtherIntermediateStateWithPermissions => this.IsWorkflowEnabled ? !this.workflowEngine.IsWorkflowCompleted(ContentItem) && WorkflowSecurity.Instance.HasStateReviewerPermission(ContentItem.StateID) : null;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool IsWorkflowCompleted => WorkflowEngine.Instance.IsWorkflowCompleted(TabController.CurrentPage.ContentItemId);

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool HasBeenPublished => TabController.CurrentPage.HasBeenPublished;

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool HasUnpublishVersion => TabVersionBuilder.Instance.GetUnPublishedVersion(TabController.CurrentPage.TabID) != null;

        internal bool HasUnpublishVersionWithPermissions => this.HasUnpublishVersion && this.IsWorkflowEnabled && WorkflowSecurity.Instance.HasStateReviewerPermission(ContentItem.StateID);

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        internal bool HasDraftPermission => PermissionProvider.Instance().CanAddContentToPage(TabController.CurrentPage);

        private static ContentItem ContentItem => Util.GetContentController().GetContentItem(TabController.CurrentPage.ContentItemId);

        private static bool IsVersioningEnabled => TabVersionSettings.Instance.IsVersioningEnabled(PortalSettings.Current.PortalId, TabController.CurrentPage.TabID);

        // State before the last one.
        private WorkflowState PriorState => this.IsWorkflowEnabled ? this.Workflow.States == null || !this.Workflow.States.Any() ? null : this.Workflow.States.OrderBy(s => s.Order).Reverse().Skip(1).FirstOrDefault() : null;

        public override bool Visible() =>
            this.IsEditMode
            && this.IsPlatform
            && this.IsWorkflowEnabled;
    }
}
