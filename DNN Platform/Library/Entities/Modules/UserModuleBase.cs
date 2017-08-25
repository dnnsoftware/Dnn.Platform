#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Caching;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Entities.Modules
{
    public enum DisplayMode
    {
        All = 0,
        FirstLetter = 1,
        None = 2
    }

    public enum UsersControl
    {
        Combo = 0,
        TextBox = 1
    }

    /// <summary>
    /// The UserModuleBase class defines a custom base class inherited by all
    /// desktop portal modules within the Portal that manage Users.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class UserModuleBase : PortalModuleBase
    {
        private UserInfo _User;

        /// <summary>
        /// Gets whether we are in Add User mode
        /// </summary>
        protected virtual bool AddUser
        {
            get
            {
                return (UserId == Null.NullInteger);
            }
        }

        /// <summary>
        /// Gets whether the current user is an Administrator (or SuperUser)
        /// </summary>
        protected bool IsAdmin
        {
            get
            {
                return Request.IsAuthenticated && PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName);
            }
        }

        /// <summary>
        /// gets whether this is the current user or admin
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool IsUserOrAdmin
        {
            get
            {
                return IsUser || IsAdmin;
            }
        }

        /// <summary>
        /// Gets whether this control is in the Host menu
        /// </summary>
        protected bool IsHostTab
        {
            get
            {
                return base.IsHostMenu;
            }
        }

        /// <summary>
        /// Gets whether the control is being called form the User Accounts module
        /// </summary>
        protected bool IsEdit
        {
            get
            {
                bool _IsEdit = false;
                if (Request.QueryString["ctl"] != null)
                {
                    string ctl = Request.QueryString["ctl"];
                    if (ctl.ToLowerInvariant() == "edit")
                    {
                        _IsEdit = true;
                    }
                }
                return _IsEdit;
            }
        }

        /// <summary>
        /// Gets whether the current user is modifying their profile
        /// </summary>
        protected bool IsProfile
        {
            get
            {
                bool _IsProfile = false;
                if (IsUser)
                {
                    if (PortalSettings.UserTabId != -1)
                    {
						//user defined tab
                        if (PortalSettings.ActiveTab.TabID == PortalSettings.UserTabId)
                        {
                            _IsProfile = true;
                        }
                    }
                    else
                    {
						//admin tab
                        if (Request.QueryString["ctl"] != null)
                        {
                            string ctl = Request.QueryString["ctl"];
                            if (ctl.ToLowerInvariant() == "profile")
                            {
                                _IsProfile = true;
                            }
                        }
                    }
                }
                return _IsProfile;
            }
        }

        /// <summary>
        /// Gets whether an anonymous user is trying to register
        /// </summary>
        protected bool IsRegister
        {
            get
            {
                return !IsAdmin && !IsUser;
            }
        }

        /// <summary>
        /// Gets whether the User is editing their own information
        /// </summary>
        protected bool IsUser
        {
            get
            {
                return Request.IsAuthenticated && (UserId == UserInfo.UserID);
            }
        }

        /// <summary>
        /// Gets the PortalId to use for this control
        /// </summary>
        protected int UserPortalID
        {
            get
            {
                return IsHostTab ? Null.NullInteger : PortalId;
            }
        }

        /// <summary>
        /// Gets and sets the User associated with this control
        /// </summary>
        public UserInfo User
        {
            get
            {
                return _User ?? (_User = AddUser ? InitialiseUser() : UserController.GetUserById(UserPortalID, UserId));
            }
            set
            {
                _User = value;
                if (_User != null)
                {
                    UserId = _User.UserID;
                }
            }
        }

        /// <summary>
        /// Gets and sets the UserId associated with this control
        /// </summary>
        public new int UserId
        {
            get
            {
                int _UserId = Null.NullInteger;
                if (ViewState["UserId"] == null)
                {
                    if (Request.QueryString["userid"] != null)
                    {
                        int userId;
                        // Use Int32.MaxValue as invalid UserId
                        _UserId = Int32.TryParse(Request.QueryString["userid"], out userId) ? userId : Int32.MaxValue;
                        ViewState["UserId"] = _UserId;
                    }
                }
                else
                {
                    _UserId = Convert.ToInt32(ViewState["UserId"]);
                }
                return _UserId;
            }
            set
            {
                ViewState["UserId"] = value;
            }
        }

        /// <summary>
        /// Gets a Setting for the Module
        /// </summary>
        /// <remarks>
        /// </remarks>
        public static object GetSetting(int portalId, string settingKey)
        {
            Hashtable settings = UserController.GetUserSettings(portalId);
            if (settings[settingKey] == null)
            {
                settings = UserController.GetUserSettings(portalId, settings);
            }
            return settings[settingKey];
        }

        public static void UpdateSetting(int portalId, string key, string setting)
        {
            if (portalId == Null.NullInteger)
            {
                HostController.Instance.Update(new ConfigurationSetting {Value = setting, Key = key});
            }
            else
            {
                PortalController.UpdatePortalSetting(portalId, key, setting);
            }
        }

        /// <summary>
        /// Updates the Settings for the Module
        /// </summary>
        public static void UpdateSettings(int portalId, Hashtable settings)
        {
            string key;
            string setting;
            IDictionaryEnumerator settingsEnumerator = settings.GetEnumerator();
            while (settingsEnumerator.MoveNext())
            {
                key = Convert.ToString(settingsEnumerator.Key);
                setting = Convert.ToString(settingsEnumerator.Value);
                UpdateSetting(portalId, key, setting);
            }
        }

        /// <summary>
        /// InitialiseUser initialises a "new" user
        /// </summary>
        private UserInfo InitialiseUser()
        {
            var newUser = new UserInfo();
            if (IsHostMenu && !IsRegister)
            {
                newUser.IsSuperUser = true;
            }
            else
            {
                newUser.PortalID = PortalId;
            }

            //Initialise the ProfileProperties Collection
            string lc = new Localization().CurrentUICulture;

            newUser.Profile.InitialiseProfile(PortalId);
            newUser.Profile.PreferredTimeZone = PortalSettings.TimeZone;

            newUser.Profile.PreferredLocale = lc;

            //Set default countr
            string country = Null.NullString;
            country = LookupCountry();
            if (!String.IsNullOrEmpty(country))
            {
                ListController listController = new ListController();
                var listitem = listController.GetListEntryInfo("Country", country);
                if (listitem != null)
                {
                    country = listitem.EntryID.ToString();
                }

                newUser.Profile.Country = country;
            }
            //Set AffiliateId
            int AffiliateId = Null.NullInteger;
            if (Request.Cookies["AffiliateId"] != null)
            {
                AffiliateId = int.Parse(Request.Cookies["AffiliateId"].Value);
            }
            newUser.AffiliateID = AffiliateId;
            return newUser;
        }

        private string LookupCountry()
        {
            string IP;
            bool IsLocal = false;
            bool _CacheGeoIPData = true;
            string _GeoIPFile;
            _GeoIPFile = "controls/CountryListBox/Data/GeoIP.dat";
            if (Page.Request.UserHostAddress == "127.0.0.1")
            {
				//'The country cannot be detected because the user is local.
                IsLocal = true;
                //Set the IP address in case they didn't specify LocalhostCountryCode
                IP = Page.Request.UserHostAddress;
            }
            else
            {
				//Set the IP address so we can find the country
                IP = Page.Request.UserHostAddress;
            }
            //Check to see if we need to generate the Cache for the GeoIPData file
            if (Context.Cache.Get("GeoIPData") == null && _CacheGeoIPData)
            {
				//Store it as	well as	setting	a dependency on	the	file
                Context.Cache.Insert("GeoIPData", CountryLookup.FileToMemory(Context.Server.MapPath(_GeoIPFile)), new CacheDependency(Context.Server.MapPath(_GeoIPFile)));
            }
			
            //Check to see if the request is a localhost request
            //and see if the LocalhostCountryCode is specified
            if (IsLocal)
            {
                return Null.NullString;
            }
			
            //Either this is a remote request or it is a local
            //request with no LocalhostCountryCode specified
            CountryLookup _CountryLookup;

            //Check to see if we are using the Cached
            //version of the GeoIPData file
            if (_CacheGeoIPData)
            {
				//Yes, get it from cache
                _CountryLookup = new CountryLookup((MemoryStream) Context.Cache.Get("GeoIPData"));
            }
            else
            {
				//No, get it from file
                _CountryLookup = new CountryLookup(Context.Server.MapPath(_GeoIPFile));
            }
            //Get the country code based on the IP address
            string country = Null.NullString;
            try
            {
                country = _CountryLookup.LookupCountryName(IP);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            return country;
        }

        /// <summary>
        /// AddLocalizedModuleMessage adds a localized module message
        /// </summary>
        /// <param name="message">The localized message</param>
        /// <param name="type">The type of message</param>
        /// <param name="display">A flag that determines whether the message should be displayed</param>
        protected void AddLocalizedModuleMessage(string message, ModuleMessage.ModuleMessageType type, bool display)
        {
            if (display)
            {
                UI.Skins.Skin.AddModuleMessage(this, message, type);
            }
        }

        /// <summary>
        /// AddModuleMessage adds a module message
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="type">The type of message</param>
        /// <param name="display">A flag that determines whether the message should be displayed</param>
        protected void AddModuleMessage(string message, ModuleMessage.ModuleMessageType type, bool display)
        {
            AddLocalizedModuleMessage(Localization.GetString(message, LocalResourceFile), type, display);
        }

        protected string CompleteUserCreation(UserCreateStatus createStatus, UserInfo newUser, bool notify, bool register)
        {
            var strMessage = "";
            var message = ModuleMessage.ModuleMessageType.RedError;
            if (register)
            {
				//send notification to portal administrator of new user registration
				//check the receive notification setting first, but if register type is Private, we will always send the notification email.
				//because the user need administrators to do the approve action so that he can continue use the website.
				if (PortalSettings.EnableRegisterNotification || PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration)
				{
				    strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationAdmin, PortalSettings);
				    SendAdminNotification(newUser, PortalSettings);
				}

                var loginStatus = UserLoginStatus.LOGIN_FAILURE;

                //complete registration
                switch (PortalSettings.UserRegistration)
                {
                    case (int) Globals.PortalRegistrationType.PrivateRegistration:
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPrivate, PortalSettings);

                        //show a message that a portal administrator has to verify the user credentials
                        if (string.IsNullOrEmpty(strMessage))
                        {
                            strMessage += Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile);
                            message = ModuleMessage.ModuleMessageType.GreenSuccess;
                        }
                        break;
                    case (int) Globals.PortalRegistrationType.PublicRegistration:
                        Mail.SendMail(newUser, MessageType.UserRegistrationPublic, PortalSettings);
                        UserController.UserLogin(PortalSettings.PortalId, newUser.Username, newUser.Membership.Password, "", PortalSettings.PortalName, "", ref loginStatus, false);
                        break;
                    case (int) Globals.PortalRegistrationType.VerifiedRegistration:
                        Mail.SendMail(newUser, MessageType.UserRegistrationVerified, PortalSettings);
                        UserController.UserLogin(PortalSettings.PortalId, newUser.Username, newUser.Membership.Password, "", PortalSettings.PortalName, "", ref loginStatus, false);
                        break;
                }
                //store preferredlocale in cookie
                Localization.SetLanguage(newUser.Profile.PreferredLocale);
                if (IsRegister && message == ModuleMessage.ModuleMessageType.RedError)
                {
                    AddLocalizedModuleMessage(string.Format(Localization.GetString("SendMail.Error", Localization.SharedResourceFile), strMessage), message, (!String.IsNullOrEmpty(strMessage)));
                }
                else
                {
                    AddLocalizedModuleMessage(strMessage, message, (!String.IsNullOrEmpty(strMessage)));
                }
            }
            else
            {
                if (notify)
                {
					//Send Notification to User
                    if (PortalSettings.UserRegistration == (int) Globals.PortalRegistrationType.VerifiedRegistration)
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationVerified, PortalSettings);
                    }
                    else
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPublic, PortalSettings);
                    }
                }
            }
           
            return strMessage;
        }

        #region Private methods

        private void SendAdminNotification(UserInfo newUser, PortalSettings portalSettings)
        {
            var notificationType = newUser.Membership.Approved ? "NewUserRegistration" : "NewUnauthorizedUserRegistration";
            var locale = LocaleController.Instance.GetDefaultLocale(portalSettings.PortalId).Code;
            var notification = new Notification
            {
                NotificationTypeID = NotificationsController.Instance.GetNotificationType(notificationType).NotificationTypeId,
                IncludeDismissAction = newUser.Membership.Approved,
                SenderUserID = portalSettings.AdministratorId,
                Subject = GetNotificationSubject(locale, newUser, portalSettings),
                Body = GetNotificationBody(locale, newUser, portalSettings),
                Context = newUser.UserID.ToString(CultureInfo.InvariantCulture)
            };
            var adminrole = RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId);
            var roles = new List<RoleInfo> { adminrole };
            NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, roles, new List<UserInfo>());
        }

        private string GetNotificationBody(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY";
            return LocalizeNotificationText(text, locale, newUser, portalSettings);
        }

        private string LocalizeNotificationText(string text, string locale, UserInfo user, PortalSettings portalSettings)
        {
            //This method could need a custom ArrayList in future notification types. Currently it is null
            return Localization.GetSystemMessage(locale, portalSettings, text, user, Localization.GlobalResourceFile, null, "", portalSettings.AdministratorId);            
        }

        private string GetNotificationSubject(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT";
            return LocalizeNotificationText(text, locale, newUser, portalSettings);
        }

        #endregion
    }
}
