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

using System;
using DotNetNuke.Common.Internal;
using DotNetNuke.Entities.Friends;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.EventArgs;

// ReSharper disable UseNullPropagation

namespace DotNetNuke.Entities
{
    public class EventManager : ServiceLocator<IEventManager, EventManager>, IEventManager
    {
        private event EventHandler<FileAddedEventArgs> FileAdded;
        private event EventHandler<FileChangedEventArgs> FileChanged;
        private event EventHandler<FileDeletedEventArgs> FileDeleted;
        private event EventHandler<FileChangedEventArgs> FileMetadataChanged;
        private event EventHandler<FileMovedEventArgs> FileMoved;
        private event EventHandler<FileChangedEventArgs> FileOverwritten;
        private event EventHandler<FileRenamedEventArgs> FileRenamed;
        private event EventHandler<FileDownloadedEventArgs> FileDownloaded; 

        private event EventHandler<FolderChangedEventArgs> FolderAdded;
        private event EventHandler<FolderDeletedEventArgs> FolderDeleted;
        private event EventHandler<FolderMovedEventArgs> FolderMoved;
        private event EventHandler<FolderRenamedEventArgs> FolderRenamed;

        private event EventHandler<RelationshipEventArgs> FollowRequested;
        private event EventHandler<RelationshipEventArgs> UnfollowRequested;

        private event EventHandler<RelationshipEventArgs> FriendshipAccepted;
        private event EventHandler<RelationshipEventArgs> FriendshipDeleted;
        private event EventHandler<RelationshipEventArgs> FriendshipRequested;

        private event EventHandler<ModuleEventArgs> ModuleCreated;
        private event EventHandler<ModuleEventArgs> ModuleUpdated;
        private event EventHandler<ModuleEventArgs> ModuleRemoved; // soft delete
        private event EventHandler<ModuleEventArgs> ModuleDeleted; // hard delete

        private event EventHandler<PortalCreatedEventArgs> PortalCreated;
        private event EventHandler<PortalTemplateEventArgs> PortalTemplateCreated;

        private event EventHandler<ProfileEventArgs> ProfileUpdated;

        private event EventHandler<RoleEventArgs> RoleCreated;
        private event EventHandler<RoleEventArgs> RoleDeleted;
        private event EventHandler<RoleEventArgs> RoleJoined;
        private event EventHandler<RoleEventArgs> RoleLeft;

        private event EventHandler<TabEventArgs> TabCreated;
        private event EventHandler<TabEventArgs> TabUpdated;
        private event EventHandler<TabEventArgs> TabRemoved; // soft delete
        private event EventHandler<TabEventArgs> TabDeleted; // hard delete
        private event EventHandler<TabEventArgs> TabRestored;
        private event EventHandler<TabEventArgs> TabMarkedAsPublished;

        private event EventHandler<TabSyncEventArgs> TabSerialize; // soft delete
        private event EventHandler<TabSyncEventArgs> TabDeserialize; // hard delete

        private event EventHandler<UserEventArgs> UserApproved;
        private event EventHandler<UserEventArgs> UserAuthenticated;
        private event EventHandler<UserEventArgs> UserCreated;
        private event EventHandler<UserEventArgs> UserDeleted;
        private event EventHandler<UserEventArgs> UserRemoved;
        private event EventHandler<UpdateUserEventArgs> UserUpdated;


