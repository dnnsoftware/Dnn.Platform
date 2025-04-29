// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities;

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem.EventArgs;

/// <summary>Represents the events of the EventManager.</summary>
public interface IEventManager
{
    /// <summary>Fires up when a file was added.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFileAdded(FileAddedEventArgs args);

    /// <summary>Fires up when a file was changed.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFileChanged(FileChangedEventArgs args);

    /// <summary>Fires up when a file was deleted.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFileDeleted(FileDeletedEventArgs args);

    /// <summary>Fires up when a file metadata was changed.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFileMetadataChanged(FileChangedEventArgs args);

    /// <summary>Fires up when a file was moved.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFileMoved(FileMovedEventArgs args);

    /// <summary>Fires up when a file was overwritten.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFileOverwritten(FileChangedEventArgs args);

    /// <summary>Fires up when a file was renamed.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFileRenamed(FileRenamedEventArgs args);

    /// <summary>Fires up when a file was downloaded.</summary>
    /// <param name="e">The event arguments.</param>
    void OnFileDownloaded(FileDownloadedEventArgs e);

    /// <summary>Fires up when a folder was added.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFolderAdded(FolderChangedEventArgs args);

    /// <summary>Fires up when a folder was deleted.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFolderDeleted(FolderDeletedEventArgs args);

    /// <summary>Fires up when a folder was moved.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFolderMoved(FolderMovedEventArgs args);

    /// <summary>Fires up when a folder was renamed.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFolderRenamed(FolderRenamedEventArgs args);

    /// <summary>Fires up when a user requested to follow another user.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFollowRequested(RelationshipEventArgs args);

    /// <summary>Fires up when a user has accepted a friendship request.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFriendshipAccepted(RelationshipEventArgs args);

    /// <summary>Fires up when a user deleted a friendship.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFriendshipDeleted(RelationshipEventArgs args);

    /// <summary>Fires up when a user has requested a friendship.</summary>
    /// <param name="args">The event arguments.</param>
    void OnFriendshipRequested(RelationshipEventArgs args);

    /// <summary>Fires up when a module was created.</summary>
    /// <param name="args">The event arguments.</param>
    void OnModuleCreated(ModuleEventArgs args);

    /// <summary>Fires up when a module was deleted.</summary>
    /// <param name="args">The event arguments.</param>
    void OnModuleDeleted(ModuleEventArgs args);

    /// <summary>Fires up when a module was removed.</summary>
    /// <param name="args">The event arguments.</param>
    void OnModuleRemoved(ModuleEventArgs args);

    /// <summary>Fires up when a module was updated.</summary>
    /// <param name="args">The event arguments.</param>
    void OnModuleUpdated(ModuleEventArgs args);

    /// <summary>Fires up when a portal was created.</summary>
    /// <param name="args">The event arguments.</param>
    void OnPortalCreated(PortalCreatedEventArgs args);

    /// <summary>Fires up when a portal setting was updated.</summary>
    /// <param name="args">The event arguments.</param>
    void OnPortalSettingUpdated(PortalSettingUpdatedEventArgs args);

    /// <summary>Fires up when a portal template was created.</summary>
    /// <param name="args">The event arguments.</param>
    void OnPortalTemplateCreated(PortalTemplateEventArgs args);

    /// <summary>Fires up when a user profile was updated.</summary>
    /// <param name="args">The event arguments.</param>
    void OnProfileUpdated(ProfileEventArgs args);

    /// <summary>Fires up when a role was created.</summary>
    /// <param name="args">The event arguments.</param>
    void OnRoleCreated(RoleEventArgs args);

    /// <summary>Fires up when a role was deleted.</summary>
    /// <param name="args">The event arguments.</param>
    void OnRoleDeleted(RoleEventArgs args);

    /// <summary>Fires up when a role was joined.</summary>
    /// <param name="args">The event arguments.</param>
    void OnRoleJoined(RoleEventArgs args);

    /// <summary>Fires up when a role was left.</summary>
    /// <param name="args">The event arguments.</param>
    void OnRoleLeft(RoleEventArgs args);

    /// <summary>Fires up when a page (tab) was created.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabCreated(TabEventArgs args);

    /// <summary>Fires up when a page (tab) was deleted.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabDeleted(TabEventArgs args);

    /// <summary>Fires up when a page (tab) is deserialized.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabDeserialize(TabSyncEventArgs args);

    /// <summary>Fires up when a page (tab) was marked as published.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabMarkedAsPublished(TabEventArgs args);

    /// <summary>Fires up when a page (tab) was removed.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabRemoved(TabEventArgs args);

    /// <summary>Fires up when a page (tab) was restored.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabRestored(TabEventArgs args);

    /// <summary>Fires up when a page (tab) is serialized.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabSerialize(TabSyncEventArgs args);

    /// <summary>Fires up when a page (tab) was updated.</summary>
    /// <param name="args">The event arguments.</param>
    void OnTabUpdated(TabEventArgs args);

    /// <summary>Fires up when a user as requested to unfollow another user.</summary>
    /// <param name="args">The event arguments.</param>
    void OnUnfollowRequested(RelationshipEventArgs args);

    /// <summary>Fires up when a user was approved.</summary>
    /// <param name="args">The event arguments.</param>
    void OnUserApproved(UserEventArgs args);

    /// <summary>Fires up when a user was authenticated.</summary>
    /// <param name="args">The event arguments.</param>
    void OnUserAuthenticated(UserEventArgs args);

    /// <summary>Fires up when a user was created.</summary>
    /// <param name="args">The event arguments.</param>
    void OnUserCreated(UserEventArgs args);

    /// <summary>Fires up when a user was deleted.</summary>
    /// <param name="args">The event arguments.</param>
    void OnUserDeleted(UserEventArgs args);

    /// <summary>Fires up when a user was removed.</summary>
    /// <param name="args">The event arguments.</param>
    void OnUserRemoved(UserEventArgs args);

    /// <summary>Fires up when a user was updated.</summary>
    /// <param name="args">The event arguments.</param>
    void OnUserUpdated(UpdateUserEventArgs args);

    /// <summary>Refreshes the page (tab) synchronization handlers.</summary>
    void RefreshTabSyncHandlers();
}
