using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Common.Utilities;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;

namespace DotNetNuke.Modules.Groups
{
    public partial class GroupEdit : GroupsModuleBase
    {
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
            DotNetNuke.Framework.jQuery.RegisterDnnJQueryPlugins(this.Page);

            imgGroup.Src = Page.ResolveUrl("~/DesktopModules/SocialGroups/Images/") + "sample-group-profile.jpg";
            if (!Page.IsPostBack && GroupId > 0)
            {
                RoleController roleController = new RoleController();
                var roleInfo = roleController.GetRole(GroupId, PortalId);

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
                RoleController roleController = new RoleController();
                Security.PortalSecurity ps = new Security.PortalSecurity();

                txtGroupName.Text = ps.InputFilter(txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoScripting);
                txtGroupName.Text = ps.InputFilter(txtGroupName.Text, Security.PortalSecurity.FilterFlag.NoMarkup);
                txtDescription.Text = ps.InputFilter(txtDescription.Text, Security.PortalSecurity.FilterFlag.NoScripting);
                txtDescription.Text = ps.InputFilter(txtDescription.Text, Security.PortalSecurity.FilterFlag.NoMarkup);
                
                var roleInfo = roleController.GetRole(GroupId, PortalId);
                if (roleInfo != null)
                {

                    if (txtGroupName.Visible) //if this is visible assume that we're editing the groupname
                    {
                        if (txtGroupName.Text != roleInfo.RoleName)
                        {
                            if (roleController.GetRoleByName(PortalId, txtGroupName.Text) != null)
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
                    
                    TestableRoleController.Instance.UpdateRoleSettings(roleInfo, true);
                    roleController.UpdateRole(roleInfo);

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
                            roleController.UpdateRole(roleInfo);
                        }
                    }

                    //Clear Roles Cache
                    DataCache.RemoveCache("GetRoles");

                }

                Response.Redirect(Globals.NavigateURL(TabId, "", new String[] { "groupid=" + GroupId.ToString() }));
            }
        }
    }
}