        public EventManager()
        {
            foreach (var handler in EventHandlersContainer<IFileEventHandlers>.Instance.EventHandlers)
            {
                FileChanged += handler.Value.FileOverwritten;
                FileDeleted += handler.Value.FileDeleted;
                FileRenamed += handler.Value.FileRenamed;
                FileMoved += handler.Value.FileMoved;
                FileAdded += handler.Value.FileAdded;
                FileOverwritten += handler.Value.FileOverwritten;
                FileMetadataChanged += handler.Value.FileMetadataChanged;
                FileDownloaded += handler.Value.FileDownloaded;

                FolderAdded += handler.Value.FolderAdded;
                FolderDeleted += handler.Value.FolderDeleted;
                FolderMoved += handler.Value.FolderMoved;
                FolderRenamed += handler.Value.FolderRenamed;
            }

            foreach (var handler in EventHandlersContainer<IFollowerEventHandlers>.Instance.EventHandlers)
            {
                FollowRequested += handler.Value.FollowRequested;
                UnfollowRequested += handler.Value.UnfollowRequested;
            }

            foreach (var handlers in EventHandlersContainer<IFriendshipEventHandlers>.Instance.EventHandlers)
            {
                FriendshipRequested += handlers.Value.FriendshipRequested;
                FriendshipAccepted += handlers.Value.FriendshipAccepted;
                FriendshipDeleted += handlers.Value.FriendshipDeleted;
            }

            foreach (var handlers in EventHandlersContainer<IModuleEventHandler>.Instance.EventHandlers)
            {
                ModuleCreated += handlers.Value.ModuleCreated;
                ModuleUpdated += handlers.Value.ModuleUpdated;
                ModuleRemoved += handlers.Value.ModuleRemoved;
                ModuleDeleted += handlers.Value.ModuleDeleted;
            }

            foreach (var handler in EventHandlersContainer<IPortalEventHandlers>.Instance.EventHandlers)
            {
                PortalCreated += handler.Value.PortalCreated;
            }

            foreach (var handler in EventHandlersContainer<IPortalTemplateEventHandlers>.Instance.EventHandlers)
            {
                PortalTemplateCreated += handler.Value.TemplateCreated;
            }

            foreach (var handler in EventHandlersContainer<IProfileEventHandlers>.Instance.EventHandlers)
            {
                ProfileUpdated += handler.Value.ProfileUpdated;
            }

            foreach (var handler in EventHandlersContainer<IRoleEventHandlers>.Instance.EventHandlers)
            {
                RoleCreated += handler.Value.RoleCreated;
                RoleDeleted += handler.Value.RoleDeleted;
                RoleJoined += handler.Value.RoleJoined;
                RoleLeft += handler.Value.RoleLeft;
            }

            foreach (var handler in EventHandlersContainer<ITabEventHandler>.Instance.EventHandlers)
            {
                TabCreated += handler.Value.TabCreated;
                TabUpdated += handler.Value.TabUpdated;
                TabRemoved += handler.Value.TabRemoved;
                TabDeleted += handler.Value.TabDeleted;
                TabRestored += handler.Value.TabRestored;
                TabMarkedAsPublished += handler.Value.TabMarkedAsPublished;
            }

            foreach (var handler in EventHandlersContainer<ITabSyncEventHandler>.Instance.EventHandlers)
            {
                TabSerialize += handler.Value.TabSerialize;
                TabDeserialize += handler.Value.TabDeserialize;
            }

            foreach (var handler in EventHandlersContainer<IUserEventHandlers>.Instance.EventHandlers)
            {
                UserAuthenticated += handler.Value.UserAuthenticated;
                UserCreated += handler.Value.UserCreated;
                UserDeleted += handler.Value.UserDeleted;
                UserRemoved += handler.Value.UserRemoved;
                UserApproved += handler.Value.UserApproved;
                UserUpdated += handler.Value.UserUpdated;
            }

        }

        protected override Func<IEventManager> GetFactory()
        {
            return () => new EventManager();
        }

        public virtual void OnFileAdded(FileAddedEventArgs args)
        {
            if (FileAdded != null)
            {
                FileAdded(this, args);
            }
        }

        public virtual void OnFileChanged(FileChangedEventArgs args)
        {
            if (FileChanged != null)
            {
                FileChanged(this, args);
            }
        }

        public virtual void OnFileDeleted(FileDeletedEventArgs args)
        {
            if (FileDeleted != null)
            {
                FileDeleted(this, args);
            }
        }

        public virtual void OnFileMetadataChanged(FileChangedEventArgs args)
        {
            if (FileMetadataChanged != null)
            {
                FileMetadataChanged(this, args);
            }
        }

        public virtual void OnFileDownloaded(FileDownloadedEventArgs args)
        {
            if (FileDownloaded != null)
            {
                FileDownloaded(this, args);
            }
        }

        public virtual void OnFileMoved(FileMovedEventArgs args)
        {
            if (FileMoved != null)
            {
                FileMoved(this, args);
            }
        }

        public virtual void OnFileOverwritten(FileChangedEventArgs args)
        {
            if (FileOverwritten != null)
            {
                FileOverwritten(this, args);
            }
        }

        public virtual void OnFileRenamed(FileRenamedEventArgs args)
        {
            if (FileRenamed != null)
            {
                FileRenamed(this, args);
            }
        }

        public virtual void OnFolderAdded(FolderChangedEventArgs args)
        {
            if (FolderAdded != null)
            {
                FolderAdded(this, args);
            }
        }

        public virtual void OnFolderDeleted(FolderDeletedEventArgs args)
        {
            if (FolderDeleted != null)
            {
                FolderDeleted(this, args);
            }
        }

        public virtual void OnFolderMoved(FolderMovedEventArgs args)
        {
            if (FolderMoved != null)
            {
                FolderMoved(this, args);
            }
        }

        public virtual void OnFolderRenamed(FolderRenamedEventArgs args)
        {
            if (FolderRenamed != null)
            {
                FolderRenamed(this, args);
            }
        }

        public virtual void OnFollowRequested(RelationshipEventArgs args)
        {
            if (FollowRequested != null)
            {
                FollowRequested(this, args);
            }
        }

        public virtual void OnFriendshipAccepted(RelationshipEventArgs args)
        {
            if (FriendshipAccepted != null)
            {
                FriendshipAccepted(this, args);
            }
        }

        public virtual void OnFriendshipDeleted(RelationshipEventArgs args)
        {
            if (FriendshipDeleted != null)
            {
                FriendshipDeleted(this, args);
            }
        }

