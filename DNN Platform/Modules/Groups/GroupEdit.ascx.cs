﻿using System;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common.Utilities;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetNuke.Modules.Groups
{
    public partial class GroupEdit : GroupsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public GroupEdit()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
            btnSave.Click += Save_Click;
            btnCancel.Click += Cancel_Click;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            imgGroup.Src = Page.ResolveUrl("~/DesktopModules/SocialGroups/Images/") + "sample-group-profile.jpg";
            if (!Page.IsPostBack && GroupId > 0)
            {
                var roleInfo = RoleController.Instance.GetRoleById(PortalId, GroupId);

                if (roleInfo != null)
                {
                    if (!UserInfo.IsInRole(PortalSettings.AdministratorRoleName))
                    {
                        if (roleInfo.CreatedByUserID != UserInfo.UserID)
                        {
                            Response.Redirect(ModuleContext.NavigateUrl(TabId, "", false, new String[] { "groupid=" + GroupId.ToString() }));
                        }
                    }

                    txtGroupName.Visible = !roleInfo.IsSystemRole;
                    reqGroupName.Enabled = !roleInfo.IsSystemRole;

                    if(!roleInfo.IsSystemRole)
                        txtGroupName.Text = roleInfo.RoleName;
                    else
                        litGroupName.Text = roleInfo.RoleName;

                    txtDescription.Text = roleInfo.Description;
                    rdAccessTypePrivate.Checked = !roleInfo.IsPublic;
                    rdAccessTypePublic.Checked = roleInfo.IsPublic;


                    if (roleInfo.Settings.ContainsKey("ReviewMembers"))
                    {
                        chkMemberApproved.Checked = Convert.ToBoolean(roleInfo.Settings["ReviewMembers"].ToString());
                    }
                    imgGroup.Src = roleInfo.PhotoURL;
                }
                else
                {
                    Response.Redirect(ModuleContext.NavigateUrl(TabId, "", false));
                }
            }
        }
        private void Cancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(ModuleContext.NavigateUrl(TabId, "", false, new String[] { "groupid=" + GroupId.ToString() }));
        }

        private void Save_Click(object sender, EventArgs e)
        {
            if (GroupId > 0)
            {
                Security.PortalSecurity ps = Security.PortalSecurity.Instance;

                txtGroupName.Text = ps.InputFilter(txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoScripting);
                txtGroupName.Text = ps.InputFilter(txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoMarkup);
                txtDescription.Text = ps.InputFilter(txtDescription.Text, Security.PortalSecurity.FilterFlag.NoScripting);
                txtDescription.Text = ps.InputFilter(txtDescription.Text, Security.PortalSecurity.FilterFlag.NoMarkup);

                var roleInfo = RoleController.Instance.GetRoleById(PortalId, GroupId);
                if (roleInfo != null)
                {

                    if (txtGroupName.Visible) //if this is visible assume that we're editing the groupname
                    {
                        if (txtGroupName.Text != roleInfo.RoleName)
                        {
                            if (RoleController.Instance.GetRoleByName(PortalId, txtGroupName.Text) != null)
                            {
                                lblInvalidGroupName.Visible = true;
                                return;
                            }
                        }
                    }

                    if(!roleInfo.IsSystemRole)
                    {
                        roleInfo.RoleName = txtGroupName.Text;
                    }

                    roleInfo.Description = txtDescription.Text;
                    roleInfo.IsPublic = rdAccessTypePublic.Checked;

                    if (roleInfo.Settings.ContainsKey("ReviewMembers"))
                        roleInfo.Settings["ReviewMembers"] = chkMemberApproved.Checked.ToString();
                    else
                        roleInfo.Settings.Add("ReviewMembers", chkMemberApproved.Checked.ToString());

                    RoleController.Instance.UpdateRoleSettings(roleInfo, true);
                    RoleController.Instance.UpdateRole(roleInfo);

                    if (inpFile.PostedFile.ContentLength > 0)
                    {
                        IFileManager _fileManager = FileManager.Instance;
                        IFolderManager _folderManager = FolderManager.Instance;
                        var rootFolderPath = PathUtils.Instance.FormatFolderPath(PortalSettings.HomeDirectory);

                        IFolderInfo groupFolder = _folderManager.GetFolder(PortalSettings.PortalId, "Groups/" + roleInfo.RoleID);
                        if (groupFolder == null)
                        {
                            groupFolder = _folderManager.AddFolder(PortalSettings.PortalId, "Groups/" + roleInfo.RoleID);
                        }
                        if (groupFolder != null)
                        {
                            var fileName = Path.GetFileName(inpFile.PostedFile.FileName);
                            var fileInfo = _fileManager.AddFile(groupFolder, fileName, inpFile.PostedFile.InputStream, true);
                            roleInfo.IconFile = "FileID=" + fileInfo.FileId;
                            RoleController.Instance.UpdateRole(roleInfo);
                        }
                    }

                    //Clear Roles Cache
                    DataCache.RemoveCache("GetRoles");

                }

                Response.Redirect(_navigationManager.NavigateURL(TabId, "", new String[] { "groupid=" + GroupId.ToString() }));
            }
        }
    }
}
