// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
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

    public enum DisplayMode
    {
        All = 0,
        FirstLetter = 1,
        None = 2,
    }

    public enum UsersControl
    {
        Combo = 0,
        TextBox = 1,
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
        /// Gets or sets and sets the User associated with this control.
        /// </summary>
        public UserInfo User
        {
            get
            {
                return this._User ?? (this._User = this.AddUser ? this.InitialiseUser() : UserController.GetUserById(this.UserPortalID, this.UserId));
            }

            set
            {
                this._User = value;
                if (this._User != null)
                {
                    this.UserId = this._User.UserID;
                }
            }
        }

        /// <summary>
        /// Gets or sets and sets the UserId associated with this control.
        /// </summary>
        public new int UserId
        {
            get
            {
                int _UserId = Null.NullInteger;
                if (this.ViewState["UserId"] == null)
                {
                    if (this.Request.QueryString["userid"] != null)
                    {
                        int userId;

                        // Use Int32.MaxValue as invalid UserId
                        _UserId = int.TryParse(this.Request.QueryString["userid"], out userId) ? userId : int.MaxValue;
                        this.ViewState["UserId"] = _UserId;
                    }
                }
                else
                {
                    _UserId = Convert.ToInt32(this.ViewState["UserId"]);
                }

                return _UserId;
            }

            set
            {
                this.ViewState["UserId"] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether we are in Add User mode.
        /// </summary>
        protected virtual bool AddUser
        {
            get
            {
                return this.UserId == Null.NullInteger;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the current user is an Administrator (or SuperUser).
        /// </summary>
        protected bool IsAdmin
        {
            get
            {
                return this.Request.IsAuthenticated && PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether this is the current user or admin.
        /// </summary>
        /// <value>
        /// <placeholder>gets whether this is the current user or admin</placeholder>
        /// </value>
        /// <returns></returns>
        /// <remarks></remarks>
        protected bool IsUserOrAdmin
        {
            get
            {
                return this.IsUser || this.IsAdmin;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether this control is in the Host menu.
        /// </summary>
        protected bool IsHostTab
        {
            get
            {
                return this.IsHostMenu;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the control is being called form the User Accounts module.
        /// </summary>
        protected bool IsEdit
        {
            get
            {
                bool _IsEdit = false;
                if (this.Request.QueryString["ctl"] != null)
                {
                    string ctl = this.Request.QueryString["ctl"];
                    if (ctl.Equals("edit", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _IsEdit = true;
                    }
                }

                return _IsEdit;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the current user is modifying their profile.
        /// </summary>
        protected bool IsProfile
        {
            get
            {
                bool _IsProfile = false;
                if (this.IsUser)
                {
                    if (this.PortalSettings.UserTabId != -1)
                    {
                        // user defined tab
                        if (this.PortalSettings.ActiveTab.TabID == this.PortalSettings.UserTabId)
                        {
                            _IsProfile = true;
                        }
                    }
                    else
                    {
                        // admin tab
                        if (this.Request.QueryString["ctl"] != null)
                        {
                            string ctl = this.Request.QueryString["ctl"];
                            if (ctl.Equals("profile", StringComparison.InvariantCultureIgnoreCase))
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
        /// Gets a value indicating whether gets whether an anonymous user is trying to register.
        /// </summary>
        protected bool IsRegister
        {
            get
            {
                return !this.IsAdmin && !this.IsUser;
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the User is editing their own information.
        /// </summary>
        protected bool IsUser
        {
            get
            {
                return this.Request.IsAuthenticated && (this.UserId == this.UserInfo.UserID);
            }
        }

        /// <summary>
        /// Gets the PortalId to use for this control.
        /// </summary>
        protected int UserPortalID
        {
            get
            {
                return this.IsHostTab ? Null.NullInteger : this.PortalId;
            }
        }

        /// <summary>
        /// Gets a Setting for the Module.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
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
                HostController.Instance.Update(new ConfigurationSetting { Value = setting, Key = key });
            }
            else
            {
                PortalController.UpdatePortalSetting(portalId, key, setting);
            }
        }

        /// <summary>
        /// Updates the Settings for the Module.
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
        /// AddLocalizedModuleMessage adds a localized module message.
        /// </summary>
        /// <param name="message">The localized message.</param>
        /// <param name="type">The type of message.</param>
        /// <param name="display">A flag that determines whether the message should be displayed.</param>
        protected void AddLocalizedModuleMessage(string message, ModuleMessage.ModuleMessageType type, bool display)
        {
            if (display)
            {
                UI.Skins.Skin.AddModuleMessage(this, message, type);
            }
        }

        /// <summary>
        /// AddModuleMessage adds a module message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type of message.</param>
        /// <param name="display">A flag that determines whether the message should be displayed.</param>
        protected void AddModuleMessage(string message, ModuleMessage.ModuleMessageType type, bool display)
        {
            this.AddLocalizedModuleMessage(Localization.GetString(message, this.LocalResourceFile), type, display);
        }

        protected string CompleteUserCreation(UserCreateStatus createStatus, UserInfo newUser, bool notify, bool register)
        {
            var strMessage = string.Empty;
            var message = ModuleMessage.ModuleMessageType.RedError;
            if (register)
            {
                // send notification to portal administrator of new user registration
                // check the receive notification setting first, but if register type is Private, we will always send the notification email.
                // because the user need administrators to do the approve action so that he can continue use the website.
                if (this.PortalSettings.EnableRegisterNotification || this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.PrivateRegistration)
                {
                    strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationAdmin, this.PortalSettings);
                    this.SendAdminNotification(newUser, this.PortalSettings);
                }

                var loginStatus = UserLoginStatus.LOGIN_FAILURE;

                // complete registration
                switch (this.PortalSettings.UserRegistration)
                {
                    case (int)Globals.PortalRegistrationType.PrivateRegistration:
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPrivate, this.PortalSettings);

                        // show a message that a portal administrator has to verify the user credentials
                        if (string.IsNullOrEmpty(strMessage))
                        {
                            strMessage += Localization.GetString("PrivateConfirmationMessage", Localization.SharedResourceFile);
                            message = ModuleMessage.ModuleMessageType.GreenSuccess;
                        }

                        break;
                    case (int)Globals.PortalRegistrationType.PublicRegistration:
                        Mail.SendMail(newUser, MessageType.UserRegistrationPublic, this.PortalSettings);
                        UserController.UserLogin(this.PortalSettings.PortalId, newUser.Username, newUser.Membership.Password, string.Empty, this.PortalSettings.PortalName, string.Empty, ref loginStatus, false);
                        break;
                    case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                        Mail.SendMail(newUser, MessageType.UserRegistrationVerified, this.PortalSettings);
                        UserController.UserLogin(this.PortalSettings.PortalId, newUser.Username, newUser.Membership.Password, string.Empty, this.PortalSettings.PortalName, string.Empty, ref loginStatus, false);
                        break;
                }

                // store preferredlocale in cookie
                Localization.SetLanguage(newUser.Profile.PreferredLocale);
                if (this.IsRegister && message == ModuleMessage.ModuleMessageType.RedError)
                {
                    this.AddLocalizedModuleMessage(string.Format(Localization.GetString("SendMail.Error", Localization.SharedResourceFile), strMessage), message, !string.IsNullOrEmpty(strMessage));
                }
                else
                {
                    this.AddLocalizedModuleMessage(strMessage, message, !string.IsNullOrEmpty(strMessage));
                }
            }
            else
            {
                if (notify)
                {
                    // Send Notification to User
                    if (this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationVerified, this.PortalSettings);
                    }
                    else
                    {
                        strMessage += Mail.SendMail(newUser, MessageType.UserRegistrationPublic, this.PortalSettings);
                    }
                }
            }

            return strMessage;
        }

        /// <summary>
        /// InitialiseUser initialises a "new" user.
        /// </summary>
        private UserInfo InitialiseUser()
        {
            var newUser = new UserInfo();
            if (this.IsHostMenu && !this.IsRegister)
            {
                newUser.IsSuperUser = true;
            }
            else
            {
                newUser.PortalID = this.PortalId;
            }

            // Initialise the ProfileProperties Collection
            string lc = new Localization().CurrentUICulture;

            newUser.Profile.InitialiseProfile(this.PortalId);
            newUser.Profile.PreferredTimeZone = this.PortalSettings.TimeZone;

            newUser.Profile.PreferredLocale = lc;

            // Set default countr
            string country = Null.NullString;
            country = this.LookupCountry();
            if (!string.IsNullOrEmpty(country))
            {
                ListController listController = new ListController();
                var listitem = listController.GetListEntryInfo("Country", country);
                if (listitem != null)
                {
                    country = listitem.EntryID.ToString();
                }

                newUser.Profile.Country = country;
            }

            // Set AffiliateId
            int AffiliateId = Null.NullInteger;
            if (this.Request.Cookies["AffiliateId"] != null)
            {
                AffiliateId = int.Parse(this.Request.Cookies["AffiliateId"].Value);
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
            if (this.Page.Request.UserHostAddress == "127.0.0.1")
            {
                // 'The country cannot be detected because the user is local.
                IsLocal = true;

                // Set the IP address in case they didn't specify LocalhostCountryCode
                IP = this.Page.Request.UserHostAddress;
            }
            else
            {
                // Set the IP address so we can find the country
                IP = this.Page.Request.UserHostAddress;
            }

            // Check to see if we need to generate the Cache for the GeoIPData file
            if (this.Context.Cache.Get("GeoIPData") == null && _CacheGeoIPData)
            {
                // Store it as  well as setting a dependency on the file
                this.Context.Cache.Insert("GeoIPData", CountryLookup.FileToMemory(this.Context.Server.MapPath(_GeoIPFile)), new CacheDependency(this.Context.Server.MapPath(_GeoIPFile)));
            }

            // Check to see if the request is a localhost request
            // and see if the LocalhostCountryCode is specified
            if (IsLocal)
            {
                return Null.NullString;
            }

            // Either this is a remote request or it is a local
            // request with no LocalhostCountryCode specified
            CountryLookup _CountryLookup;

            // Check to see if we are using the Cached
            // version of the GeoIPData file
            if (_CacheGeoIPData)
            {
                // Yes, get it from cache
                _CountryLookup = new CountryLookup((MemoryStream)this.Context.Cache.Get("GeoIPData"));
            }
            else
            {
                // No, get it from file
                _CountryLookup = new CountryLookup(this.Context.Server.MapPath(_GeoIPFile));
            }

            // Get the country code based on the IP address
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

        private void SendAdminNotification(UserInfo newUser, PortalSettings portalSettings)
        {
            var notificationType = newUser.Membership.Approved ? "NewUserRegistration" : "NewUnauthorizedUserRegistration";
            var locale = LocaleController.Instance.GetDefaultLocale(portalSettings.PortalId).Code;
            var notification = new Notification
            {
                NotificationTypeID = NotificationsController.Instance.GetNotificationType(notificationType).NotificationTypeId,
                IncludeDismissAction = newUser.Membership.Approved,
                SenderUserID = portalSettings.AdministratorId,
                Subject = this.GetNotificationSubject(locale, newUser, portalSettings),
                Body = this.GetNotificationBody(locale, newUser, portalSettings),
                Context = newUser.UserID.ToString(CultureInfo.InvariantCulture),
            };
            var adminrole = RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId);
            var roles = new List<RoleInfo> { adminrole };
            NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, roles, new List<UserInfo>());
        }

        private string GetNotificationBody(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_BODY";
            return this.LocalizeNotificationText(text, locale, newUser, portalSettings);
        }

        private string LocalizeNotificationText(string text, string locale, UserInfo user, PortalSettings portalSettings)
        {
            // This method could need a custom ArrayList in future notification types. Currently it is null
            return Localization.GetSystemMessage(locale, portalSettings, text, user, Localization.GlobalResourceFile, null, string.Empty, portalSettings.AdministratorId);
        }

        private string GetNotificationSubject(string locale, UserInfo newUser, PortalSettings portalSettings)
        {
            const string text = "EMAIL_USER_REGISTRATION_ADMINISTRATOR_SUBJECT";
            return this.LocalizeNotificationText(text, locale, newUser, portalSettings);
        }
    }
}
