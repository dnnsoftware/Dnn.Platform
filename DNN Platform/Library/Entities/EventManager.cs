// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
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
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Manages DNN Events.</summary>
    public class EventManager : ServiceLocator<IEventManager, EventManager>, IEventManager
    {
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="EventManager"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. Use overload taking IEventLogger. Scheduled removal in v11.0.0.")]
        public EventManager()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EventManager"/> class.</summary>
        /// <param name="eventLogger">An event logger.</param>
        public EventManager(IEventLogger eventLogger)
        {
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();

            foreach (var handler in EventHandlersContainer<IFileEventHandlers>.Instance.EventHandlers)
            {
                this.FileChanged += handler.Value.FileOverwritten;
                this.FileDeleted += handler.Value.FileDeleted;
                this.FileRenamed += handler.Value.FileRenamed;
                this.FileMoved += handler.Value.FileMoved;
                this.FileAdded += handler.Value.FileAdded;
                this.FileOverwritten += handler.Value.FileOverwritten;
                this.FileMetadataChanged += handler.Value.FileMetadataChanged;
                this.FileDownloaded += handler.Value.FileDownloaded;

                this.FolderAdded += handler.Value.FolderAdded;
                this.FolderDeleted += handler.Value.FolderDeleted;
                this.FolderMoved += handler.Value.FolderMoved;
                this.FolderRenamed += handler.Value.FolderRenamed;
            }

            foreach (var handler in EventHandlersContainer<IFollowerEventHandlers>.Instance.EventHandlers)
            {
                this.FollowRequested += handler.Value.FollowRequested;
                this.UnfollowRequested += handler.Value.UnfollowRequested;
            }

            foreach (var handlers in EventHandlersContainer<IFriendshipEventHandlers>.Instance.EventHandlers)
            {
                this.FriendshipRequested += handlers.Value.FriendshipRequested;
                this.FriendshipAccepted += handlers.Value.FriendshipAccepted;
                this.FriendshipDeleted += handlers.Value.FriendshipDeleted;
            }

            foreach (var handlers in EventHandlersContainer<IModuleEventHandler>.Instance.EventHandlers)
            {
                this.ModuleCreated += handlers.Value.ModuleCreated;
                this.ModuleUpdated += handlers.Value.ModuleUpdated;
                this.ModuleRemoved += handlers.Value.ModuleRemoved;
                this.ModuleDeleted += handlers.Value.ModuleDeleted;
            }

            foreach (var handler in EventHandlersContainer<IPortalEventHandlers>.Instance.EventHandlers)
            {
                this.PortalCreated += handler.Value.PortalCreated;
            }

            foreach (var handler in EventHandlersContainer<IPortalSettingHandlers>.Instance.EventHandlers)
            {
                this.PortalSettingUpdated += handler.Value.PortalSettingUpdated;
            }

            foreach (var handler in EventHandlersContainer<IPortalTemplateEventHandlers>.Instance.EventHandlers)
            {
                this.PortalTemplateCreated += handler.Value.TemplateCreated;
            }

            foreach (var handler in EventHandlersContainer<IProfileEventHandlers>.Instance.EventHandlers)
            {
                this.ProfileUpdated += handler.Value.ProfileUpdated;
            }

            foreach (var handler in EventHandlersContainer<IRoleEventHandlers>.Instance.EventHandlers)
            {
                this.RoleCreated += handler.Value.RoleCreated;
                this.RoleDeleted += handler.Value.RoleDeleted;
                this.RoleJoined += handler.Value.RoleJoined;
                this.RoleLeft += handler.Value.RoleLeft;
                if (handler.Value is IRoleUpdateEventHandlers updateHandler)
                {
                    this.RoleUpdated += updateHandler.RoleUpdated;
                }
            }

            foreach (var handler in EventHandlersContainer<ITabEventHandler>.Instance.EventHandlers)
            {
                this.TabCreated += handler.Value.TabCreated;
                this.TabUpdated += handler.Value.TabUpdated;
                this.TabRemoved += handler.Value.TabRemoved;
                this.TabDeleted += handler.Value.TabDeleted;
                this.TabRestored += handler.Value.TabRestored;
                this.TabMarkedAsPublished += handler.Value.TabMarkedAsPublished;
            }

            foreach (var handler in EventHandlersContainer<ITabSyncEventHandler>.Instance.EventHandlers)
            {
                this.TabSerialize += handler.Value.TabSerialize;
                this.TabDeserialize += handler.Value.TabDeserialize;
            }

            foreach (var handler in EventHandlersContainer<IUserEventHandlers>.Instance.EventHandlers)
            {
                this.UserAuthenticated += handler.Value.UserAuthenticated;
                this.UserCreated += handler.Value.UserCreated;
                this.UserDeleted += handler.Value.UserDeleted;
                this.UserRemoved += handler.Value.UserRemoved;
                this.UserApproved += handler.Value.UserApproved;
                this.UserUpdated += handler.Value.UserUpdated;
            }
        }

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

        private event EventHandler<PortalSettingUpdatedEventArgs> PortalSettingUpdated;

        private event EventHandler<ProfileEventArgs> ProfileUpdated;

        private event EventHandler<RoleEventArgs> RoleCreated;

        private event EventHandler<RoleEventArgs> RoleUpdated;

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

        /// <inheritdoc/>
        public virtual void OnFileAdded(FileAddedEventArgs args)
        {
            if (this.FileAdded != null)
            {
                this.FileAdded(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_ADDED);
        }

        /// <inheritdoc/>
        public virtual void OnFileChanged(FileChangedEventArgs args)
        {
            if (this.FileChanged != null)
            {
                this.FileChanged(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_CHANGED);
        }

        /// <inheritdoc/>
        public virtual void OnFileDeleted(FileDeletedEventArgs args)
        {
            if (this.FileDeleted != null)
            {
                this.FileDeleted(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_DELETED);
        }

        /// <inheritdoc/>
        public virtual void OnFileMetadataChanged(FileChangedEventArgs args)
        {
            if (this.FileMetadataChanged != null)
            {
                this.FileMetadataChanged(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_METADATACHANGED);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public virtual void OnFileDownloaded(FileDownloadedEventArgs args)
        {
            if (this.FileDownloaded != null)
            {
                this.FileDownloaded(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_DOWNLOADED);
        }

        /// <inheritdoc/>
        public virtual void OnFileMoved(FileMovedEventArgs args)
        {
            if (this.FileMoved != null)
            {
                this.FileMoved(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_MOVED);
        }

        /// <inheritdoc/>
        public virtual void OnFileOverwritten(FileChangedEventArgs args)
        {
            if (this.FileOverwritten != null)
            {
                this.FileOverwritten(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_OVERWRITTEN);
        }

        /// <inheritdoc/>
        public virtual void OnFileRenamed(FileRenamedEventArgs args)
        {
            if (this.FileRenamed != null)
            {
                this.FileRenamed(this, args);
            }

            this.AddLog(args.FileInfo, args.UserId, EventLogType.FILE_RENAMED);
        }

        /// <inheritdoc/>
        public virtual void OnFolderAdded(FolderChangedEventArgs args)
        {
            if (this.FolderAdded != null)
            {
                this.FolderAdded(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnFolderDeleted(FolderDeletedEventArgs args)
        {
            if (this.FolderDeleted != null)
            {
                this.FolderDeleted(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnFolderMoved(FolderMovedEventArgs args)
        {
            if (this.FolderMoved != null)
            {
                this.FolderMoved(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnFolderRenamed(FolderRenamedEventArgs args)
        {
            if (this.FolderRenamed != null)
            {
                this.FolderRenamed(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnFollowRequested(RelationshipEventArgs args)
        {
            if (this.FollowRequested != null)
            {
                this.FollowRequested(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnFriendshipAccepted(RelationshipEventArgs args)
        {
            if (this.FriendshipAccepted != null)
            {
                this.FriendshipAccepted(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnFriendshipDeleted(RelationshipEventArgs args)
        {
            if (this.FriendshipDeleted != null)
            {
                this.FriendshipDeleted(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnFriendshipRequested(RelationshipEventArgs args)
        {
            if (this.FriendshipRequested != null)
            {
                this.FriendshipRequested(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnModuleCreated(ModuleEventArgs args)
        {
            if (this.ModuleCreated != null)
            {
                this.ModuleCreated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnModuleDeleted(ModuleEventArgs args)
        {
            if (this.ModuleDeleted != null)
            {
                this.ModuleDeleted(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnModuleRemoved(ModuleEventArgs args)
        {
            if (this.ModuleRemoved != null)
            {
                this.ModuleRemoved(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnModuleUpdated(ModuleEventArgs args)
        {
            if (this.ModuleUpdated != null)
            {
                this.ModuleUpdated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnPortalCreated(PortalCreatedEventArgs args)
        {
            if (this.PortalCreated != null)
            {
                this.PortalCreated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnPortalSettingUpdated(PortalSettingUpdatedEventArgs args)
        {
            if (this.PortalSettingUpdated != null)
            {
                this.PortalSettingUpdated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnPortalTemplateCreated(PortalTemplateEventArgs args)
        {
            if (this.PortalTemplateCreated != null)
            {
                this.PortalTemplateCreated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnProfileUpdated(ProfileEventArgs args)
        {
            if (this.ProfileUpdated != null)
            {
                this.ProfileUpdated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnRoleCreated(RoleEventArgs args)
        {
            if (this.RoleCreated != null)
            {
                this.RoleCreated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnRoleUpdated(RoleEventArgs args)
        {
            this.RoleUpdated?.Invoke(this, args);
        }

        /// <inheritdoc/>
        public virtual void OnRoleDeleted(RoleEventArgs args)
        {
            if (this.RoleDeleted != null)
            {
                this.RoleDeleted(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnRoleJoined(RoleEventArgs args)
        {
            if (this.RoleJoined != null)
            {
                this.RoleJoined(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnRoleLeft(RoleEventArgs args)
        {
            if (this.RoleLeft != null)
            {
                this.RoleLeft(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabCreated(TabEventArgs args)
        {
            if (this.TabCreated != null)
            {
                this.TabCreated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabDeleted(TabEventArgs args)
        {
            if (this.TabDeleted != null)
            {
                this.TabDeleted(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabDeserialize(TabSyncEventArgs args)
        {
            if (this.TabDeserialize != null)
            {
                this.TabDeserialize(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabMarkedAsPublished(TabEventArgs args)
        {
            if (this.TabMarkedAsPublished != null)
            {
                this.TabMarkedAsPublished(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabRemoved(TabEventArgs args)
        {
            if (this.TabRemoved != null)
            {
                this.TabRemoved(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabRestored(TabEventArgs args)
        {
            if (this.TabRestored != null)
            {
                this.TabRestored(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabSerialize(TabSyncEventArgs args)
        {
            if (this.TabSerialize != null)
            {
                this.TabSerialize(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnTabUpdated(TabEventArgs args)
        {
            if (this.TabUpdated != null)
            {
                this.TabUpdated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnUnfollowRequested(RelationshipEventArgs args)
        {
            if (this.UnfollowRequested != null)
            {
                this.UnfollowRequested(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnUserApproved(UserEventArgs args)
        {
            if (this.UserApproved != null)
            {
                this.UserApproved(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnUserAuthenticated(UserEventArgs args)
        {
            if (this.UserAuthenticated != null)
            {
                this.UserAuthenticated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnUserCreated(UserEventArgs args)
        {
            if (this.UserCreated != null)
            {
                this.UserCreated(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnUserDeleted(UserEventArgs args)
        {
            if (this.UserDeleted != null)
            {
                this.UserDeleted(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnUserRemoved(UserEventArgs args)
        {
            if (this.UserRemoved != null)
            {
                this.UserRemoved(this, args);
            }
        }

        /// <inheritdoc/>
        public virtual void OnUserUpdated(UpdateUserEventArgs args)
        {
            if (this.UserUpdated != null)
            {
                this.UserUpdated(this, args);
            }
        }

        /// <inheritdoc/>
        public void RefreshTabSyncHandlers()
        {
            foreach (var handlers in new EventHandlersContainer<ITabSyncEventHandler>().EventHandlers)
            {
                this.TabSerialize += handlers.Value.TabSerialize;
                this.TabDeserialize += handlers.Value.TabDeserialize;
            }
        }

        /// <inheritdoc/>
        protected override Func<IEventManager> GetFactory()
        {
            return () => new EventManager(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>());
        }

        private void AddLog(IFileInfo fileInfo, int userId, EventLogType logType)
        {
            if (fileInfo == null)
            {
                return;
            }

            this.eventLogger.AddLog(fileInfo, PortalSettings.Current, userId, string.Empty, logType);
        }
    }
}
