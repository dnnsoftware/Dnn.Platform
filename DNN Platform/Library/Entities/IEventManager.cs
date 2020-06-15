// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities
{
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Tabs.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem.EventArgs;

    public interface IEventManager
    {
        void OnFileAdded(FileAddedEventArgs args);

        void OnFileChanged(FileChangedEventArgs args);

        void OnFileDeleted(FileDeletedEventArgs args);

        void OnFileMetadataChanged(FileChangedEventArgs args);

        void OnFileMoved(FileMovedEventArgs args);

        void OnFileOverwritten(FileChangedEventArgs args);

        void OnFileRenamed(FileRenamedEventArgs args);

        void OnFileDownloaded(FileDownloadedEventArgs e);

        void OnFolderAdded(FolderChangedEventArgs args);

        void OnFolderDeleted(FolderDeletedEventArgs args);

        void OnFolderMoved(FolderMovedEventArgs args);

        void OnFolderRenamed(FolderRenamedEventArgs args);

        void OnFollowRequested(RelationshipEventArgs args);

        void OnFriendshipAccepted(RelationshipEventArgs args);

        void OnFriendshipDeleted(RelationshipEventArgs args);

        void OnFriendshipRequested(RelationshipEventArgs args);

        void OnModuleCreated(ModuleEventArgs args);

        void OnModuleDeleted(ModuleEventArgs args);

        void OnModuleRemoved(ModuleEventArgs args);

        void OnModuleUpdated(ModuleEventArgs args);

        void OnPortalCreated(PortalCreatedEventArgs args);

        void OnPortalSettingUpdated(PortalSettingUpdatedEventArgs args);

        void OnPortalTemplateCreated(PortalTemplateEventArgs args);

        void OnProfileUpdated(ProfileEventArgs args);

        void OnRoleCreated(RoleEventArgs args);

        void OnRoleDeleted(RoleEventArgs args);

        void OnRoleJoined(RoleEventArgs args);

        void OnRoleLeft(RoleEventArgs args);

        void OnTabCreated(TabEventArgs args);

        void OnTabDeleted(TabEventArgs args);

        void OnTabDeserialize(TabSyncEventArgs args);

        void OnTabMarkedAsPublished(TabEventArgs args);

        void OnTabRemoved(TabEventArgs args);

        void OnTabRestored(TabEventArgs args);

        void OnTabSerialize(TabSyncEventArgs args);

        void OnTabUpdated(TabEventArgs args);

        void OnUnfollowRequested(RelationshipEventArgs args);

        void OnUserApproved(UserEventArgs args);

        void OnUserAuthenticated(UserEventArgs args);

        void OnUserCreated(UserEventArgs args);

        void OnUserDeleted(UserEventArgs args);

        void OnUserRemoved(UserEventArgs args);

        void OnUserUpdated(UpdateUserEventArgs args);

        void RefreshTabSyncHandlers();
    }
}
