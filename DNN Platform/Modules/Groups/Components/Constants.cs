// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    public class Constants
    {
        internal const string DefaultRoleGroupSetting = "DefaultRoleGroup_Setting";
        internal const string DefautlGroupViewMode = "DefaultGroupViewMode_Setting";
        internal const string GroupViewPage = "GroupViewPage_Setting";
        internal const string GroupListPage = "GroupListPage_Setting";
        internal const string GroupLoadView = "GroupLoadView_Setting";
        internal const string GroupListPageSize = "GroupListPageSize_Setting";

        internal const string GroupListUserGroupsOnly = "GroupListUserGroupsOnly_Setting";
        internal const string GroupListSearchEnabled = "GroupListSearchEnabled_Setting";
        internal const string GroupListSortField = "GroupListSortField_Setting";
        internal const string GroupListSortDirection = "GroupListSortDirection_Setting";

        internal const string GroupViewTemplate = "GroupViewTemplate_Setting";
        internal const string GroupListTemplate = "GroupListTemplate_Setting";
        internal const string GroupModerationEnabled = "GroupModerationEnabled_Setting";

        internal const string GroupPendingNotification = "GroupPendingNotification";  // Sent to Moderators when a group is created and moderation is enabled.
        internal const string GroupApprovedNotification = "GroupApprovedNotification";  // Sent to group creator when group is approved.
        internal const string GroupCreatedNotification = "GroupCreatedNotification"; // Sent to Admins/Moderators when a new group is created and moderation is disabled.
        internal const string GroupRejectedNotification = "GroupRejectedNotification"; // Sent to group creator when a group is rejected.

        internal const string MemberPendingNotification = "GroupMemberPendingNotification";  // Sent to Group Owners when a new member has requested access to a private group.
        internal const string MemberApprovedNotification = "GroupMemberApprovedNotification"; // Sent to Member when membership is approved.
        internal const string MemberJoinedNotification = "MemberJoinedNotification"; // Sent to Group Owners when a new member has joined a public group.
        internal const string MemberRejectedNotification = "GroupMemberRejectedNotification"; // Sent to requesting member when membership is rejected.

        internal const string SharedResourcesPath = "~/DesktopModules/SocialGroups/App_LocalResources/SharedResources.resx";
        internal const string ModulePath = "~/DesktopModules/SocialGroups/";

        internal const string DefaultGroupName = "Social Groups";
    }
}
