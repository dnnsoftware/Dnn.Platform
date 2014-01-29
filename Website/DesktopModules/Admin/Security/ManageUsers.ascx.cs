#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
#endregion
#region Usings

using System;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Modules.Admin.Security;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Profile;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins.Controls;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ManageUsers UserModuleBase is used to manage Users
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	9/13/2004	Updated to reflect design changes for Help, 508 support
    ///                       and localisation
    ///     [cnurse]    2/21/2005   Updated to use new User UserControl
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class ManageUsers : UserModuleBase, IActionable
    {
		#region Protected Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to display the Manage Services tab
        /// </summary>
        /// <history>
        /// 	[cnurse]	08/11/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool DisplayServices
        {
            get
            {
                object setting = GetSetting(PortalId, "Profile_ManageServices");
                return Convert.ToBoolean(setting) && !(IsEdit || User.IsSuperUser);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Redirect URL (after successful registration)
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/18/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string RedirectURL
        {
            get
            {
                string _RedirectURL = "";

                object setting = GetSetting(PortalId, "Redirect_AfterRegistration");

                if (Convert.ToInt32(setting) == Null.NullInteger)
                {
                    if (Request.QueryString["returnurl"] != null)
                    {
						//return to the url passed to register
                        _RedirectURL = HttpUtility.UrlDecode(Request.QueryString["returnurl"]);
                        //redirect url should never contain a protocol ( if it does, it is likely a cross-site request forgery attempt )
                        if (_RedirectURL.Contains("://"))
                        {
                            _RedirectURL = "";
                        }
                        if (_RedirectURL.Contains("?returnurl"))
                        {
                            string baseURL = _RedirectURL.Substring(0, _RedirectURL.IndexOf("?returnurl"));
                            string returnURL = _RedirectURL.Substring(_RedirectURL.IndexOf("?returnurl") + 11);

                            _RedirectURL = string.Concat(baseURL, "?returnurl", HttpUtility.UrlEncode(returnURL));
                        }
                    }
                    if (String.IsNullOrEmpty(_RedirectURL))
                    {
						//redirect to current page 
                        _RedirectURL = Globals.NavigateURL();
                    }
                }
                else //redirect to after registration page
                {
                    _RedirectURL = Globals.NavigateURL(Convert.ToInt32(setting));
                }
                return _RedirectURL;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url for the page
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/09/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string ReturnUrl
        {
            get
            {
                return Globals.NavigateURL(TabId, "", !String.IsNullOrEmpty(UserFilter) ? UserFilter : "");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Filter to use
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/09/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected string UserFilter
        {
            get
            {
                string filterString = !string.IsNullOrEmpty(Request["filter"]) ? "filter=" + Request["filter"] : "";
                string filterProperty = !string.IsNullOrEmpty(Request["filterproperty"]) ? "filterproperty=" + Request["filterproperty"] : "";
                string page = !string.IsNullOrEmpty(Request["currentpage"]) ? "currentpage=" + Request["currentpage"] : "";

                if (!string.IsNullOrEmpty(filterString))
                {
                    filterString += "&";
                }
                if (!string.IsNullOrEmpty(filterProperty))
                {
                    filterString += filterProperty + "&";
                }
                if (!string.IsNullOrEmpty(page))
                {
                    filterString += page;
                }
                return filterString;
            }
        }

		#endregion

		#region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the current Page No
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/09/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public int PageNo
        {
            get
            {
                int _PageNo = 0;
                if (ViewState["PageNo"] != null && !IsPostBack)
                {
                    _PageNo = Convert.ToInt32(ViewState["PageNo"]);
                }
                return _PageNo;
            }
            set
            {
                ViewState["PageNo"] = value;
            }
        }
		
		#endregion

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();
                if (!IsProfile)
                {
                    if (!AddUser && !IsEdit)
                    {
                        Actions.Add(GetNextActionID(),
                                    Localization.GetString(ModuleActionType.AddContent, LocalResourceFile),
                                    ModuleActionType.AddContent,
                                    "",
                                    "add.gif",
                                    EditUrl(),
                                    false,
                                    SecurityAccessLevel.Admin,
                                    true,
                                    false);
                        if (ProfileProviderConfig.CanEditProviderProperties)
                        {
                            Actions.Add(GetNextActionID(),
                                        Localization.GetString("ManageProfile.Action", LocalResourceFile),
                                        ModuleActionType.AddContent,
                                        "",
                                        "icon_profile_16px.gif",
                                        EditUrl("ManageProfile"),
                                        false,
                                        SecurityAccessLevel.Admin,
                                        true,
                                        false);
                        }
                        Actions.Add(GetNextActionID(),
                                    Localization.GetString("Cancel.Action", LocalResourceFile),
                                    ModuleActionType.AddContent,
                                    "",
                                    "lt.gif",
                                    ReturnUrl,
                                    false,
                                    SecurityAccessLevel.Admin,
                                    true,
                                    false);
                    }
                }
                return Actions;
            }
        }

        #endregion

		#region Private Methods

        private void BindData()
        {
            if (User != null)
            {
				//If trying to add a SuperUser - check that user is a SuperUser
                if (VerifyUserPermissions()==false)
                {
                    return;
                }
				
                if (AddUser)
                {
                    cmdAdd.Text = Localization.GetString("AddUser", LocalResourceFile);
                    lblTitle.Text = Localization.GetString("AddUser", LocalResourceFile);
                }
                else
                {
                    if (!Request.IsAuthenticated)
                    {
                        titleRow.Visible = false;
                    }
                    else
                    {
                        if (IsProfile)
                        {
                            titleRow.Visible = false;
                        }
                        else
                        {
                            lblTitle.Text = string.Format(Localization.GetString("UserTitle", LocalResourceFile), User.Username, User.UserID);
                        }
                    }
                }
                if (!Page.IsPostBack)
                {
                    if ((Request.QueryString["pageno"] != null))
                    {
                        PageNo = int.Parse(Request.QueryString["pageno"]);
                    }
                    else
                    {
                        PageNo = 0;
                    }
                }
                ShowPanel();
            }
            else
            {
                AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                DisableForm();
            }
        }

        private bool VerifyUserPermissions()
        {
            if (AddUser && IsHostMenu && !UserInfo.IsSuperUser)
            {
                AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                DisableForm();
                return false;
            }
				
            //Check if User is a member of the Current Portal (or a member of the MasterPortal if PortalGroups enabled)
            if (User.PortalID != Null.NullInteger && User.PortalID != PortalId)
            {
                AddModuleMessage("InvalidUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                DisableForm();
                return false;
            }
				
            //Check if User is a SuperUser and that the current User is a SuperUser
            if (User.IsSuperUser && !UserInfo.IsSuperUser)
            {
                AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                DisableForm();
                return false;
            }
            if (IsEdit)
            {
                //Check if user has admin rights
                if (!IsAdmin || (User.IsInRole(PortalSettings.AdministratorRoleName) && !PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)))
                {
                    AddModuleMessage("NotAuthorized", ModuleMessage.ModuleMessageType.YellowWarning, true);
                    DisableForm();
                    return false;
                }
            }
            else
            {
                if (Request.IsAuthenticated)
                {
                    if (!PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName) )
                    {
                        if (HasManageUsersModulePermission() == false)
                        {
                            //Display current user's profile
                            Response.Redirect(Globals.NavigateURL(PortalSettings.UserTabId, "", "UserID=" + UserInfo.UserID), true);
                        }
                    }
                }
                else
                {
                    if ((User.UserID > Null.NullInteger))
                    {
                        AddModuleMessage("NotAuthorized", ModuleMessage.ModuleMessageType.YellowWarning, true);
                        DisableForm();
                        return false;
                    }
                }
            }
            return true;
        }

        private void BindMembership()
        {
            ctlMembership.User = User;
            ctlMembership.DataBind();
            AddModuleMessage("UserLockedOut", ModuleMessage.ModuleMessageType.YellowWarning, ctlMembership.UserMembership.LockedOut && (!Page.IsPostBack));
            imgLockedOut.Visible = ctlMembership.UserMembership.LockedOut;
            imgOnline.Visible = ctlMembership.UserMembership.IsOnLine;
        }

        private void BindUser()
        {
            if (AddUser)
            {
                ctlUser.ShowUpdate = false;
                CheckQuota();
            }
            ctlUser.User = User;
            ctlUser.DataBind();

            //Bind the Membership
            if (AddUser || (!IsAdmin))
            {
				membershipRow.Visible = false;
            }
            else
            {
                BindMembership();
            }
        }

        private void CheckQuota()
        {
            if (PortalSettings.Users < PortalSettings.UserQuota || UserInfo.IsSuperUser || PortalSettings.UserQuota == 0)
            {
                cmdAdd.Enabled = true;
            }
            else
            {
                cmdAdd.Enabled = false;
                AddModuleMessage("ExceededUserQuota", ModuleMessage.ModuleMessageType.YellowWarning, true);
            }
        }

        private void DisableForm()
        {
            adminTabNav.Visible = false;
            dnnRoleDetails.Visible = false;
            dnnPasswordDetails.Visible = false;
            dnnProfileDetails.Visible = false;
            actionsRow.Visible = false;
            ctlMembership.Visible = false;
            ctlUser.Visible = false;
        }

        private void ShowPanel()
        {
            if (AddUser)
            {
                adminTabNav.Visible = false;
                if (Request.IsAuthenticated && MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
					//Admin adding user
					dnnManageUsers.Visible = false;
                    actionsRow.Visible = false;
                    AddModuleMessage("CannotAddUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                }
                else
                {
					dnnManageUsers.Visible = true;
                    actionsRow.Visible = true;
                }
                BindUser();
                dnnProfileDetails.Visible = false;
            }
            else
            {
                if ((!IsAdmin))
                {
                    passwordTab.Visible = false;
                }
                else
                {
                    ctlPassword.User = User;
                    ctlPassword.DataBind();
                }
                if ((!IsEdit || User.IsSuperUser))
                {
                    rolesTab.Visible = false;
                }
                else
                {
                    ctlRoles.DataBind();
                }

                BindUser();
                ctlProfile.User = User;
                ctlProfile.DataBind(); 
            }

            dnnRoleDetails.Visible = IsEdit && !User.IsSuperUser && !AddUser;
            dnnPasswordDetails.Visible = (IsAdmin) && !AddUser;
        }

		#endregion

		#region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/01/2006
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdCancel.Click += cmdCancel_Click;
            cmdAdd.Click += cmdAdd_Click;

            ctlUser.UserCreateCompleted += UserCreateCompleted;
            ctlUser.UserDeleted += UserDeleted;
            ctlUser.UserRemoved += UserRemoved;
            ctlUser.UserRestored += UserRestored;
            ctlUser.UserUpdateCompleted += UserUpdateCompleted;
            ctlUser.UserUpdateError += UserUpdateError;

            ctlProfile.ProfileUpdateCompleted += ProfileUpdateCompleted;
            ctlPassword.PasswordUpdated += PasswordUpdated;
            ctlPassword.PasswordQuestionAnswerUpdated += PasswordQuestionAnswerUpdated;
            ctlMembership.MembershipAuthorized += MembershipAuthorized;
            ctlMembership.MembershipPasswordUpdateChanged += MembershipPasswordUpdateChanged;
            ctlMembership.MembershipUnAuthorized += MembershipUnAuthorized;
            ctlMembership.MembershipUnLocked += MembershipUnLocked;
            ctlMembership.MembershipDemoteFromSuperuser += MembershipDemoteFromSuperuser;
            ctlMembership.MembershipPromoteToSuperuser += MembershipPromoteToSuperuser;
            
            jQuery.RequestDnnPluginsRegistration();

            //Set the Membership Control Properties
            ctlMembership.ID = "Membership";
            ctlMembership.ModuleConfiguration = ModuleConfiguration;
            ctlMembership.UserId = UserId;

            //Set the User Control Properties
            ctlUser.ID = "User";
            ctlUser.ModuleConfiguration = ModuleConfiguration;
            ctlUser.UserId = UserId;

            //Set the Roles Control Properties
            ctlRoles.ID = "SecurityRoles";
            ctlRoles.ModuleConfiguration = ModuleConfiguration;
            ctlRoles.ParentModule = this;

            //Set the Password Control Properties
            ctlPassword.ID = "Password";
            ctlPassword.ModuleConfiguration = ModuleConfiguration;
            ctlPassword.UserId = UserId;

            //Set the Profile Control Properties
            ctlProfile.ID = "Profile";
            ctlProfile.ModuleConfiguration = ModuleConfiguration;
            ctlProfile.UserId = UserId;

            //Customise the Control Title
            if (AddUser)
            {
                if (!Request.IsAuthenticated)
                {
					//Register
                    ModuleConfiguration.ModuleTitle = Localization.GetString("Register.Title", LocalResourceFile);
                }
                else
                {
					//Add User
                    ModuleConfiguration.ModuleTitle = Localization.GetString("AddUser.Title", LocalResourceFile);
                }

                userContainer.CssClass += " register";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/01/2006
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                //Add an Action Event Handler to the Skin
                AddActionHandler(ModuleAction_Click);

                //Bind the User information to the controls
                BindData();

                loginLink.NavigateUrl = Globals.LoginURL(RedirectURL, (Request.QueryString["override"] != null));

                if (PortalSettings.EnablePopUps)
                {
                    loginLink.Attributes.Add("onclick", "return " + UrlUtils.PopUpUrl(loginLink.NavigateUrl, this, PortalSettings, true, false, 300, 650));
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Globals.NavigateURL(), true);
        }

        private bool HasManageUsersModulePermission()
        {
            return ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "MANAGEUSER");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdRegister_Click runs when the Register button is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	05/18/2006
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void cmdAdd_Click(object sender, EventArgs e)
        {
            if (IsAdmin == false && HasManageUsersModulePermission() == false)
            {
                return;
            }
            if (ctlUser.IsValid && (ctlProfile.IsValid))
            {
                ctlUser.CreateUser();
            }
            else
            {
                if (ctlUser.CreateStatus != UserCreateStatus.AddUser)
                {
                    AddLocalizedModuleMessage(UserController.GetUserCreateStatus(ctlUser.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ModuleAction_Click handles all ModuleAction events raised from the skin
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="sender"> The object that triggers the event</param>
        /// <param name="e">An ActionEventArgs object</param>
        /// <history>
        /// 	[cnurse]	03/01/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void ModuleAction_Click(object sender, ActionEventArgs e)
        {
            switch (e.Action.CommandArgument)
            {
                case "ManageRoles":
                    //pnlRoles.Visible = true;
                    //pnlUser.Visible = false;
                    break;
                case "Cancel":
                    break;
                case "Delete":
                    break;
                case "Edit":
                    break;
                case "Save":
                    break;
                default:
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipAuthorized runs when the User has been unlocked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/01/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void MembershipAuthorized(object sender, EventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            try
            {
                AddModuleMessage("UserAuthorized", ModuleMessage.ModuleMessageType.GreenSuccess, true);
                
				//Send Notification to User
				if (string.IsNullOrEmpty(User.Membership.Password) && !MembershipProviderConfig.RequiresQuestionAndAnswer && MembershipProviderConfig.PasswordRetrievalEnabled)
                {
                    UserInfo user = User;
                    User.Membership.Password = UserController.GetPassword(ref user, "");
                }
                Mail.SendMail(User, MessageType.UserRegistrationPublic, PortalSettings);
                BindMembership();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipPasswordUpdateChanged runs when the Admin has forced the User to update their password
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	05/14/2008	created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void MembershipPasswordUpdateChanged(object sender, EventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            try
            {
                AddModuleMessage("UserPasswordUpdateChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                BindMembership();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }



        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipPromoteToSuperuser runs when the User has been promoted to a superuser
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void MembershipPromoteToSuperuser(object sender, EventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            try
            {
                AddModuleMessage("UserPromotedToSuperuser", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                BindMembership();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipDemoteFromSuperuser runs when the User has been demoted to a regular user
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void MembershipDemoteFromSuperuser(object sender, EventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            try
            {
                AddModuleMessage("UserDemotedFromSuperuser", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                BindMembership();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipUnAuthorized runs when the User has been unlocked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/01/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void MembershipUnAuthorized(object sender, EventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            try
            {
                AddModuleMessage("UserUnAuthorized", ModuleMessage.ModuleMessageType.GreenSuccess, true);

                BindMembership();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// MembershipUnLocked runs when the User has been unlocked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/01/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void MembershipUnLocked(object sender, EventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            try
            {
                AddModuleMessage("UserUnLocked", ModuleMessage.ModuleMessageType.GreenSuccess, true);
                BindMembership();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PasswordQuestionAnswerUpdated runs when the Password Q and A have been updated.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/09/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void PasswordQuestionAnswerUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            PasswordUpdateStatus status = e.UpdateStatus;
            if (status == PasswordUpdateStatus.Success)
            {
                AddModuleMessage("PasswordQAChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);
            }
            else
            {
                AddModuleMessage(status.ToString(), ModuleMessage.ModuleMessageType.RedError, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PasswordUpdated runs when the Password has been updated or reset
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/08/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void PasswordUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
            PasswordUpdateStatus status = e.UpdateStatus;

            if (status == PasswordUpdateStatus.Success)
            {
				//Send Notification to User
                try
                {
                    var accessingUser = (UserInfo) HttpContext.Current.Items["UserInfo"];
                    if (accessingUser.UserID != User.UserID)
                    {
						//The password was changed by someone else 
                        Mail.SendMail(User, MessageType.PasswordUpdated, PortalSettings);
                    }
                    else
                    {
						//The User changed his own password
                        Mail.SendMail(User, MessageType.UserUpdatedOwnPassword, PortalSettings);
                    }
                    AddModuleMessage("PasswordChanged", ModuleMessage.ModuleMessageType.GreenSuccess, true);
                }
                catch (Exception ex)
                {
                    AddModuleMessage("PasswordMailError", ModuleMessage.ModuleMessageType.YellowWarning, true);
                    Exceptions.LogException(ex);
                }
            }
            else
            {
                AddModuleMessage(status.ToString(), ModuleMessage.ModuleMessageType.RedError, true);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ProfileUpdateCompleted runs when the Profile has been updated
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/20/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void ProfileUpdateCompleted(object sender, EventArgs e)
        {
            if (IsAdmin == false)
            {
                return;
            }
			
            //Redirect to same page (this will update all controls for any changes to profile
            //and leave us at Page 0 (User Credentials)
            Response.Redirect(Request.RawUrl, true);
        }

        private void SubscriptionUpdated(object sender, MemberServices.SubscriptionUpdatedEventArgs e)
        {
            string message = Null.NullString;
            if (e.Cancel)
            {
                message = string.Format(Localization.GetString("UserUnSubscribed", LocalResourceFile), e.RoleName);
            }
            else
            {
                message = string.Format(Localization.GetString("UserSubscribed", LocalResourceFile), e.RoleName);
            }
            AddLocalizedModuleMessage(message, ModuleMessage.ModuleMessageType.GreenSuccess, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserCreateCompleted runs when a new user has been Created
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/06/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void UserCreateCompleted(object sender, UserUserControlBase.UserCreatedEventArgs e)
        {
            try
            {
                if (e.CreateStatus == UserCreateStatus.Success)
                {
                    CompleteUserCreation(e.CreateStatus, e.NewUser, e.Notify, false);
                    Response.Redirect(ReturnUrl, true);
                }
                else
                {
                    AddLocalizedModuleMessage(UserController.GetUserCreateStatus(e.CreateStatus), ModuleMessage.ModuleMessageType.RedError, true);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserDeleted runs when the User has been deleted
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/01/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void UserDeleted(object sender, UserUserControlBase.UserDeletedEventArgs e)
        {
            try
            {
                Response.Redirect(ReturnUrl, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserUpdateCompleted runs when a user has been updated
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	3/02/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void UserRestored(object sender, UserUserControlBase.UserRestoredEventArgs e)
        {
            try
            {
                Response.Redirect(ReturnUrl, true);
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void UserRemoved(object sender, UserUserControlBase.UserRemovedEventArgs e)
        {
            try
            {
                Response.Redirect(ReturnUrl, true);
                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void UserUpdateCompleted(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserUpdateError runs when there is an error updating the user
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	2/07/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void UserUpdateError(object sender, UserUserControlBase.UserUpdateErrorArgs e)
        {
            AddModuleMessage(e.Message, ModuleMessage.ModuleMessageType.RedError, true);
        }
		
		#endregion
    }
}
