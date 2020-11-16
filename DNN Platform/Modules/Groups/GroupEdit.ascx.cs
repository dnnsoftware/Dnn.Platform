// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.IO;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using Microsoft.Extensions.DependencyInjection;

    public partial class GroupEdit : GroupsModuleBase
    {
        private readonly INavigationManager _navigationManager;

        public GroupEdit()
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

            this.imgGroup.Src = this.Page.ResolveUrl("~/DesktopModules/SocialGroups/Images/") + "sample-group-profile.jpg";
            if (!this.Page.IsPostBack && this.GroupId > 0)
            {
                var roleInfo = RoleController.Instance.GetRoleById(this.PortalId, this.GroupId);

                if (roleInfo != null)
                {
                    if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                    {
                        if (roleInfo.CreatedByUserID != this.UserInfo.UserID)
                        {
                            this.Response.Redirect(this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false, new string[] { "groupid=" + this.GroupId.ToString() }));
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

                    if (roleInfo.Settings.ContainsKey("ReviewMembers"))
                    {
                        this.chkMemberApproved.Checked = Convert.ToBoolean(roleInfo.Settings["ReviewMembers"].ToString());
                    }

                    this.imgGroup.Src = roleInfo.PhotoURL;
                }
                else
                {
                    this.Response.Redirect(this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false));
                }
            }
        }

        private void InitializeComponent()
        {
            this.Load += this.Page_Load;
            this.btnSave.Click += this.Save_Click;
            this.btnCancel.Click += this.Cancel_Click;
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.ModuleContext.NavigateUrl(this.TabId, string.Empty, false, new string[] { "groupid=" + this.GroupId.ToString() }));
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (this.GroupId > 0)
            {
                Security.PortalSecurity ps = Security.PortalSecurity.Instance;

                this.txtGroupName.Text = ps.InputFilter(this.txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoScripting);
                this.txtGroupName.Text = ps.InputFilter(this.txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoMarkup);
                this.txtDescription.Text = ps.InputFilter(this.txtDescription.Text, Security.PortalSecurity.FilterFlag.NoScripting);
                this.txtDescription.Text = ps.InputFilter(this.txtDescription.Text, Security.PortalSecurity.FilterFlag.NoMarkup);

                var roleInfo = RoleController.Instance.GetRoleById(this.PortalId, this.GroupId);
                if (roleInfo != null)
                {
                    if (this.txtGroupName.Visible) // if this is visible assume that we're editing the groupname
                    {
                        if (this.txtGroupName.Text != roleInfo.RoleName)
                        {
                            if (RoleController.Instance.GetRoleByName(this.PortalId, this.txtGroupName.Text) != null)
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

                    RoleController.Instance.UpdateRoleSettings(roleInfo, true);
                    RoleController.Instance.UpdateRole(roleInfo);

                    if (this.inpFile.PostedFile.ContentLength > 0)
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

                    // Clear Roles Cache
                    DataCache.RemoveCache("GetRoles");
                }

                this.Response.Redirect(this._navigationManager.NavigateURL(this.TabId, string.Empty, new string[] { "groupid=" + this.GroupId.ToString() }));
            }
        }
    }
}
