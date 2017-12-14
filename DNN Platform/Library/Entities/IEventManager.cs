// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem.EventArgs;

namespace DotNetNuke.Entities
{
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
