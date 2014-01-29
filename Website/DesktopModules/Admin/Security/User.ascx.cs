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
using System.Collections;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.UI.WebControls;

using Telerik.Web.UI;

using DataCache = DotNetNuke.Common.Utilities.DataCache;
using Globals = DotNetNuke.Common.Globals;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The User UserModuleBase is used to manage the base parts of a User.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	03/01/2006  created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class User : UserUserControlBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (User));
		#region Public Properties

        public UserCreateStatus CreateStatus { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the User is valid
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/21/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool IsValid
        {
            get
            {
                return Validate();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the Password section is displayed
        /// </summary>
        /// <history>
        /// 	[cnurse]	07/17/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool ShowPassword
        {
            get
            {
                return Password.Visible;
            }
            set
            {
                Password.Visible = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the Update button
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/18/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool ShowUpdate
        {
            get
            {
                return actionsRow.Visible;
            }
            set
            {
                actionsRow.Visible = value;
            }
        }

		/// <summary>
		/// User Form's css class.
		/// </summary>
    	public string CssClass
    	{
    		get
    		{
				return pnlAddUser.CssClass;
    		}
			set
			{
				userForm.CssClass = string.IsNullOrEmpty(userForm.CssClass) ? value : string.Format("{0} {1}", userForm.CssClass, value);
				pnlAddUser.CssClass = string.IsNullOrEmpty(pnlAddUser.CssClass) ? value : string.Format("{0} {1}", pnlAddUser.CssClass, value); ;
			}
    	}

		#endregion

		#region Private Methods

        /// <summary>
        /// method checks to see if its allowed to change the username
        /// valid if a host, or an admin where the username is in only 1 portal
        /// </summary>
        /// <returns></returns>
        private bool CanUpdateUsername()
        {
            //do not allow for non-logged in users
            if (Request.IsAuthenticated==false || AddUser)
            {
                return false;
            }

            //can only update username if a host/admin and account being managed is not a superuser
            if (UserController.GetCurrentUserInfo().IsSuperUser)
            {
                //only allow updates for non-superuser accounts
                if (User.IsSuperUser==false)
                {
                    return true;
                }
            }

            //if an admin, check if the user is only within this portal
            if (UserController.GetCurrentUserInfo().IsInRole(PortalSettings.AdministratorRoleName))
            {
                //only allow updates for non-superuser accounts
                if (User.IsSuperUser)
                {
                    return false;
                }
                if (PortalController.GetPortalsByUser(User.UserID).Count == 1) return true;
            }

            return false;
        }

        private void UpdateDisplayName()
        {
			//Update DisplayName to conform to Format
            object setting = GetSetting(UserPortalID, "Security_DisplayNameFormat");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                User.UpdateDisplayName(Convert.ToString(setting));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validate validates the User
        /// </summary>
        /// <history>
        /// 	[cnurse]	08/10/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private bool Validate()
        {
            //Check User Editor
            bool _IsValid = userForm.IsValid;

            //Check Password is valid
            if (AddUser && ShowPassword)
            {
                CreateStatus = UserCreateStatus.AddUser;
                if (!chkRandom.Checked)
                {					
					//1. Check Password is Valid
                    if (CreateStatus == UserCreateStatus.AddUser && !UserController.ValidatePassword(txtPassword.Text))
                    {
                        CreateStatus = UserCreateStatus.InvalidPassword;
                    }
                    if (CreateStatus == UserCreateStatus.AddUser)
                    {
                        User.Membership.Password = txtPassword.Text;
                    }
                }
                else
                {
					//Generate a random password for the user
                    User.Membership.Password = UserController.GeneratePassword();
                }
				
                //Check Question/Answer
                if (CreateStatus == UserCreateStatus.AddUser && MembershipProviderConfig.RequiresQuestionAndAnswer)
                {
                    if (string.IsNullOrEmpty(txtQuestion.Text))
                    {
						//Invalid Question
                        CreateStatus = UserCreateStatus.InvalidQuestion;
                    }
                    else
                    {
                        User.Membership.PasswordQuestion = txtQuestion.Text;
                    }
                    if (CreateStatus == UserCreateStatus.AddUser)
                    {
                        if (string.IsNullOrEmpty(txtAnswer.Text))
                        {
							//Invalid Question
                            CreateStatus = UserCreateStatus.InvalidAnswer;
                        }
                        else
                        {
                            User.Membership.PasswordAnswer = txtAnswer.Text;
                        }
                    }
                }
                if (CreateStatus != UserCreateStatus.AddUser)
                {
                    _IsValid = false;
                }
            }
            return _IsValid;
        }

		#endregion

		#region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateUser creates a new user in the Database
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/18/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void CreateUser()
        {
            //Update DisplayName to conform to Format
            UpdateDisplayName();

            if (IsRegister)
            {
                User.Membership.Approved = PortalSettings.UserRegistration == (int) Globals.PortalRegistrationType.PublicRegistration;
            }
            else
            {
                //Set the Approved status from the value in the Authorized checkbox
                User.Membership.Approved = chkAuthorize.Checked;
            }
            var user = User;
            var createStatus = UserController.CreateUser(ref user);

            var args = (createStatus == UserCreateStatus.Success)
                                            ? new UserCreatedEventArgs(User) {Notify = chkNotify.Checked} 
                                            : new UserCreatedEventArgs(null);
            args.CreateStatus = createStatus;
            OnUserCreated(args);
            OnUserCreateCompleted(args);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DataBind binds the data to the controls
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override void DataBind()
        {
            if (Page.IsPostBack == false)
            {
                string confirmString = Localization.GetString("DeleteItem");
                if (IsUser)
                {
                    confirmString = Localization.GetString("ConfirmUnRegister", LocalResourceFile);
                }
                ClientAPI.AddButtonConfirm(cmdDelete, confirmString);
                chkRandom.Checked = false;
            }

            cmdDelete.Visible = false;
            cmdRemove.Visible = false;
            cmdRestore.Visible = false;
            if (!AddUser)
            {
                var deletePermitted = (User.UserID != PortalSettings.AdministratorId) && !(IsUser && User.IsSuperUser);
                if ((deletePermitted))
                {
                    if ((User.IsDeleted))
                    {
                        cmdRemove.Visible = true;
                        cmdRestore.Visible = true;
                    }
                    else
                    {
                        cmdDelete.Visible = true;
                    }
                }
            }

            cmdUpdate.Text = Localization.GetString(IsUser ? "Register" : "CreateUser", LocalResourceFile);
            cmdDelete.Text = Localization.GetString(IsUser ? "UnRegister" : "Delete", LocalResourceFile);
            if (AddUser)
            {
                pnlAddUser.Visible = true;
                if (IsRegister)
                {
                    AuthorizeNotify.Visible = false;
                    randomRow.Visible = false;
                    if (ShowPassword)
                    {
                        questionRow.Visible = MembershipProviderConfig.RequiresQuestionAndAnswer;
                        answerRow.Visible = MembershipProviderConfig.RequiresQuestionAndAnswer;
                        lblPasswordHelp.Text = Localization.GetString("PasswordHelpUser", LocalResourceFile);
                    }
                }
                else
                {
                    lblPasswordHelp.Text = Localization.GetString("PasswordHelpAdmin", LocalResourceFile);
                }
                txtConfirm.Attributes.Add("value", txtConfirm.Text);
                txtPassword.Attributes.Add("value", txtPassword.Text);
            }

            userNameReadOnly.Visible = !AddUser;
            userName.Visible = AddUser;
            
            if (CanUpdateUsername())
            {
               
                renameUserName.Visible = true;
                
                userName.Visible = false;
                userNameReadOnly.Visible = false;

                ArrayList portals = PortalController.GetPortalsByUser(User.UserID);
                if (portals.Count>1)
                {
                    numSites.Text=String.Format(Localization.GetString("UpdateUserName", LocalResourceFile), portals.Count.ToString());
                    cboSites.Visible = true;
                    cboSites.DataSource = portals;
                    cboSites.DataTextField = "PortalName";
                    cboSites.DataBind();

                    renameUserPortals.Visible = true;
                }
            }

            var userNameSetting = GetSetting(UserPortalID, "Security_UserNameValidation");
            if ((userNameSetting != null) && (!string.IsNullOrEmpty(Convert.ToString(userNameSetting))))
            {
                userName.ValidationExpression = Convert.ToString(userNameSetting);
            }

            var setting = GetSetting(UserPortalID, "Security_EmailValidation");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                email.ValidationExpression = Convert.ToString(setting);
            }

            setting = GetSetting(UserPortalID, "Security_DisplayNameFormat");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                if (AddUser)
                {
                    displayNameReadOnly.Visible = false;
                    displayName.Visible = false;
                }
                else
                {
                    displayNameReadOnly.Visible = true;
                    displayName.Visible = false;
                }
                firstName.Visible = true;
                lastName.Visible = true;
            }
            else
            {
                displayNameReadOnly.Visible = false;
                displayName.Visible = true;
                firstName.Visible = false;
                lastName.Visible = false;
            }

            userForm.DataSource = User;
			if (!Page.IsPostBack)
			{
				userForm.DataBind();
			    renameUserName.Value = User.Username;
			}
        }

		#endregion

		#region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.jquery.tooltip.js");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Shared/scripts/dnn.PasswordStrength.js");
			ClientResourceManager.RegisterScript(Page, "~/DesktopModules/Admin/Security/Scripts/dnn.PasswordComparer.js");

            jQuery.RequestDnnPluginsRegistration();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;
            cmdRemove.Click += cmdRemove_Click;
            cmdRestore.Click += cmdRestore_Click;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

			if (Host.EnableStrengthMeter)
			{
				passwordContainer.CssClass = "password-strength-container";
				txtPassword.CssClass = "password-strength";
				txtConfirm.CssClass = string.Format("{0} checkStength", txtConfirm.CssClass);
				
				var options = new DnnPaswordStrengthOptions();
				var optionsAsJsonString = Json.Serialize(options);
				var passwordScript = string.Format("dnn.initializePasswordStrength('.{0}', {1});{2}",
					"password-strength", optionsAsJsonString, Environment.NewLine);

				if (ScriptManager.GetCurrent(Page) != null)
				{
					// respect MS AJAX
					ScriptManager.RegisterStartupScript(Page, GetType(), "PasswordStrength", passwordScript, true);
				}
				else
				{
					Page.ClientScript.RegisterStartupScript(GetType(), "PasswordStrength", passwordScript, true);
				}
			}

			var confirmPasswordOptions = new DnnConfirmPasswordOptions()
			{
				FirstElementSelector = "#" + passwordContainer.ClientID + " input[type=password]",
				SecondElementSelector = ".password-confirm",
				ContainerSelector = ".dnnFormPassword",
				UnmatchedCssClass = "unmatched",
				MatchedCssClass = "matched"
			};

			var confirmOptionsAsJsonString = Json.Serialize(confirmPasswordOptions);
			var confirmScript = string.Format("dnn.initializePasswordComparer({0});{1}", confirmOptionsAsJsonString, Environment.NewLine);

			if (ScriptManager.GetCurrent(Page) != null)
			{
				// respect MS AJAX
				ScriptManager.RegisterStartupScript(Page, GetType(), "ConfirmPassword", confirmScript, true);
			}
			else
			{
				Page.ClientScript.RegisterStartupScript(GetType(), "ConfirmPassword", confirmScript, true);
			}
        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdDelete_Click runs when the delete Button is clicked
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdDelete_Click(Object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            string name = User.Username;
            int id = UserId;
            UserInfo user = User;
            if (UserController.DeleteUser(ref user, true, false))
            {
                OnUserDeleted(new UserDeletedEventArgs(id, name));
            }
            else
            {
                OnUserDeleteError(new UserUpdateErrorArgs(id, name, "UserDeleteError"));
            }
        }

        private void cmdRestore_Click(Object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            var name = User.Username;
            var id = UserId;

            var userInfo = User;
            if (UserController.RestoreUser(ref userInfo))
            {
                OnUserRestored(new UserRestoredEventArgs(id, name));
            }
            else
            {
                OnUserRestoreError(new UserUpdateErrorArgs(id, name, "UserRestoreError"));
            }
        }

        private void cmdRemove_Click(Object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            var name = User.Username;
            var id = UserId;

            if (UserController.RemoveUser(User))
            {
                OnUserRemoved(new UserRemovedEventArgs(id, name));
            }
            else
            {
                OnUserRemoveError(new UserUpdateErrorArgs(id, name, "UserRemoveError"));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdUpdate_Click runs when the Update Button is clicked
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/01/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdUpdate_Click(Object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }

            if (AddUser)
            {
                if (IsValid)
                {
                    CreateUser();
                    DataCache.ClearPortalCache(PortalId, true);
                }
            }
            else
            {
                if (userForm.IsValid && (User != null))
                {
                    if (User.UserID == PortalSettings.AdministratorId)
                    {
						//Clear the Portal Cache
                        DataCache.ClearPortalCache(UserPortalID, true);
                    }
                    try
                    {
						//Update DisplayName to conform to Format
                        UpdateDisplayName();
                        //either update the username or update the user details
                        if (CanUpdateUsername())
                        {
                            UserController.ChangeUsername(User.UserID, renameUserName.Value.ToString());
                        }

                        UserController.UpdateUser(UserPortalID, User);
                        OnUserUpdated(EventArgs.Empty);
                        OnUserUpdateCompleted(EventArgs.Empty);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);

                        var args = new UserUpdateErrorArgs(User.UserID, User.Username, "EmailError");
                        OnUserUpdateError(args);
                    }
                }
            }
        }
		
		#endregion
    }
}