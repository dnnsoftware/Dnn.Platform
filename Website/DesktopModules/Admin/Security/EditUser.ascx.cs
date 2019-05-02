#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.Admin.Security;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.UI.Skins.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ManageUsers UserModuleBase is used to manage Users
    /// </summary>
    /// <remarks>
    /// </remarks>
    public partial class EditUser : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EditUser));

        #region Protected Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether to display the Manage Services tab
        /// </summary>
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
        protected string RedirectURL
        {
            get
            {
                string _RedirectURL = "";

                if (PortalSettings.Registration.RedirectAfterRegistration == Null.NullInteger)
                {
                    if (Request.QueryString["returnurl"] != null)
                    {
                        //return to the url passed to register
                        _RedirectURL = HttpUtility.UrlDecode(Request.QueryString["returnurl"]);

                        //clean the return url to avoid possible XSS attack.
                        _RedirectURL = UrlUtils.ValidReturnUrl(_RedirectURL);

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
                    _RedirectURL = Globals.NavigateURL(PortalSettings.Registration.RedirectAfterRegistration);
                }
                return _RedirectURL;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url for the page
        /// </summary>
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

        public bool ShowVanityUrl { get; private set; }

        #endregion

        #region Private Methods

        private void BindData()
        {
            if (User != null)
            {
                //If trying to add a SuperUser - check that user is a SuperUser
                if (VerifyUserPermissions() == false)
                {
                    return;
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
                userForm.DataSource = User;


                // hide username field in UseEmailAsUserName mode
                bool disableUsername = PortalController.GetPortalSettingAsBoolean("Registration_UseEmailAsUserName", PortalId, false);
                if (disableUsername)
                {
                    userForm.Items[0].Visible = false;
                }

                if (!Page.IsPostBack)
                {
                    userForm.DataBind();
                }

                ctlPassword.User = User;
                ctlPassword.DataBind();

                if ((!DisplayServices))
                {
                    servicesTab.Visible = false;
                }
                else
                {
                    ctlServices.User = User;
                    ctlServices.DataBind();
                }

                BindUser();
                ctlProfile.User = User;
                ctlProfile.DataBind();

                dnnServicesDetails.Visible = DisplayServices;

                var urlSettings = new DotNetNuke.Entities.Urls.FriendlyUrlSettings(PortalSettings.PortalId);
                var showVanityUrl = (Config.GetFriendlyUrlProvider() == "advanced") && !User.IsSuperUser;
                if (showVanityUrl)
                {
                    VanityUrlRow.Visible = true;
                    if (String.IsNullOrEmpty(User.VanityUrl))
                    {
                        //Clean Display Name
                        bool modified;
                        var options = UrlRewriterUtils.GetOptionsFromSettings(urlSettings);
                        var cleanUrl = FriendlyUrlController.CleanNameForUrl(User.DisplayName, options, out modified);
                        var uniqueUrl = FriendlyUrlController.ValidateUrl(cleanUrl, -1, PortalSettings, out modified).ToLowerInvariant();

                        VanityUrlAlias.Text = String.Format("{0}/{1}/", PortalSettings.PortalAlias.HTTPAlias, urlSettings.VanityUrlPrefix);
                        VanityUrlTextBox.Text = uniqueUrl;
                        ShowVanityUrl = true;
                    }
                    else
                    {
                        VanityUrl.Text = String.Format("{0}/{1}/{2}", PortalSettings.PortalAlias.HTTPAlias, urlSettings.VanityUrlPrefix, User.VanityUrl);
                        ShowVanityUrl = false;
                    }
                }
            }
            else
            {
                AddModuleMessage("NoUser", ModuleMessage.ModuleMessageType.YellowWarning, true);
                DisableForm();
            }
        }

        private bool VerifyUserPermissions()
        {
            if (IsHostMenu && !UserInfo.IsSuperUser)
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
                if (!IsUser)
                {
                    if (Request.IsAuthenticated)
                    {
                        if (!PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName))
                        {
                            //Display current user's profile
                            Response.Redirect(Globals.NavigateURL(PortalSettings.UserTabId, "", "UserID=" + UserInfo.UserID), true);
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
            }
            return true;
        }

        private void BindMembership()
        {
            ctlMembership.User = User;
            ctlMembership.DataBind();
            AddModuleMessage("UserLockedOut", ModuleMessage.ModuleMessageType.YellowWarning, ctlMembership.UserMembership.LockedOut && (!Page.IsPostBack));
        }

        private void BindUser()
        {
            BindMembership();

        }

        private void DisableForm()
        {
            adminTabNav.Visible = false;
            dnnProfileDetails.Visible = false;
            dnnServicesDetails.Visible = false;
            actionsRow.Visible = false;
            ctlMembership.Visible = false;
        }

        private void UpdateDisplayName()
        {
            //Update DisplayName to conform to Format
            if (!string.IsNullOrEmpty(PortalSettings.Registration.DisplayNameFormat))
            {
                User.UpdateDisplayName(PortalSettings.Registration.DisplayNameFormat);
            }
        }


        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdDelete.Click += cmdDelete_Click;
            cmdUpdate.Click += cmdUpdate_Click;

            ctlServices.SubscriptionUpdated += SubscriptionUpdated;
            ctlProfile.ProfileUpdateCompleted += ProfileUpdateCompleted;
            ctlPassword.PasswordUpdated += PasswordUpdated;
            ctlPassword.PasswordQuestionAnswerUpdated += PasswordQuestionAnswerUpdated;

            email.ValidationExpression = PortalSettings.Registration.EmailValidator;

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);
            JavaScript.RequestRegistration(CommonJs.Knockout);


            //Set the Membership Control Properties
            ctlMembership.ID = "Membership";
            ctlMembership.ModuleConfiguration = ModuleConfiguration;
            ctlMembership.UserId = UserId;

            //Set the Password Control Properties
            ctlPassword.ID = "Password";
            ctlPassword.ModuleConfiguration = ModuleConfiguration;
            ctlPassword.UserId = UserId;

            //Set the Profile Control Properties
            ctlProfile.ID = "Profile";
            ctlProfile.ModuleConfiguration = ModuleConfiguration;
            ctlProfile.UserId = UserId;

            //Set the Services Control Properties
            ctlServices.ID = "MemberServices";
            ctlServices.ModuleConfiguration = ModuleConfiguration;
            ctlServices.UserId = UserId;

            //Define DisplayName filed Enabled Property:
            object setting = GetSetting(UserPortalID, "Security_DisplayNameFormat");
            if ((setting != null) && (!string.IsNullOrEmpty(Convert.ToString(setting))))
            {
                displayName.Enabled = false;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                //Bind the User information to the controls
                BindData();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void cmdDelete_Click(object sender, EventArgs e)
        {
            UserInfo user = User;
            var success = false;
            if (PortalSettings.DataConsentActive && user.UserID == UserInfo.UserID)
            {
                switch (PortalSettings.DataConsentUserDeleteAction)
                {
                    case PortalSettings.UserDeleteAction.Manual:
                        user.Membership.Approved = false;
                        UserController.UpdateUser(PortalSettings.PortalId, user);
                        UserController.UserRequestsRemoval(user, true);
                        success = true;
                        break;
                    case PortalSettings.UserDeleteAction.DelayedHardDelete:
                        success = UserController.DeleteUser(ref user, true, false);
                        UserController.UserRequestsRemoval(user, true);
                        break;
                    case PortalSettings.UserDeleteAction.HardDelete:
                        success = UserController.RemoveUser(user);
                        break;
                    default: // if user delete is switched off under Data Consent then we revert to the old behavior
                        success = UserController.DeleteUser(ref user, true, false);
                        break;
                }
            }
            else
            {
                success = UserController.DeleteUser(ref user, true, false);
            }
            if (!success)
            {
                AddModuleMessage("UserDeleteError", ModuleMessage.ModuleMessageType.RedError, true);
            }

            //DNN-26777 
            PortalSecurity.Instance.SignOut();
            Response.Redirect(Globals.NavigateURL(PortalSettings.HomeTabId));
        }

        protected void cmdUpdate_Click(object sender, EventArgs e)
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

                    //DNN-5874 Check if unique display name is required
                    if (PortalSettings.Registration.RequireUniqueDisplayName)
                    {
                        var usersWithSameDisplayName = (List<UserInfo>)MembershipProvider.Instance().GetUsersBasicSearch(PortalId, 0, 2, "DisplayName", true, "DisplayName", User.DisplayName);
                        if (usersWithSameDisplayName.Any(user => user.UserID != User.UserID))
                        {
                            throw new Exception("Display Name must be unique");
                        }
                    }

                    UserController.UpdateUser(UserPortalID, User);

                    // make sure username matches possibly changed email address
                    if (PortalSettings.Registration.UseEmailAsUserName)
                    {
                        if (User.Username.ToLower() != User.Email.ToLower())
                        {
                            UserController.ChangeUsername(User.UserID, User.Email);

                            //after username changed, should redirect to login page to let user authenticate again.
                            var loginUrl = Globals.LoginURL(HttpUtility.UrlEncode(Request.RawUrl), false);
                            var spliter = loginUrl.Contains("?") ? "&" : "?";
                            loginUrl = $"{loginUrl}{spliter}username={User.Email}&usernameChanged=true";
                            Response.Redirect(loginUrl, true);
                        }
                    }

                    Response.Redirect(Request.RawUrl);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    if (exc.Message == "Display Name must be unique")
                    {
                        AddModuleMessage("DisplayNameNotUnique", ModuleMessage.ModuleMessageType.RedError, true);
                    }
                    else
                    {
                        AddModuleMessage("UserUpdatedError", ModuleMessage.ModuleMessageType.RedError, true);
                    }
                }
            }

        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// PasswordQuestionAnswerUpdated runs when the Password Q and A have been updated.
        /// </summary>
        /// <remarks>
        /// </remarks>
        private void PasswordQuestionAnswerUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            if (IsUserOrAdmin == false)
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
        private void PasswordUpdated(object sender, Password.PasswordUpdatedEventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            PasswordUpdateStatus status = e.UpdateStatus;

            if (status == PasswordUpdateStatus.Success)
            {
                //Send Notification to User
                try
                {
                    var accessingUser = (UserInfo)HttpContext.Current.Items["UserInfo"];
                    if (accessingUser.UserID != User.UserID)
                    {
                        //The password was changed by someone else 
                        Mail.SendMail(User, MessageType.PasswordReminder, PortalSettings);
                    }
                    else
                    {
                        //The User changed his own password
                        Mail.SendMail(User, MessageType.UserUpdatedOwnPassword, PortalSettings);
                        PortalSecurity.Instance.SignIn(User, false);
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
        private void ProfileUpdateCompleted(object sender, EventArgs e)
        {
            if (IsUserOrAdmin == false)
            {
                return;
            }
            if (IsUser)
            {
                //Notify the user that his/her profile was updated
                Mail.SendMail(User, MessageType.ProfileUpdated, PortalSettings);

                ProfilePropertyDefinition localeProperty = User.Profile.GetProperty("PreferredLocale");
                if (localeProperty.IsDirty)
                {
                    //store preferredlocale in cookie, if none specified set to portal default.
                    if (User.Profile.PreferredLocale == string.Empty)
                    {
                        Localization.SetLanguage(PortalController.GetPortalDefaultLanguage(User.PortalID));
                    }
                    else
                    {
                        Localization.SetLanguage(User.Profile.PreferredLocale);
                    }
                }
            }

            //Redirect to same page (this will update all controls for any changes to profile
            //and leave us at Page 0 (User Credentials)
            Response.Redirect(Request.RawUrl, true);
        }

        private void SubscriptionUpdated(object sender, MemberServices.SubscriptionUpdatedEventArgs e)
        {
            string message;
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

        #endregion
    }
}