        public virtual void OnFriendshipRequested(RelationshipEventArgs args)
        {
            if (FriendshipRequested != null)
            {
                FriendshipRequested(this, args);
            }
        }

        public virtual void OnModuleCreated(ModuleEventArgs args)
        {
            if (ModuleCreated != null)
            {
                ModuleCreated(this, args);
            }
        }

        public virtual void OnModuleDeleted(ModuleEventArgs args)
        {
            if (ModuleDeleted != null)
            {
                ModuleDeleted(this, args);
            }
        }

        public virtual void OnModuleRemoved(ModuleEventArgs args)
        {
            if (ModuleRemoved != null)
            {
                ModuleRemoved(this, args);
            }
        }

        public virtual void OnModuleUpdated(ModuleEventArgs args)
        {
            if (ModuleUpdated != null)
            {
                ModuleUpdated(this, args);
            }
        }

        public virtual void OnPortalCreated(PortalCreatedEventArgs args)
        {
            if (PortalCreated != null)
            {
                PortalCreated(this, args);
            }
        }

        public virtual void OnPortalTemplateCreated(PortalTemplateEventArgs args)
        {
            if (PortalTemplateCreated != null)
            {
                PortalTemplateCreated(this, args);
            }
        }

        public virtual void OnProfileUpdated(ProfileEventArgs args)
        {
            if (ProfileUpdated != null)
            {
                ProfileUpdated(this, args);
            }
        }

        public virtual void OnRoleCreated(RoleEventArgs args)
        {
            if (RoleCreated != null)
            {
                RoleCreated(this, args);
            }
        }

        public virtual void OnRoleDeleted(RoleEventArgs args)
        {
            if (RoleDeleted != null)
            {
                RoleDeleted(this, args);
            }
        }

        public virtual void OnRoleJoined(RoleEventArgs args)
        {
            if (RoleJoined != null)
            {
                RoleJoined(this, args);
            }
        }

        public virtual void OnRoleLeft(RoleEventArgs args)
        {
            if (RoleLeft != null)
            {
                RoleLeft(this, args);
            }
        }

        public virtual void OnTabCreated(TabEventArgs args)
        {
            if (TabCreated != null)
            {
                TabCreated(this, args);
            }
        }

        public virtual void OnTabDeleted(TabEventArgs args)
        {
            if (TabDeleted != null)
            {
                TabDeleted(this, args);
            }
        }

        public virtual void OnTabDeserialize(TabSyncEventArgs args)
        {
            if (TabDeserialize != null)
            {
                TabDeserialize(this, args);
            }
        }

        public virtual void OnTabMarkedAsPublished(TabEventArgs args)
        {
            if (TabMarkedAsPublished != null)
            {
                TabMarkedAsPublished(this, args);
            }
        }

        public virtual void OnTabRemoved(TabEventArgs args)
        {
            if (TabRemoved != null)
            {
                TabRemoved(this, args);
            }
        }

        public virtual void OnTabRestored(TabEventArgs args)
        {
            if (TabRestored != null)
            {
                TabRestored(this, args);
            }
        }

        public virtual void OnTabSerialize(TabSyncEventArgs args)
        {
            if (TabSerialize != null)
            {
                TabSerialize(this, args);
            }
        }

        public virtual void OnTabUpdated(TabEventArgs args)
        {
            if (TabUpdated != null)
            {
                TabUpdated(this, args);
            }
        }

        public virtual void OnUnfollowRequested(RelationshipEventArgs args)
        {
            if (UnfollowRequested != null)
            {
                UnfollowRequested(this, args);
            }
        }

        public virtual void OnUserApproved(UserEventArgs args)
        {
            if (UserApproved != null)
            {
                UserApproved(this, args);
            }
        }

        public virtual void OnUserAuthenticated(UserEventArgs args)
        {
            if (UserAuthenticated != null)
            {
                UserAuthenticated(this, args);
            }
        }

        public virtual void OnUserCreated(UserEventArgs args)
        {
            if (UserCreated != null)
            {
                UserCreated(this, args);
            }
        }

        public virtual void OnUserDeleted(UserEventArgs args)
        {
            if (UserDeleted != null)
            {
                UserDeleted(this, args);
            }
        }

        public virtual void OnUserRemoved(UserEventArgs args)
        {
            if (UserRemoved != null)
            {
                UserRemoved(this, args);
            }
        }

        public virtual void OnUserUpdated(UpdateUserEventArgs args)
        {
            if (UserUpdated != null)
            {
                UserUpdated(this, args);
            }
        }

        public void RefreshTabSyncHandlers()
        {
            foreach (var handlers in new EventHandlersContainer<ITabSyncEventHandler>().EventHandlers)
            {
                TabSerialize += handlers.Value.TabSerialize;
                TabDeserialize += handlers.Value.TabDeserialize;
            }
        }
    }
}
