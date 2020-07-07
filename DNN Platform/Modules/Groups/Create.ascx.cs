// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using Microsoft.Extensions.DependencyInjection;

    public partial class Create : GroupsModuleBase
    {
        private readonly INavigationManager _navigationManager;

        public Create()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected override void OnInit(EventArgs e)
        {
            this.InitializeComponent();
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        private void InitializeComponent()
        {
            this.Load += this.Page_Load;
            this.btnCreate.Click += this.Create_Click;
            this.btnCancel.Click += this.Cancel_Click;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false, null));
        }

        private void Create_Click(object sender, EventArgs e)
        {
            var ps = Security.PortalSecurity.Instance;
            this.txtGroupName.Text = ps.InputFilter(this.txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoScripting);
            this.txtGroupName.Text = ps.InputFilter(this.txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoMarkup);

            this.txtDescription.Text = ps.InputFilter(this.txtDescription.Text, Security.PortalSecurity.FilterFlag.NoScripting);
            this.txtDescription.Text = ps.InputFilter(this.txtDescription.Text, Security.PortalSecurity.FilterFlag.NoMarkup);
            if (RoleController.Instance.GetRoleByName(this.PortalId, this.txtGroupName.Text) != null)
            {
                this.lblInvalidGroupName.Visible = true;
                return;
            }

            var modRoles = new List<RoleInfo>();
            var modUsers = new List<UserInfo>();
            foreach (ModulePermissionInfo modulePermissionInfo in ModulePermissionController.GetModulePermissions(this.ModuleId, this.TabId))
            {
                if (modulePermissionInfo.PermissionKey == "MODGROUP" && modulePermissionInfo.AllowAccess)
                {
                    if (modulePermissionInfo.RoleID > int.Parse(Globals.glbRoleNothing))
                    {
                        modRoles.Add(RoleController.Instance.GetRoleById(this.PortalId, modulePermissionInfo.RoleID));
                    }
                    else if (modulePermissionInfo.UserID > Null.NullInteger)
                    {
                        modUsers.Add(UserController.GetUserById(this.PortalId, modulePermissionInfo.UserID));
                    }
                }
            }

            var roleInfo = new RoleInfo()
            {
                PortalID = this.PortalId,
                RoleName = this.txtGroupName.Text,
                Description = this.txtDescription.Text,
                SecurityMode = SecurityMode.SocialGroup,
                Status = RoleStatus.Approved,
                IsPublic = this.rdAccessTypePublic.Checked,
            };
            var userRoleStatus = RoleStatus.Pending;
            if (this.GroupModerationEnabled)
            {
                roleInfo.Status = RoleStatus.Pending;
                userRoleStatus = RoleStatus.Pending;
            }
            else
            {
                userRoleStatus = RoleStatus.Approved;
            }

            var objModulePermissions = new ModulePermissionCollection(CBO.FillCollection(DataProvider.Instance().GetModulePermissionsByModuleID(this.ModuleId, -1), typeof(ModulePermissionInfo)));
            if (ModulePermissionController.HasModulePermission(objModulePermissions, "MODGROUP"))
            {
                roleInfo.Status = RoleStatus.Approved;
                userRoleStatus = RoleStatus.Approved;
            }

            var roleGroupId = this.DefaultRoleGroupId;
            if (roleGroupId < Null.NullInteger)
            {
                roleGroupId = Null.NullInteger;
            }

            roleInfo.RoleGroupID = roleGroupId;

            roleInfo.RoleID = RoleController.Instance.AddRole(roleInfo);
            roleInfo = RoleController.Instance.GetRoleById(this.PortalId, roleInfo.RoleID);

            var groupUrl = this._navigationManager.NavigateURL(this.GroupViewTabId, string.Empty, new string[] { "groupid=" + roleInfo.RoleID.ToString() });
            if (groupUrl.StartsWith("http://") || groupUrl.StartsWith("https://"))
            {
                const int startIndex = 8; // length of https://
                groupUrl = groupUrl.Substring(groupUrl.IndexOf("/", startIndex, StringComparison.InvariantCultureIgnoreCase));
            }

            roleInfo.Settings.Add("URL", groupUrl);

            roleInfo.Settings.Add("GroupCreatorName", this.UserInfo.DisplayName);
            roleInfo.Settings.Add("ReviewMembers", this.chkMemberApproved.Checked.ToString());

            RoleController.Instance.UpdateRoleSettings(roleInfo, true);
            if (this.inpFile.PostedFile != null && this.inpFile.PostedFile.ContentLength > 0)
            {
                IFileManager _fileManager = FileManager.Instance;
                IFolderManager _folderManager = FolderManager.Instance;
                var rootFolderPath = PathUtils.Instance.FormatFolderPath(this.PortalSettings.HomeDirectory);

                IFolderInfo groupFolder = _folderManager.GetFolder(this.PortalSettings.PortalId, "Groups/" + roleInfo.RoleID);
                if (groupFolder == null)
                {
                    groupFolder = _folderManager.AddFolder(this.PortalSettings.PortalId, "Groups/" + roleInfo.RoleID);
                }

                if (groupFolder != null)
                {
                    var fileName = Path.GetFileName(this.inpFile.PostedFile.FileName);
                    var fileInfo = _fileManager.AddFile(groupFolder, fileName, this.inpFile.PostedFile.InputStream, true);
                    roleInfo.IconFile = "FileID=" + fileInfo.FileId;
                    RoleController.Instance.UpdateRole(roleInfo);
                }
            }

            var notifications = new Notifications();

            RoleController.Instance.AddUserRole(this.PortalId, this.UserId, roleInfo.RoleID, userRoleStatus, true, Null.NullDate, Null.NullDate);
            if (roleInfo.Status == RoleStatus.Pending)
            {
                // Send notification to Group Moderators to approve/reject group.
                notifications.AddGroupNotification(Constants.GroupPendingNotification, this.GroupViewTabId, this.ModuleId, roleInfo, this.UserInfo, modRoles, modUsers);
            }
            else
            {
                // Send notification to Group Moderators informing of new group.
                notifications.AddGroupNotification(Constants.GroupCreatedNotification, this.GroupViewTabId, this.ModuleId, roleInfo, this.UserInfo, modRoles, modUsers);

                // Add entry to journal.
                GroupUtilities.CreateJournalEntry(roleInfo, this.UserInfo);
            }

            this.Response.Redirect(this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false, null));
        }
    }
}
