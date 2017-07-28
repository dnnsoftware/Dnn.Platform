using System;
using System.Collections.Generic;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Common;
using DotNetNuke.Security.Roles;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.FileSystem;
using System.IO;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Modules.Groups.Components;

namespace DotNetNuke.Modules.Groups
{

    public partial class Create : GroupsModuleBase
    {

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
            btnCreate.Click += Create_Click;
            btnCancel.Click += Cancel_Click;
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(ModuleContext.NavigateUrl(TabId, string.Empty, false, null));
        }
        private void Create_Click(object sender, EventArgs e)
        {
            var ps = Security.PortalSecurity.Instance;
            txtGroupName.Text = ps.InputFilter(txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoScripting);
            txtGroupName.Text = ps.InputFilter(txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoMarkup);

            txtDescription.Text = ps.InputFilter(txtDescription.Text, Security.PortalSecurity.FilterFlag.NoScripting);
            txtDescription.Text = ps.InputFilter(txtDescription.Text, Security.PortalSecurity.FilterFlag.NoMarkup);
            if (RoleController.Instance.GetRoleByName(PortalId, txtGroupName.Text) != null)
            {
                lblInvalidGroupName.Visible = true;
                return;
            }


            var modRoles = new List<RoleInfo>();
			var modUsers = new List<UserInfo>();
            foreach (ModulePermissionInfo modulePermissionInfo in ModulePermissionController.GetModulePermissions(ModuleId, TabId))
            {
                if (modulePermissionInfo.PermissionKey == "MODGROUP" && modulePermissionInfo.AllowAccess)
                {
	                if (modulePermissionInfo.RoleID > int.Parse(Globals.glbRoleNothing))
	                {
                        modRoles.Add(RoleController.Instance.GetRoleById(PortalId, modulePermissionInfo.RoleID));
	                }
					else if (modulePermissionInfo.UserID > Null.NullInteger)
					{
						modUsers.Add(UserController.GetUserById(PortalId, modulePermissionInfo.UserID));
					}
                }
            }

            var roleInfo = new RoleInfo()
            {
                PortalID = PortalId,
                RoleName = txtGroupName.Text,
                Description = txtDescription.Text,
                SecurityMode = SecurityMode.SocialGroup,
                Status = RoleStatus.Approved,
                IsPublic = rdAccessTypePublic.Checked
            };
            var userRoleStatus = RoleStatus.Pending;
            if (GroupModerationEnabled)
            {
                roleInfo.Status = RoleStatus.Pending;
                userRoleStatus = RoleStatus.Pending;
            }
            else
            {
                userRoleStatus = RoleStatus.Approved;
            }

            var objModulePermissions = new ModulePermissionCollection(CBO.FillCollection(DataProvider.Instance().GetModulePermissionsByModuleID(ModuleId, -1), typeof(ModulePermissionInfo)));
            if (ModulePermissionController.HasModulePermission(objModulePermissions, "MODGROUP"))
            {
                roleInfo.Status = RoleStatus.Approved;
                userRoleStatus = RoleStatus.Approved;
            }

	        var roleGroupId = DefaultRoleGroupId;
	        if (roleGroupId < Null.NullInteger)
	        {
		        roleGroupId = Null.NullInteger;
	        }
			roleInfo.RoleGroupID = roleGroupId;

            roleInfo.RoleID = RoleController.Instance.AddRole(roleInfo);
            roleInfo = RoleController.Instance.GetRoleById(PortalId, roleInfo.RoleID);

	        var groupUrl = Globals.NavigateURL(GroupViewTabId, "", new String[] {"groupid=" + roleInfo.RoleID.ToString()});
			if (groupUrl.StartsWith("http://") || groupUrl.StartsWith("https://"))
			{
				const int startIndex = 8; // length of https://
				groupUrl = groupUrl.Substring(groupUrl.IndexOf("/", startIndex, StringComparison.InvariantCultureIgnoreCase));
			}
			roleInfo.Settings.Add("URL", groupUrl);

            roleInfo.Settings.Add("GroupCreatorName", UserInfo.DisplayName);
            roleInfo.Settings.Add("ReviewMembers", chkMemberApproved.Checked.ToString());

            RoleController.Instance.UpdateRoleSettings(roleInfo, true);
	    if (inpFile.PostedFile != null && inpFile.PostedFile.ContentLength > 0)
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

            var notifications = new Notifications();


            RoleController.Instance.AddUserRole(PortalId, UserId, roleInfo.RoleID, userRoleStatus, true, Null.NullDate, Null.NullDate);
            if (roleInfo.Status == RoleStatus.Pending)
            {
                //Send notification to Group Moderators to approve/reject group.
                notifications.AddGroupNotification(Constants.GroupPendingNotification, GroupViewTabId, ModuleId, roleInfo, UserInfo, modRoles, modUsers);
            }
            else
            {
                //Send notification to Group Moderators informing of new group.
                notifications.AddGroupNotification(Constants.GroupCreatedNotification, GroupViewTabId, ModuleId, roleInfo, UserInfo, modRoles, modUsers);

                //Add entry to journal.
                GroupUtilities.CreateJournalEntry(roleInfo, UserInfo);
            }

            Response.Redirect(Globals.NavigateURL(GroupViewTabId, "", new String[] { "groupid=" + roleInfo.RoleID.ToString() }));
        }
    }
}