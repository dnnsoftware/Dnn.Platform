// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups;

using System;
using System.IO;
using System.Web.UI;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using Microsoft.Extensions.DependencyInjection;

/// <summary>Display the group editor.</summary>
public partial class GroupEdit : GroupsModuleBase
{
    private readonly INavigationManager navigationManager;
    private readonly IFileManager fileManager;
    private readonly IFolderManager folderManager;
    private readonly IFileContentTypeManager fileContentTypeManager;
    private readonly IRoleController roleController;
    private readonly IApplicationStatusInfo appStatus;
    private readonly IEventLogger eventLogger;

    /// <summary>Initializes a new instance of the <see cref="GroupEdit"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.1.1. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
    public GroupEdit()
        : this(null, null, null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GroupEdit"/> class.</summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="fileManager">The file manager.</param>
    /// <param name="folderManager">The folder manager.</param>
    /// <param name="fileContentTypeManager">The file content type manager.</param>
    /// <param name="roleController">The role controller.</param>
    /// <param name="applicationStatusInfo">The application status info.</param>
    /// <param name="eventLogger">The event logger.</param>
    public GroupEdit(INavigationManager navigationManager, IFileManager fileManager, IFolderManager folderManager, IFileContentTypeManager fileContentTypeManager, IRoleController roleController, IApplicationStatusInfo applicationStatusInfo, IEventLogger eventLogger)
    {
        this.navigationManager = navigationManager ?? this.DependencyProvider.GetRequiredService<INavigationManager>();
        this.fileManager = fileManager ?? this.DependencyProvider.GetRequiredService<IFileManager>();
        this.folderManager = folderManager ?? this.DependencyProvider.GetRequiredService<IFolderManager>();
        this.fileContentTypeManager = fileContentTypeManager ?? this.DependencyProvider.GetRequiredService<IFileContentTypeManager>();
        this.roleController = roleController ?? this.DependencyProvider.GetRequiredService<IRoleController>();
        this.appStatus = applicationStatusInfo ?? this.DependencyProvider.GetRequiredService<IApplicationStatusInfo>();
        this.eventLogger = eventLogger ?? this.DependencyProvider.GetRequiredService<IEventLogger>();
    }

    /// <inheritdoc/>
    protected override void OnInit(EventArgs e)
    {
        this.Load += this.Page_Load;
        this.btnSave.Click += this.Save_Click;
        this.btnCancel.Click += this.Cancel_Click;
        base.OnInit(e);
    }

    /// <summary>Handles the <see cref="Control.Load"/> event.</summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event args.</param>
    protected void Page_Load(object sender, EventArgs e)
    {
        JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.DnnPlugins);

        this.imgGroup.Src = this.Page.ResolveUrl("~/DesktopModules/SocialGroups/Images/") + "sample-group-profile.jpg";
        if (this.Page.IsPostBack || this.GroupId <= 0)
        {
            return;
        }

        var roleInfo = this.roleController.GetRoleById(this.PortalId, this.GroupId);
        if (roleInfo == null)
        {
            this.Response.Redirect(this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false));
            return;
        }

        if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
        {
            if (roleInfo.CreatedByUserID != this.UserInfo.UserID)
            {
                this.Response.Redirect(
                    this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false, $"groupid={this.GroupId}"));
            }
        }

        this.txtGroupName.Visible = !roleInfo.IsSystemRole;
        this.reqGroupName.Enabled = !roleInfo.IsSystemRole;

        if (!roleInfo.IsSystemRole)
        {
            this.txtGroupName.Text = roleInfo.RoleName;
        }
        else
        {
            this.litGroupName.Text = roleInfo.RoleName;
        }

        this.txtDescription.Text = roleInfo.Description;
        this.rdAccessTypePrivate.Checked = !roleInfo.IsPublic;
        this.rdAccessTypePublic.Checked = roleInfo.IsPublic;

        if (roleInfo.Settings.TryGetValue("ReviewMembers", out var reviewMembers))
        {
            this.chkMemberApproved.Checked = Convert.ToBoolean(reviewMembers.ToString());
        }

        this.imgGroup.Src = roleInfo.PhotoURL;
    }

    private void Cancel_Click(object sender, EventArgs e)
    {
        this.Response.Redirect(this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false, $"groupid={this.GroupId}"));
    }

    private void Save_Click(object sender, EventArgs e)
    {
        if (this.GroupId <= 0)
        {
            return;
        }

        var ps = Security.PortalSecurity.Instance;

        this.txtGroupName.Text = ps.InputFilter(this.txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoScripting);
        this.txtGroupName.Text = ps.InputFilter(this.txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoMarkup);
        this.txtDescription.Text = ps.InputFilter(this.txtDescription.Text, Security.PortalSecurity.FilterFlag.NoScripting);
        this.txtDescription.Text = ps.InputFilter(this.txtDescription.Text, Security.PortalSecurity.FilterFlag.NoMarkup);

        var roleInfo = this.roleController.GetRoleById(this.PortalId, this.GroupId);
        if (roleInfo != null)
        {
            // if this is visible assume that we're editing the groupname
            if (this.txtGroupName.Visible)
            {
                if (this.txtGroupName.Text != roleInfo.RoleName)
                {
                    if (this.roleController.GetRoleByName(this.PortalId, this.txtGroupName.Text) != null)
                    {
                        this.lblInvalidGroupName.Visible = true;
                        return;
                    }
                }
            }

            if (!roleInfo.IsSystemRole)
            {
                roleInfo.RoleName = this.txtGroupName.Text;
            }

            roleInfo.Description = this.txtDescription.Text;
            roleInfo.IsPublic = this.rdAccessTypePublic.Checked;

            if (roleInfo.Settings.ContainsKey("ReviewMembers"))
            {
                roleInfo.Settings["ReviewMembers"] = this.chkMemberApproved.Checked.ToString();
            }
            else
            {
                roleInfo.Settings.Add("ReviewMembers", this.chkMemberApproved.Checked.ToString());
            }

            this.roleController.UpdateRoleSettings(roleInfo, true);
            this.roleController.UpdateRole(roleInfo);

            if (this.inpFile.PostedFile.ContentLength > 0)
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

            // Clear Roles Cache
            DataCache.RemoveCache("GetRoles");
        }

        this.Response.Redirect(this.navigationManager.NavigateURL(this.TabId, string.Empty, $"groupid={this.GroupId}"));
    }
}
