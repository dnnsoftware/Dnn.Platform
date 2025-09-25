// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups;

using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Abstractions.Security.Permissions;
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

/// <summary>Display the group create view.</summary>
public partial class Create : GroupsModuleBase
{
    private readonly INavigationManager navigationManager;
    private readonly IFileManager fileManager;
    private readonly IFolderManager folderManager;
    private readonly IFileContentTypeManager fileContentTypeManager;
    private readonly IRoleController roleController;
    private readonly IApplicationStatusInfo appStatus;
    private readonly IEventLogger eventLogger;
    private readonly DataProvider dataProvider;

    /// <summary>Initializes a new instance of the <see cref="Create"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.1.1. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
    public Create()
        : this(null, null, null, null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="Create"/> class.</summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="fileManager">The file manager.</param>
    /// <param name="folderManager">The folder manager.</param>
    /// <param name="fileContentTypeManager">The file content type manager.</param>
    /// <param name="roleController">The role controller.</param>
    /// <param name="applicationStatusInfo">The application status info.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="dataProvider">The data provider.</param>
    public Create(INavigationManager navigationManager, IFileManager fileManager, IFolderManager folderManager, IFileContentTypeManager fileContentTypeManager, IRoleController roleController, IApplicationStatusInfo applicationStatusInfo, IEventLogger eventLogger, DataProvider dataProvider)
    {
        this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
        this.fileManager = fileManager ?? this.DependencyProvider.GetRequiredService<IFileManager>();
        this.folderManager = folderManager ?? this.DependencyProvider.GetRequiredService<IFolderManager>();
        this.fileContentTypeManager = fileContentTypeManager ?? this.DependencyProvider.GetRequiredService<IFileContentTypeManager>();
        this.roleController = roleController ?? this.DependencyProvider.GetRequiredService<IRoleController>();
        this.appStatus = applicationStatusInfo ?? this.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
        this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
        this.dataProvider = dataProvider ?? this.DependencyProvider.GetRequiredService<DataProvider>();
    }

    /// <inheritdoc/>
    protected override void OnInit(EventArgs e)
    {
        this.Load += this.Page_Load;
        this.btnCreate.Click += this.Create_Click;
        this.btnCancel.Click += this.Cancel_Click;
        base.OnInit(e);
    }

    /// <summary>Handles the <see cref="Control.Load"/> event.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);
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
        if (this.roleController.GetRoleByName(this.PortalId, this.txtGroupName.Text) != null)
        {
            this.lblInvalidGroupName.Visible = true;
            return;
        }

        var modRoles = new List<RoleInfo>();
        var modUsers = new List<UserInfo>();
        foreach (IPermissionInfo modulePermissionInfo in ModulePermissionController.GetModulePermissions(this.ModuleId, this.TabId))
        {
            if (modulePermissionInfo.PermissionKey == "MODGROUP" && modulePermissionInfo.AllowAccess)
            {
                if (modulePermissionInfo.RoleId > int.Parse(Globals.glbRoleNothing))
                {
                    modRoles.Add(this.roleController.GetRoleById(this.PortalId, modulePermissionInfo.RoleId));
                }
                else if (modulePermissionInfo.UserId > Null.NullInteger)
                {
                    modUsers.Add(UserController.GetUserById(this.PortalId, modulePermissionInfo.UserId));
                }
            }
        }

        var roleInfo = new RoleInfo
        {
            PortalID = this.PortalId,
            RoleName = this.txtGroupName.Text,
            Description = this.txtDescription.Text,
            SecurityMode = SecurityMode.SocialGroup,
            Status = RoleStatus.Approved,
            IsPublic = this.rdAccessTypePublic.Checked,
        };
        RoleStatus userRoleStatus;
        if (this.GroupModerationEnabled)
        {
            roleInfo.Status = RoleStatus.Pending;
            userRoleStatus = RoleStatus.Pending;
        }
        else
        {
            userRoleStatus = RoleStatus.Approved;
        }

        var objModulePermissions = new ModulePermissionCollection(CBO.FillCollection(this.dataProvider.GetModulePermissionsByModuleID(this.ModuleId, -1), typeof(ModulePermissionInfo)));
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

        roleInfo.RoleID = this.roleController.AddRole(roleInfo);
        roleInfo = this.roleController.GetRoleById(this.PortalId, roleInfo.RoleID);

        var groupUrl = this.navigationManager.NavigateURL(this.GroupViewTabId, string.Empty, $"groupid={roleInfo.RoleID}");
        if (groupUrl.StartsWith("http://") || groupUrl.StartsWith("https://"))
        {
            const int startIndex = 8; // length of https://
            groupUrl = groupUrl.Substring(groupUrl.IndexOf("/", startIndex, StringComparison.InvariantCultureIgnoreCase));
        }

        roleInfo.Settings.Add("URL", groupUrl);
        roleInfo.Settings.Add("GroupCreatorName", this.UserInfo.DisplayName);
        roleInfo.Settings.Add("ReviewMembers", this.chkMemberApproved.Checked.ToString());

        this.roleController.UpdateRoleSettings(roleInfo, true);
        if (this.inpFile.PostedFile is { ContentLength: > 0 })
        {
            var groupFolder = this.folderManager.GetFolder(this.PortalSettings.PortalId, $"Groups/{roleInfo.RoleID}") ??
                              this.folderManager.AddFolder(this.PortalSettings.PortalId, $"Groups/{roleInfo.RoleID}");

            if (groupFolder != null)
            {
                var fileName = Path.GetFileName(this.inpFile.PostedFile.FileName);
                var fileInfo = this.fileManager.AddFile(groupFolder, fileName, this.inpFile.PostedFile.InputStream, true, true, this.fileContentTypeManager.GetContentType(Path.GetExtension(fileName)));
                roleInfo.IconFile = $"FileID={fileInfo.FileId}";
                this.roleController.UpdateRole(roleInfo);
            }
        }

        var notifications = new Notifications();

        this.roleController.AddUserRole(this.PortalId, this.UserId, roleInfo.RoleID, userRoleStatus, true, Null.NullDate, Null.NullDate);
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
