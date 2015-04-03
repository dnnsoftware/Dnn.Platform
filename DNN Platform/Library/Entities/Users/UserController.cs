#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Messaging.Data;
using MembershipProvider = DotNetNuke.Security.Membership.MembershipProvider;

namespace DotNetNuke.Entities.Users
{
    /// <summary>
    /// The UserController class provides Business Layer methods for Users
    /// </summary>
    /// <remarks>
    /// DotNetNuke user management is base on asp.net membership provider, but  the default implementation of these providers 
    /// do not satisfy the broad set of use cases which we need to support in DotNetNuke. so The dependency of DotNetNuke on the 
    /// MemberRole (ASP.NET 2 Membership) components will be abstracted into a DotNetNuke Membership Provider, in order to allow 
    /// developers complete flexibility in implementing alternate Membership approaches.
    /// <list type="bullet">
    /// <item>This will allow for a number of enhancements to be added</item>
    /// <item>Removal of dependence on the HttpContext</item>
    /// <item>Support for Hashed Passwords</item>
    /// <item>Support for Password Question and Answer</item>
    /// <item>Enforce Password Complexity</item>
    /// <item>Password Aging (Expiry)</item>
    /// <item>Force Password Update</item>
    /// <item>Enable/Disable Password Retrieval/Reset</item>
    /// <item>CAPTCHA Support</item>
    /// <item>Redirect after registration/login/logout</item>
    /// </list>
    /// </remarks>
    /// <seealso cref="DotNetNuke.Security.Membership.MembershipProvider"/>
    /// -----------------------------------------------------------------------------
    public partial class UserController : ServiceLocator<IUserController, UserController>, IUserController
    {
        private const string DefaultUsersFoldersPath = "Users";

        protected override Func<IUserController> GetFactory()
        {
            return () => new UserController();
        }

        #region Public Properties

        public string DisplayFormat { get; set; }

        public int PortalId { get; set; }

        #endregion

        private static event EventHandler<UserEventArgs> UserAuthenticated;

        private static event EventHandler<UserEventArgs> UserCreated;

        private static event EventHandler<UserEventArgs> UserDeleted;

        private static event EventHandler<UserEventArgs> UserRemoved;

        private static event EventHandler<UserEventArgs> UserApproved;

        static UserController()
        {            
            foreach (var handlers in EventHandlersContainer<IUserEventHandlers>.Instance.EventHandlers)
            {
                UserAuthenticated += handlers.Value.UserAuthenticated;
                UserCreated += handlers.Value.UserCreated;
                UserDeleted += handlers.Value.UserDeleted;
                UserRemoved += handlers.Value.UserRemoved;
                UserApproved += handlers.Value.UserApproved;
            }
        }

        #region Private Methods

        private static void AddEventLog(int portalId, string username, int userId, string portalName, string ip, UserLoginStatus loginStatus)
        {
            //initialize log record
            var objSecurity = new PortalSecurity();
            var log = new LogInfo
            {
                LogTypeKey = loginStatus.ToString(),
                LogPortalID = portalId,
                LogPortalName = portalName,
                LogUserName = objSecurity.InputFilter(username, PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup),
                LogUserID = userId
            };
            log.AddProperty("IP", ip);

            //create log record
            LogController.Instance.AddLog(log);
        }

        private static void AutoAssignUsersToPortalRoles(UserInfo user, int portalId)
        {
            foreach (var role in RoleController.Instance.GetRoles(portalId, role => role.AutoAssignment && role.Status == RoleStatus.Approved))
            {
                RoleController.Instance.AddUserRole(portalId, user.UserID, role.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
            }

            //Clear the roles cache - so the usercount is correct
            RoleController.Instance.ClearRoleCache(portalId);
        }

        private static void AutoAssignUsersToRoles(UserInfo user, int portalId)
        {
            var thisPortal = PortalController.Instance.GetPortal(portalId);

            if (IsMemberOfPortalGroup(portalId))
            {
                foreach (var portal in PortalGroupController.Instance.GetPortalsByGroup(thisPortal.PortalGroupID))
                {
                    if (!user.Membership.Approved && portal.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                    {
                        var role = RoleController.Instance.GetRole(portal.PortalID, r => r.RoleName == "Unverified Users");
                        RoleController.Instance.AddUserRole(portal.PortalID, user.UserID, role.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                    }
                    else
                    {
                        AutoAssignUsersToPortalRoles(user, portal.PortalID);
                    }
                }
            }
            else
            {
                if (!user.Membership.Approved && thisPortal.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
                {
                    var role = RoleController.Instance.GetRole(portalId, r => r.RoleName == "Unverified Users");
                    RoleController.Instance.AddUserRole(portalId, user.UserID, role.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                }
                else
                {
                    AutoAssignUsersToPortalRoles(user, portalId);
                }
            }
        }

        //TODO - Handle Portal Groups
        private static void DeleteUserPermissions(UserInfo user)
        {
            FolderPermissionController.DeleteFolderPermissionsByUser(user);

            //Delete Module Permissions
            ModulePermissionController.DeleteModulePermissionsByUser(user);

            //Delete Tab Permissions
            TabPermissionController.DeleteTabPermissionsByUser(user);
        }

        private static void RestoreUserPermissions(UserInfo user)
        {
            //restore user's folder permission
            var userFolderPath = ((PathUtils)PathUtils.Instance).GetUserFolderPathInternal(user);
            var portalId = user.IsSuperUser ? Null.NullInteger : user.PortalID;
            var userFolder = FolderManager.Instance.GetFolder(portalId, userFolderPath);

            if (userFolder != null)
            {
                foreach (PermissionInfo permission in PermissionController.GetPermissionsByFolder())
                {
                    if (permission.PermissionKey.ToUpper() == "READ" 
                            || permission.PermissionKey.ToUpper() == "WRITE" 
                            || permission.PermissionKey.ToUpper() == "BROWSE")
                    {
                        var folderPermission = new FolderPermissionInfo(permission)
                                                   {
                                                       FolderID = userFolder.FolderID,
                                                       UserID = user.UserID,
                                                       RoleID = Int32.Parse(Globals.glbRoleNothing),
                                                       AllowAccess = true
                                                   };

                        userFolder.FolderPermissions.Add(folderPermission, true);
                    }
                }

                FolderPermissionController.SaveFolderPermissions((FolderInfo) userFolder);
            }
        }

        private static void FixMemberPortalId(UserInfo user, int portalId)
        {
            if (user != null)
            {
                user.PortalID = portalId;
            }
        }

        private static object GetCachedUserByPortalCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var username = (string)cacheItemArgs.ParamList[1];
            return MembershipProvider.Instance().GetUserByUserName(portalId, username);
        }

        private static UserInfo GetCurrentUserInternal()
        {
            UserInfo user;
            if ((HttpContext.Current == null))
            {
                if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                {
                    return new UserInfo();
                }
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (portalSettings != null)
                {
                    user = GetCachedUser(portalSettings.PortalId, Thread.CurrentPrincipal.Identity.Name);
                    return user ?? new UserInfo();
                }
                return new UserInfo();
            }
            user = (UserInfo)HttpContext.Current.Items["UserInfo"];
            return user ?? new UserInfo();            
        }

        private static int GetEffectivePortalId(int portalId)
        {
            return PortalController.GetEffectivePortalId(portalId);
        }

        private static object GetUserCountByPortalCallBack(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var portalUserCount = MembershipProvider.Instance().GetUserCountByPortal(portalId);
            DataCache.SetCache(cacheItemArgs.CacheKey, portalUserCount);
            return portalUserCount;
        }

        private static Dictionary<int, string> GetUserLookupDictionary(int portalId)
        {
            var masterPortalId = GetEffectivePortalId(portalId);
            var cacheKey = string.Format(DataCache.UserLookupCacheKey, masterPortalId);
            return CBO.GetCachedObject<Dictionary<int, string>>(new CacheItemArgs(cacheKey, DataCache.UserLookupCacheTimeOut, 
                                                            DataCache.UserLookupCachePriority), (c) => new Dictionary<int, string>(),true);
        }

        internal static Hashtable GetUserSettings(int portalId, Hashtable settings)
        {
            portalId = GetEffectivePortalId(portalId);

            if (settings["Column_FirstName"] == null)
            {
                settings["Column_FirstName"] = false;
            }
            if (settings["Column_LastName"] == null)
            {
                settings["Column_LastName"] = false;
            }
            if (settings["Column_DisplayName"] == null)
            {
                settings["Column_DisplayName"] = true;
            }
            if (settings["Column_Address"] == null)
            {
                settings["Column_Address"] = true;
            }
            if (settings["Column_Telephone"] == null)
            {
                settings["Column_Telephone"] = true;
            }
            if (settings["Column_Email"] == null)
            {
                settings["Column_Email"] = false;
            }
            if (settings["Column_CreatedDate"] == null)
            {
                settings["Column_CreatedDate"] = true;
            }
            if (settings["Column_LastLogin"] == null)
            {
                settings["Column_LastLogin"] = false;
            }
            if (settings["Column_Authorized"] == null)
            {
                settings["Column_Authorized"] = true;
            }
            if (settings["Display_Mode"] == null)
            {
                settings["Display_Mode"] = DisplayMode.All;
            }
            else
            {
                settings["Display_Mode"] = (DisplayMode)Convert.ToInt32(settings["Display_Mode"]);
            }
            if (settings["Display_SuppressPager"] == null)
            {
                settings["Display_SuppressPager"] = false;
            }
            if (settings["Records_PerPage"] == null)
            {
                settings["Records_PerPage"] = 10;
            }
            if (settings["Profile_DefaultVisibility"] == null)
            {
                settings["Profile_DefaultVisibility"] = UserVisibilityMode.AdminOnly;
            }
            else
            {
                settings["Profile_DefaultVisibility"] = (UserVisibilityMode)Convert.ToInt32(settings["Profile_DefaultVisibility"]);
            }
            if (settings["Profile_DisplayVisibility"] == null)
            {
                settings["Profile_DisplayVisibility"] = true;
            }
            if (settings["Profile_ManageServices"] == null)
            {
                settings["Profile_ManageServices"] = true;
            }
            if (settings["Redirect_AfterLogin"] == null)
            {
                settings["Redirect_AfterLogin"] = -1;
            }
            if (settings["Redirect_AfterRegistration"] == null)
            {
                settings["Redirect_AfterRegistration"] = -1;
            }
            if (settings["Redirect_AfterLogout"] == null)
            {
                settings["Redirect_AfterLogout"] = -1;
            }
            if (settings["Security_CaptchaLogin"] == null)
            {
                settings["Security_CaptchaLogin"] = false;
            }
            if (settings["Security_CaptchaRegister"] == null)
            {
                settings["Security_CaptchaRegister"] = false;
            }
            if (settings["Security_CaptchaChangePassword"] == null)
            {
                settings["Security_CaptchaChangePassword"] = false;
            }
            if (settings["Security_CaptchaRetrivePassword"] == null)
            {
                settings["Security_CaptchaRetrivePassword"] = false;
            }
            if (settings["Security_EmailValidation"] == null)
            {
                settings["Security_EmailValidation"] = Globals.glbEmailRegEx;
            }
            if (settings["Security_UserNameValidation"] == null)
            {
                settings["Security_UserNameValidation"] = Globals.glbUserNameRegEx;
            }
            //Forces a valid profile on registration
            if (settings["Security_RequireValidProfile"] == null)
            {
                settings["Security_RequireValidProfile"] = false;
            }
            //Forces a valid profile on login
            if (settings["Security_RequireValidProfileAtLogin"] == null)
            {
                settings["Security_RequireValidProfileAtLogin"] = true;
            }
            if (settings["Security_UsersControl"] == null)
            {
                var portal = PortalController.Instance.GetPortal(portalId);

                if (portal != null && portal.Users > 1000)
                {
                    settings["Security_UsersControl"] = UsersControl.TextBox;
                }
                else
                {
                    settings["Security_UsersControl"] = UsersControl.Combo;
                }
            }
            else
            {
                settings["Security_UsersControl"] = (UsersControl)Convert.ToInt32(settings["Security_UsersControl"]);
            }
            //Display name format
            if (settings["Security_DisplayNameFormat"] == null)
            {
                settings["Security_DisplayNameFormat"] = "";
            }
            if (settings["Registration_RequireConfirmPassword"] == null)
            {
                settings["Registration_RequireConfirmPassword"] = true;
            }
            if (settings["Registration_RandomPassword"] == null)
            {
                settings["Registration_RandomPassword"] = false;
            }
            if (settings["Registration_UseEmailAsUserName"] == null)
            {
                settings["Registration_UseEmailAsUserName"] = false;
            }
            if (settings["Registration_UseAuthProviders"] == null)
            {
                settings["Registration_UseAuthProviders"] = false;
            }
            if (settings["Registration_UseProfanityFilter"] == null)
            {
                settings["Registration_UseProfanityFilter"] = false;
            }
            if (settings["Registration_RegistrationFormType"] == null)
            {
                settings["Registration_RegistrationFormType"] = 0;
            }
            if (settings["Registration_RegistrationFields"] == null)
            {
                settings["Registration_RegistrationFields"] = String.Empty;
            } 
            if (settings["Registration_ExcludeTerms"] == null)
            {
                settings["Registration_ExcludeTerms"] = String.Empty;
            }
            if (settings["Registration_RequireUniqueDisplayName"] == null)
            {
                settings["Registration_RequireUniqueDisplayName"] = false;
            } 
            return settings;
        }

        private static bool IsMemberOfPortalGroup(int portalId)
        {
            return PortalController.IsMemberOfPortalGroup(portalId);
        }

        private static void MergeUserProfileProperties(UserInfo userMergeFrom, UserInfo userMergeTo)
        {
            foreach (ProfilePropertyDefinition property in userMergeFrom.Profile.ProfileProperties)
            {
                if (string.IsNullOrEmpty(userMergeTo.Profile.GetPropertyValue(property.PropertyName)))
                {
                    userMergeTo.Profile.SetProfileProperty(property.PropertyName, property.PropertyValue);
                }
            }
        }

        private static void MergeUserProperties(UserInfo userMergeFrom, UserInfo userMergeTo)
        {
            if (string.IsNullOrEmpty(userMergeTo.DisplayName))
            {
                userMergeTo.DisplayName = userMergeFrom.DisplayName;
            }

            if (string.IsNullOrEmpty(userMergeTo.Email))
            {
                userMergeTo.Email = userMergeFrom.Email;
            }

            if (string.IsNullOrEmpty(userMergeTo.FirstName))
            {
                userMergeTo.FirstName = userMergeFrom.FirstName;
            }

            if (string.IsNullOrEmpty(userMergeTo.LastName))
            {
                userMergeTo.LastName = userMergeFrom.LastName;
            }
        }

        private static void SendDeleteEmailNotifications(UserInfo user, PortalSettings portalSettings)
        {
            var message = new Message();
            message.FromUserID = portalSettings.AdministratorId;
            message.ToUserID = portalSettings.AdministratorId;
            message.Subject = Localization.GetSystemMessage(user.Profile.PreferredLocale,
                                                            portalSettings,
                                                            "EMAIL_USER_UNREGISTER_SUBJECT",
                                                            user,
                                                            Localization.GlobalResourceFile,
                                                            null,
                                                            "",
                                                            portalSettings.AdministratorId);
            message.Body = Localization.GetSystemMessage(user.Profile.PreferredLocale,
                                                         portalSettings,
                                                         "EMAIL_USER_UNREGISTER_BODY",
                                                         user,
                                                         Localization.GlobalResourceFile,
                                                         null,
                                                         "",
                                                         portalSettings.AdministratorId);
            message.Status = MessageStatusType.Unread;
            Mail.SendEmail(portalSettings.Email, portalSettings.Email, message.Subject, message.Body);
        }

        #endregion

        #region Public Mehods

        UserInfo IUserController.GetCurrentUserInfo()
        {
            return GetCurrentUserInternal();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUser retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userId">The Id of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public UserInfo GetUser(int portalId, int userId)
        {
            return GetUserById(portalId, userId);
        }

        public UserInfo GetUserByDisplayname(int portalId, string displayName)
        {
            return MembershipProvider.Instance().GetUserByDisplayName(PortalController.GetEffectivePortalId(portalId), displayName);
        }

        UserInfo IUserController.GetUserById(int portalId, int userId)
        {
            return GetUserById(portalId, userId);
        }

        public IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationTypeId,
            bool isAdmin, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyNames,
            string propertyValues)
        {
            return MembershipProvider.Instance().GetUsersAdvancedSearch(PortalController.GetEffectivePortalId(portalId), userId, filterUserId, filterRoleId, relationTypeId,
                                                      isAdmin, pageIndex, pageSize, sortColumn,
                                                      sortAscending, propertyNames, propertyValues);
        }

        public IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn, bool sortAscending,
            string propertyName, string propertyValue)
        {
            return MembershipProvider.Instance().GetUsersBasicSearch(PortalController.GetEffectivePortalId(portalId), pageIndex, pageSize, sortColumn,
                                                       sortAscending, propertyName, propertyValue);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Update all the Users Display Names
        /// </summary>
        /// -----------------------------------------------------------------------------
        public void UpdateDisplayNames()
        {
            int portalId = GetEffectivePortalId(PortalId);

            var arrUsers = GetUsers(PortalId);
            foreach (UserInfo objUser in arrUsers)
            {
                objUser.UpdateDisplayName(DisplayFormat);
                UpdateUser(portalId, objUser);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the number count for all duplicate e-mail adresses in the database
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static int GetDuplicateEmailCount()
        {
            return DataProvider.Instance().GetDuplicateEmailCount(PortalSettings.Current.PortalId);
        }

        #endregion

        #region Public Helper Methods

        /// <summary>
        /// add new userportal record (used for creating sites with existing user)
        /// </summary>
        /// <param name="portalId">portalid</param>
        /// <param name="userId">userid</param>
        public static void AddUserPortal(int portalId, int userId)
        {
            Requires.NotNullOrEmpty("portalId", portalId.ToString());
            Requires.NotNullOrEmpty("userId", userId.ToString());

            MembershipProvider.Instance().AddUserPortal(portalId,userId);
        }

        /// <summary>
        /// ApproveUser removes the Unverified Users role from the user and adds the auto assigned roles.
        /// </summary>
        /// <param name="user">The user to update.</param>
        public static void ApproveUser(UserInfo user)
        {
            Requires.NotNull("user", user);

            var settings = PortalController.Instance.GetCurrentPortalSettings();
            var role = RoleController.Instance.GetRole(settings.PortalId, r => r.RoleName == "Unverified Users");

            RoleController.DeleteUserRole(user, role, settings, false);

            AutoAssignUsersToRoles(user, settings.PortalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ChangePassword attempts to change the users password
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to update.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public static bool ChangePassword(UserInfo user, string oldPassword, string newPassword)
        {
            bool retValue;

            //Although we would hope that the caller has already validated the password,
            //Validate the new Password
            if (ValidatePassword(newPassword))
            {
                retValue = MembershipProvider.Instance().ChangePassword(user, oldPassword, newPassword);

                //Update User
                user.Membership.UpdatePassword = false;
                UpdateUser(user.PortalID, user);
            }
            else
            {
                throw new Exception("Invalid Password");
            }
            return retValue;
        }

        /// <summary>
        /// overload will validate the token and if valid change the password
        /// it does not require an old password as it supports hashed passwords
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// /// <param name="resetToken">The reset token, typically supplied through a password reset email.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        public static bool ChangePasswordByToken(int portalid, string username, string newPassword, string resetToken)
        {
            bool retValue;

            Guid resetTokenGuid = new Guid(resetToken);

            var user=GetUserByName(portalid, username);
            //if user does not exist return false 
            if (user==null)
            {
                return false;
            }
            //check if the token supplied is the same as the users and is still valid
            if (user.PasswordResetToken != resetTokenGuid || user.PasswordResetExpiration < DateTime.Now)
            {
                return false;
            }
            var m = new MembershipPasswordController();
            if (m.IsPasswordInHistory(user.UserID, user.PortalID, newPassword))
            {
                return false;
            }
            
            //Although we would hope that the caller has already validated the password,
            //Validate the new Password
            if (ValidatePassword(newPassword))
            {
                retValue = MembershipProvider.Instance().ResetAndChangePassword(user, newPassword);

                //update reset token values to ensure token is 1-time use
                user.PasswordResetExpiration = DateTime.MinValue;
                user.PasswordResetToken = Guid.NewGuid();

                //Update User
                user.Membership.UpdatePassword = false;
                UpdateUser(user.PortalID, user);
            }
            else
            {
                throw new Exception("Invalid Password");
            }
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ChangePasswordQuestionAndAnswer attempts to change the users password Question
        /// and PasswordAnswer
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to update.</param>
        /// <param name="password">The password.</param>
        /// <param name="passwordQuestion">The new password question.</param>
        /// <param name="passwordAnswer">The new password answer.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public static bool ChangePasswordQuestionAndAnswer(UserInfo user, string password, string passwordQuestion, string passwordAnswer)
        {
            EventLogController.Instance.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), GetCurrentUserInternal().UserID, "", EventLogController.EventLogType.USER_UPDATED);
            return MembershipProvider.Instance().ChangePasswordQuestionAndAnswer(user, password, passwordQuestion, passwordAnswer);
        }

        /// <summary>
        /// update username in the system
        /// works around membershipprovider limitation
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="newUsername">new one</param>
        public static void ChangeUsername(int userId, string newUsername)
        {
            MembershipProvider.Instance().ChangeUsername(userId, newUsername);
        }

        public static void CheckInsecurePassword(string username, string password, ref UserLoginStatus loginStatus)
        {
            if (username == "admin" && (password == "admin" || password == "dnnadmin"))
            {
                loginStatus = UserLoginStatus.LOGIN_INSECUREADMINPASSWORD;
            }
            if (username == "host" && (password == "host" || password == "dnnhost"))
            {
                loginStatus = UserLoginStatus.LOGIN_INSECUREHOSTPASSWORD;
            }
        }

        /// <summary>
        /// Copys a user to a different portal.
        /// </summary>
        /// <param name="user">The user to copy</param>
        /// <param name="destinationPortal">The destination portal</param>
        /// <param name="mergeUser">A flag that indicates whether to merge the original user</param>
        public static void CopyUserToPortal(UserInfo user, PortalInfo destinationPortal, bool mergeUser)
        {
            var targetUser = GetUserById(destinationPortal.PortalID, user.UserID);
            if (targetUser == null)
            {
                AddUserPortal(destinationPortal.PortalID, user.UserID);

                if (!user.IsSuperUser)
                {
                    AutoAssignUsersToRoles(user, destinationPortal.PortalID);
                }

                targetUser = GetUserById(destinationPortal.PortalID, user.UserID);
                MergeUserProperties(user, targetUser);
                MergeUserProfileProperties(user, targetUser);
            }
            else
            {
                targetUser.PortalID = destinationPortal.PortalID;

                if (mergeUser)
                {
                    MergeUserProperties(user, targetUser);
                    MergeUserProfileProperties(user, targetUser);
                }
            }
            
            UpdateUser(targetUser.PortalID, targetUser);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a new User in the Data Store
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The userInfo object to persist to the Database</param>
        /// <returns>The Created status ot the User</returns>
        /// -----------------------------------------------------------------------------
        public static UserCreateStatus CreateUser(ref UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);
            //ensure valid GUID exists (covers case where password is randomly generated - has 24 hr validity as per other Admin user steps
            var passwordExpiry = DateTime.Now.AddMinutes(1440);
            var passwordGuid = Guid.NewGuid();
            user.PasswordResetExpiration = passwordExpiry;
            user.PasswordResetToken = passwordGuid;
            
            //Create the User
            var createStatus = MembershipProvider.Instance().CreateUser(ref user);

            if (createStatus == UserCreateStatus.Success)
            {
                //reapply guid/expiry (cleared when user is created)
                user.PasswordResetExpiration = passwordExpiry;
                user.PasswordResetToken = passwordGuid;
                UpdateUser(user.PortalID, user);
                EventLogController.Instance.AddLog(user, PortalController.Instance.GetCurrentPortalSettings(), GetCurrentUserInternal().UserID, "", EventLogController.EventLogType.USER_CREATED);
                DataCache.ClearPortalCache(portalId, false);
                if (!user.IsSuperUser)
                {
                    //autoassign user to portal roles
                    AutoAssignUsersToRoles(user, portalId);
                }

                if (UserCreated != null)
                {
                    UserCreated(null, new UserEventArgs { User = user });
                }
            }

            //Reset PortalId
            FixMemberPortalId(user, portalId);
            return createStatus;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes all Unauthorized Users for a Portal
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteUnauthorizedUsers(int portalId)
        {
	        var arrUsers = GetUnAuthorizedUsers(portalId);
            for (int i = 0; i < arrUsers.Count; i++)
            {
                var user = arrUsers[i] as UserInfo;
                if (user != null)
                {
                    if (user.Membership.Approved == false || user.Membership.LastLoginDate == Null.NullDate)
                    {
                        DeleteUser(ref user, true, false);
                    }
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes an existing User from the Data Store
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The userInfo object to delete from the Database</param>
        /// <param name="notify">A flag that indicates whether an email notification should be sent</param>
        /// <param name="deleteAdmin">A flag that indicates whether the Portal Administrator should be deleted</param>
        /// <returns>A Boolean value that indicates whether the User was successfully deleted</returns>
        /// -----------------------------------------------------------------------------
        public static bool DeleteUser(ref UserInfo user, bool notify, bool deleteAdmin)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);

            // If the HTTP Current Context is unavailable (e.g. when called from within a SchedulerClient) GetCurrentPortalSettings() returns null and the 
            // PortalSettings are created/loaded for the portal (originally) assigned to the user.
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings() ?? new PortalSettings(portalId);

            var canDelete = deleteAdmin || (user.UserID != portalSettings.AdministratorId);

            if (canDelete)
            {
                //Delete Permissions
                DeleteUserPermissions(user);
                canDelete = MembershipProvider.Instance().DeleteUser(user);
            }

            if (canDelete)
            {
                //Obtain PortalSettings from Current Context or from the users (original) portal if the HTTP Current Context is unavailable.
                EventLogController.Instance.AddLog("Username", user.Username, portalSettings, user.UserID, EventLogController.EventLogType.USER_DELETED);
                if (notify && !user.IsSuperUser)
                {
                    //send email notification to portal administrator that the user was removed from the portal
                    SendDeleteEmailNotifications(user, portalSettings);
                }
                DataCache.ClearPortalCache(portalId, false);
                DataCache.ClearUserCache(portalId, user.Username);

				//also clear current portal's cache if the user is a host user
				if (portalSettings.PortalId != portalId)
				{
					DataCache.ClearPortalCache(portalSettings.PortalId, false);
					DataCache.ClearUserCache(portalSettings.PortalId, user.Username);
				}

                // queue remove user contributions from search index
                var document = new Services.Search.Entities.SearchDocumentToDelete
                {
                    PortalId = portalId,
                    AuthorUserId = user.UserID,
                    SearchTypeId = Services.Search.Internals.SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId
                };

                DataProvider.Instance().AddSearchDeletedItems(document);

                if (UserDeleted != null)
                {
                    UserDeleted(null, new UserEventArgs { User = user });
                }
            }

            FixMemberPortalId(user, portalId);
            
            return canDelete;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes all Users for a Portal
        /// </summary>
        /// <remarks></remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="notify">A flag that indicates whether an email notification should be sent</param>
        /// <param name="deleteAdmin">A flag that indicates whether the Portal Administrator should be deleted</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteUsers(int portalId, bool notify, bool deleteAdmin)
        {
            var arrUsers = GetUsers(portalId);
            for (int i = 0; i < arrUsers.Count; i++)
            {
                var objUser = arrUsers[i] as UserInfo;
                DeleteUser(ref objUser, notify, deleteAdmin);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates a new random password (Length = Minimum Length + 4)
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public static string GeneratePassword()
        {
            return GeneratePassword(MembershipProviderConfig.MinPasswordLength + 4);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates a new random password
        /// </summary>
        /// <param name="length">The length of password to generate.</param>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public static string GeneratePassword(int length)
        {
            return MembershipProvider.Instance().GeneratePassword(length);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetCachedUser retrieves the User from the Cache, or fetches a fresh copy if 
        /// not in cache or if Cache settings not set to HeavyCaching
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="username">The username of the user being retrieved.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetCachedUser(int portalId, string username)
        {
            //Get the User cache key
            var masterPortalId = GetEffectivePortalId(portalId);
            var cacheKey = string.Format(DataCache.UserCacheKey, masterPortalId, username);
            var user = CBO.GetCachedObject<UserInfo>(new CacheItemArgs(cacheKey, DataCache.UserCacheTimeOut, DataCache.UserCachePriority, masterPortalId, username), GetCachedUserByPortalCallBack);
            FixMemberPortalId(user, portalId);

            if (user!= null)
            {
                var lookUp = GetUserLookupDictionary(portalId);
                lookUp[user.UserID] = user.Username;
            }

            return user;
        }

        public static ArrayList GetDeletedUsers(int portalId)
        {
            return MembershipProvider.Instance().GetDeletedUsers(GetEffectivePortalId(portalId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Online Users
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>An ArrayList of UserInfo objects</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetOnlineUsers(int portalId)
        {
            return MembershipProvider.Instance().GetOnlineUsers(GetEffectivePortalId(portalId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Current Password Information for the User 
        /// </summary>
        /// <remarks>This method will only return the password if the memberProvider supports
        /// and is using a password encryption method that supports decryption.</remarks>
        /// <param name="user">The user whose Password information we are retrieving.</param>
        /// <param name="passwordAnswer">The answer to the "user's" password Question.</param>
        /// -----------------------------------------------------------------------------
        public static string GetPassword(ref UserInfo user, string passwordAnswer)
        {
            if (MembershipProviderConfig.PasswordRetrievalEnabled)
            {
                user.Membership.Password = MembershipProvider.Instance().GetPassword(user, passwordAnswer);
            }
            else
            {
                //Throw a configuration exception as password retrieval is not enabled
                throw new ConfigurationErrorsException("Password Retrieval is not enabled");
            }
            return user.Membership.Password;
        }

        public static ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
            return MembershipProvider.Instance().GetUnAuthorizedUsers(GetEffectivePortalId(portalId), includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUnAuthorizedUsers gets all the users of the portal, that are not authorized
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUnAuthorizedUsers(int portalId)
        {
            return GetUnAuthorizedUsers(portalId, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUser retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userId">The Id of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetUserById(int portalId, int userId)
        {
            // stop any sql calls for guest users
            if (userId == Null.NullInteger)
            {
                return null;
            }

            var lookUp = GetUserLookupDictionary(portalId);

            UserInfo user;
            string userName;
            if (lookUp.TryGetValue(userId, out userName))
            {
                user = GetCachedUser(portalId, userName);
            }
            else
            {
                user = MembershipProvider.Instance().GetUser(GetEffectivePortalId(portalId), userId);
                FixMemberPortalId(user, portalId);
                if (user != null)
                {

                    lookUp[userId] = user.Username;

                }
            }
            return user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByUserName retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="username">The username of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetUserByName(string username)
        {
            return MembershipProvider.Instance().GetUserByUserName(-1, username);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByUserName retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="username">The username of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetUserByName(int portalId, string username)
        {
            return GetCachedUser(portalId, username);
        }

        public static UserInfo GetUserByVanityUrl(int portalId, string vanityUrl)
        {
            return MembershipProvider.Instance().GetUserByVanityUrl(portalId, vanityUrl);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserCountByPortal gets the number of users in the portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>The no of users</returns>
        /// -----------------------------------------------------------------------------
        public static int GetUserCountByPortal(int portalId)
        {
            portalId = GetEffectivePortalId(portalId);
            var cacheKey = string.Format(DataCache.PortalUserCountCacheKey, portalId);
            return CBO.GetCachedObject<int>(new CacheItemArgs(cacheKey, DataCache.PortalUserCountCacheTimeOut, DataCache.PortalUserCountCachePriority, portalId), GetUserCountByPortalCallBack);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Retruns a String corresponding to the Registration Status of the User
        /// </summary>
        /// <param name="userRegistrationStatus">The AUserCreateStatus</param>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public static string GetUserCreateStatus(UserCreateStatus userRegistrationStatus)
        {
            switch (userRegistrationStatus)
            {
                case UserCreateStatus.DuplicateEmail:
                    return Localization.GetString("UserEmailExists");
                case UserCreateStatus.InvalidAnswer:
                    return Localization.GetString("InvalidAnswer");
                case UserCreateStatus.InvalidEmail:
                    return Localization.GetString("InvalidEmail");
                case UserCreateStatus.InvalidPassword:
                    string strInvalidPassword = Localization.GetString("InvalidPassword");
                    strInvalidPassword = strInvalidPassword.Replace("[PasswordLength]", MembershipProviderConfig.MinPasswordLength.ToString());
                    strInvalidPassword = strInvalidPassword.Replace("[NoneAlphabet]", MembershipProviderConfig.MinNonAlphanumericCharacters.ToString());
                    return strInvalidPassword;
                case UserCreateStatus.PasswordMismatch:
                    return Localization.GetString("PasswordMismatch");
                case UserCreateStatus.InvalidQuestion:
                    return Localization.GetString("InvalidQuestion");
                case UserCreateStatus.InvalidUserName:
                    return Localization.GetString("InvalidUserName");
                case UserCreateStatus.InvalidDisplayName:
                    return Localization.GetString("InvalidDisplayName");
                case UserCreateStatus.DuplicateDisplayName:
                    return Localization.GetString("DuplicateDisplayName");
                case UserCreateStatus.UserRejected:
                    return Localization.GetString("UserRejected");
                case UserCreateStatus.DuplicateUserName:
                case UserCreateStatus.UserAlreadyRegistered:
                case UserCreateStatus.UsernameAlreadyExists:
                    return Localization.GetString("UserNameExists");
                case UserCreateStatus.BannedPasswordUsed:
                    return Localization.GetString("BannedPasswordUsed");
                case UserCreateStatus.ProviderError:
                case UserCreateStatus.DuplicateProviderUserKey:
                case UserCreateStatus.InvalidProviderUserKey:
                    return Localization.GetString("RegError");
                default:
                    throw new ArgumentException("Unknown UserCreateStatus value encountered", "userRegistrationStatus");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Membership Information for the User
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The user whose Membership information we are retrieving.</param>
        /// -----------------------------------------------------------------------------
        public static void GetUserMembership(UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);
            MembershipProvider.Instance().GetUserMembership(ref user);
            FixMemberPortalId(user, portalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Default Settings for the Module
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static Hashtable GetDefaultUserSettings()
        {
            var portalId = -1;
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            if (portalSettings != null)
            {
                portalId = portalSettings.PortalId;
            }
            return GetUserSettings(portalId, new Hashtable());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserSettings retrieves the UserSettings from the User
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>The Settings Hashtable</returns>
        /// -----------------------------------------------------------------------------
        public static Hashtable GetUserSettings(int portalId)
        {
            var settings = GetDefaultUserSettings();
            Dictionary<string, string> settingsDictionary = (portalId == Null.NullInteger)
                                                            ? HostController.Instance.GetSettingsDictionary()
                                                            : PortalController.Instance.GetPortalSettings(GetEffectivePortalId(portalId));
            if (settingsDictionary != null)
            {
                foreach (KeyValuePair<string, string> kvp in settingsDictionary)
                {
                    int index = kvp.Key.IndexOf("_");
                    if (index > 0)
                    {
                        //Get the prefix
                        string prefix = kvp.Key.Substring(0, index + 1);
                        switch (prefix)
                        {
                            case "Column_":
                            case "Display_":
                            case "Profile_":
                            case "Records_":
                            case "Redirect_":
                            case "Registration_":
                            case "Security_":
                                switch (kvp.Key)
                                {
                                    case "Display_Mode":
                                        settings[kvp.Key] = (DisplayMode)Convert.ToInt32(kvp.Value);
                                        break;
                                    case "Profile_DefaultVisibility":
                                        settings[kvp.Key] = (UserVisibilityMode)Convert.ToInt32(kvp.Value);
                                        break;
                                    case "Security_UsersControl":
                                        settings[kvp.Key] = (UsersControl)Convert.ToInt32(kvp.Value);
                                        break;
                                    default:
                                        //update value or add any new values
                                        settings[kvp.Key] = kvp.Value;
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            return settings;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(int portalId)
        {
            return GetUsers(false, false, portalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(bool includeDeleted, bool superUsersOnly, int portalId)
        {
            var totalrecords = -1;
            return GetUsers(portalId, -1, -1, ref totalrecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal, by page
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsers(portalId, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal, by page
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MembershipProvider.Instance().GetUsers(GetEffectivePortalId(portalId), pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByEmail gets all the users of the portal whose email matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="emailToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByEmail gets one single user matching the email address provided
        /// This will only be useful in portals without duplicate email addresses
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="emailToMatch">The email address to use to find a match.</param>
        /// <returns>A single user object or null if no user found</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo GetUserByEmail(int portalId, string emailToMatch)
        {
            int uid = DataProvider.Instance().GetSingleUserByEmail(portalId, emailToMatch);
            if (uid > -1)
            {
                return GetUserById(portalId, uid);
            }
            return null;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByEmail gets all the users of the portal whose email matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="emailToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MembershipProvider.Instance().GetUsersByEmail(GetEffectivePortalId(portalId), emailToMatch, pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByProfileProperty gets all the users of the portal whose profile matches
        /// the profile property pased as a parameter
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="propertyName">The name of the property being matched.</param>
        /// <param name="propertyValue">The value of the property being matched.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByProfileProperty(portalId, propertyName, propertyValue, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByProfileProperty gets all the users of the portal whose profile matches
        /// the profile property pased as a parameter
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="propertyName">The name of the property being matched.</param>
        /// <param name="propertyValue">The value of the property being matched.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MembershipProvider.Instance().GetUsersByProfileProperty(GetEffectivePortalId(portalId), propertyName, propertyValue, pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByUserName gets all the users of the portal whose username matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userNameToMatch">The username to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByUserName(portalId, userNameToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByUserName gets all the users of the portal whose username matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userNameToMatch">The username to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include Deleted Users.</param>
        /// <param name="superUsersOnly">Only get super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            return MembershipProvider.Instance().GetUsersByUserName(GetEffectivePortalId(portalId), userNameToMatch, pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// GetUsersByDisplayName gets all the users of the portal whose display name matches a provided
		/// filter expression
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <param name="portalId">The Id of the Portal</param>
		/// <param name="nameToMatch">The display name to use to find a match.</param>
		/// <param name="pageIndex">The page of records to return.</param>
		/// <param name="pageSize">The size of the page</param>
		/// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
		/// <param name="includeDeleted">Include Deleted Users.</param>
		/// <param name="superUsersOnly">Only get super users.</param>
		/// <returns>An ArrayList of UserInfo objects.</returns>
		/// -----------------------------------------------------------------------------
		public static ArrayList GetUsersByDisplayName(int portalId, string nameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
		{
			return MembershipProvider.Instance().GetUsersByDisplayName(GetEffectivePortalId(portalId), nameToMatch, pageIndex, pageSize, ref totalRecords, includeDeleted, superUsersOnly);
		}

        /// <summary>
        /// Move a user to a different portal.
        /// </summary>
        /// <param name="user">The user to move</param>
        /// <param name="portal">The destination portal</param>
        /// <param name="mergeUser">A flag that indicates whether to merge the original user</param>
        public static void MoveUserToPortal(UserInfo user, PortalInfo portal, bool mergeUser)
        {
            CopyUserToPortal(user, portal, mergeUser);
            RemoveUser(user);
        }

        public static void RemoveDeletedUsers(int portalId)
        {
	        var arrUsers = GetDeletedUsers(portalId);

            foreach (UserInfo objUser in arrUsers)
            {
                if (objUser.IsDeleted)
                {
                    RemoveUser(objUser);
                }
            }
        }

        public static bool RemoveUser(UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);

            //Remove the User
            var retValue = MembershipProvider.Instance().RemoveUser(user);

            if ((retValue))
            {
                // Obtain PortalSettings from Current Context
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                //Log event
                EventLogController.Instance.AddLog("Username", user.Username, portalSettings, user.UserID, EventLogController.EventLogType.USER_REMOVED);

                //Delete userFolder - DNN-3787
                var userFolderPath = ((PathUtils)PathUtils.Instance).GetUserFolderPathInternal(user);
                var folderPortalId = user.IsSuperUser ? Null.NullInteger : user.PortalID;
                var userFolder = FolderManager.Instance.GetFolder(folderPortalId, userFolderPath);
                if (userFolder != null)
                {
                    FolderManager.Instance.Synchronize(folderPortalId, userFolderPath, true, true);
                    var notDeletedSubfolders = new List<IFolderInfo>();
                    FolderManager.Instance.DeleteFolder(userFolder, notDeletedSubfolders);

                    if (notDeletedSubfolders.Count == 0)
                    {
                        //try to remove the parent folder if there is no other users use this folder.
                        var parentFolder = FolderManager.Instance.GetFolder(userFolder.ParentID);
                        FolderManager.Instance.Synchronize(folderPortalId, parentFolder.FolderPath, true, true);
                        if(parentFolder != null && !FolderManager.Instance.GetFolders(parentFolder).Any())
                        {
                            FolderManager.Instance.DeleteFolder(parentFolder, notDeletedSubfolders);

                            if (notDeletedSubfolders.Count == 0)
                            {
                                //try to remove the root folder if there is no other users use this folder.
                                var rootFolder = FolderManager.Instance.GetFolder(parentFolder.ParentID);
                                FolderManager.Instance.Synchronize(folderPortalId, rootFolder.FolderPath, true, true);
                                if (rootFolder != null && !FolderManager.Instance.GetFolders(rootFolder).Any())
                                {
                                    FolderManager.Instance.DeleteFolder(rootFolder, notDeletedSubfolders);
                                }
                            }
                        }
                    }
                }

                DataCache.ClearPortalCache(portalId, false);
                DataCache.ClearUserCache(portalId, user.Username);

                if (UserRemoved != null)
                {
                    UserRemoved(null, new UserEventArgs { User = user });
                }
            }

            //Reset PortalId
            FixMemberPortalId(user, portalId);

            return retValue;
        }

        /// <summary>
        /// reset and change password
        /// used by admin/host users who do not need to supply an "old" password
        /// </summary>
        /// <param name="user">user being changed</param>
        /// <param name="newPassword">new password</param>
        /// <returns></returns>
        public static bool ResetAndChangePassword(UserInfo user, string newPassword)
        {
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (GetCurrentUserInternal().IsInRole(portalSettings.AdministratorRoleName))
            {
                string resetPassword = ResetPassword(user, String.Empty);
                return ChangePassword(user, resetPassword, newPassword);
            }
            return false;
        }

        public static bool ResetAndChangePassword(UserInfo user, string oldPassword, string newPassword)
        {
            if (System.Web.Security.Membership.ValidateUser(user.Username, oldPassword))
            {
                string resetPassword = ResetPassword(user, String.Empty);
                return ChangePassword(user, resetPassword, newPassword);
            }
            return false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Resets the password for the specified user
        /// </summary>
        /// <remarks>Resets the user's password</remarks>
        /// <param name="user">The user whose Password information we are resetting.</param>
        /// <param name="passwordAnswer">The answer to the "user's" password Question.</param>
        /// -----------------------------------------------------------------------------
        public static string ResetPassword(UserInfo user, string passwordAnswer)
        {
            if (MembershipProviderConfig.PasswordResetEnabled)
            {
                user.Membership.Password = MembershipProvider.Instance().ResetPassword(user, passwordAnswer);
            }
            else
            {
                //Throw a configuration exception as password reset is not enabled
                throw new ConfigurationErrorsException("Password Reset is not enabled");
            }
            return user.Membership.Password;
        }

        public static void ResetPasswordToken(UserInfo user)
        {
            ResetPasswordToken(user, false);
        }

        public static bool ResetPasswordToken(UserInfo user,bool sendEmail)
        {
            var settings = new MembershipPasswordSettings(user.PortalID);

            user.PasswordResetExpiration = DateTime.Now.AddMinutes(settings.ResetLinkValidity);
            user.PasswordResetToken = Guid.NewGuid();
            UpdateUser(user.PortalID, user);
            if (sendEmail)
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                return  Mail.SendMail(user, MessageType.PasswordReminder, portalSettings) == string.Empty;
            }
            return true;
        }

        public static void ResetPasswordToken(UserInfo user, int minutesValid)
        {
            user.PasswordResetExpiration = DateTime.Now.AddMinutes(minutesValid);
            user.PasswordResetToken = Guid.NewGuid();
            UpdateUser(user.PortalID, user);
        }  

        public static bool RestoreUser(ref UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);

            //Restore the User
            var retValue = MembershipProvider.Instance().RestoreUser(user);

            if ((retValue))
            {
                //restore user permissions
                RestoreUserPermissions(user);

                // Obtain PortalSettings from Current Context
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                //Log event
                EventLogController.Instance.AddLog("Username", user.Username, portalSettings, user.UserID, EventLogController.EventLogType.USER_RESTORED);

                DataCache.ClearPortalCache(portalId, false);
                DataCache.ClearUserCache(portalId, user.Username);
            }

            //Reset PortalId
            FixMemberPortalId(user, portalId);

            return retValue;
        }

        public static string SettingsKey(int portalId)
        {
            return "UserSettings|" + portalId;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Unlocks the User's Account
        /// </summary>
        /// <remarks></remarks>
        /// <param name="user">The user whose account is being Unlocked.</param>
        /// -----------------------------------------------------------------------------
        public static bool UnLockUser(UserInfo user)
        {
            int portalId = user.PortalID;
            user.PortalID = GetEffectivePortalId(portalId);

            //Unlock the User
            var retValue = MembershipProvider.Instance().UnLockUser(user);
            DataCache.ClearUserCache(portalId, user.Username);
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a User
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="user">The use to update</param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public static void UpdateUser(int portalId, UserInfo user)
        {
            UpdateUser(portalId, user, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   updates a user
        /// </summary>
        /// <param name = "portalId">the portalid of the user</param>
        /// <param name = "user">the user object</param>
        /// <param name = "loggedAction">whether or not the update calls the eventlog - the eventlogtype must still be enabled for logging to occur</param>
        /// <remarks>
        /// </remarks>
        public static void UpdateUser(int portalId, UserInfo user, bool loggedAction)
        {
            UpdateUser(portalId, user, loggedAction, true);
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		///   updates a user
		/// </summary>
		/// <param name = "portalId">the portalid of the user</param>
		/// <param name = "user">the user object</param>
		/// <param name = "loggedAction">whether or not the update calls the eventlog - the eventlogtype must still be enabled for logging to occur</param>
		/// <param name="clearCache">Whether clear cache after update user.</param>
		/// <remarks>
		/// This method is used internal because it should be use carefully, or it will caught cache doesn't clear correctly.
		/// </remarks>
		internal static void UpdateUser(int portalId, UserInfo user, bool loggedAction, bool clearCache)
		{
		    var originalPortalId = user.PortalID;
			portalId = GetEffectivePortalId(portalId);
			user.PortalID = portalId;

			//Update the User
			MembershipProvider.Instance().UpdateUser(user);
			if (loggedAction)
			{
                //if the httpcontext is null, then get portal settings by portal id.
                PortalSettings portalSettings = null;
                if (HttpContext.Current != null)
                {
                    portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                }
                else if (portalId > Null.NullInteger)
                {
                    portalSettings = new PortalSettings(portalId);
                }

                EventLogController.Instance.AddLog(user, portalSettings, GetCurrentUserInternal().UserID, "", EventLogController.EventLogType.USER_UPDATED);
			}

            //Reset PortalId
            FixMemberPortalId(user, originalPortalId);

			//Remove the UserInfo from the Cache, as it has been modified
			if (clearCache)
			{
				DataCache.ClearUserCache(portalId, user.Username);
			}

            if (user.Membership.Approving)
            {
                user.Membership.ConfirmApproved();
                if (UserApproved != null)
                {
                    UserApproved(null, new UserEventArgs { User = user });
                }                    
            }
		}

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's credentials against the Data Store, and sets the Forms Authentication
        /// Ticket
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="username">The user name of the User attempting to log in</param>
        /// <param name="password">The password of the User attempting to log in</param>
        /// <param name="verificationCode">The verification code of the User attempting to log in</param>
        /// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
        /// <param name="loginStatus">A UserLoginStatus enumeration that indicates the status of the 
        /// Login attempt.  This value is returned by reference.</param>
        /// <param name="createPersistentCookie">A flag that indicates whether the login credentials 
        /// should be persisted.</param>
        /// <returns>The UserInfo object representing a successful login</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo UserLogin(int portalId, string username, string password, string verificationCode, string portalName, string ip, ref UserLoginStatus loginStatus, bool createPersistentCookie)
        {
            portalId = GetEffectivePortalId(portalId);

            loginStatus = UserLoginStatus.LOGIN_FAILURE;

            //Validate the user
            var objUser = ValidateUser(portalId, username, password, verificationCode, portalName, ip, ref loginStatus);
            if (objUser != null)
            {
                //Call UserLogin overload
                UserLogin(portalId, objUser, portalName, ip, createPersistentCookie);
            }
            else
            {
                AddEventLog(portalId, username, Null.NullInteger, portalName, ip, loginStatus);
            }

            //return the User object
            return objUser;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Logs a Validated User in
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="user">The validated User</param>
        /// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
        /// <param name="createPersistentCookie">A flag that indicates whether the login credentials should be persisted.</param>
        /// -----------------------------------------------------------------------------
        public static void UserLogin(int portalId, UserInfo user, string portalName, string ip, bool createPersistentCookie)
        {
            portalId = GetEffectivePortalId(portalId);

            AddEventLog(portalId, user.Username, user.UserID, portalName, ip, user.IsSuperUser ? UserLoginStatus.LOGIN_SUPERUSER : UserLoginStatus.LOGIN_SUCCESS);

            //Update User in Database with Last IP used
            user.LastIPAddress = ip;
            UpdateUser(portalId, user, false);

            //set the forms authentication cookie ( log the user in )
            var security = new PortalSecurity();
            security.SignIn(user, createPersistentCookie);

            if (UserAuthenticated != null)
            {
                UserAuthenticated(null, new UserEventArgs() {User = user});
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a Password
        /// </summary>
        /// <param name="password">The password to Validate</param>
        /// <returns>A boolean</returns>
        /// -----------------------------------------------------------------------------
        public static bool ValidatePassword(string password)
        {
            var isValid = true;

            //Valid Length
            if (password.Length < MembershipProviderConfig.MinPasswordLength)
            {
                isValid = false;
            }

            //Validate NonAlphaChars
            var rx = new Regex("[^0-9a-zA-Z]");
            if (rx.Matches(password).Count < MembershipProviderConfig.MinNonAlphanumericCharacters)
            {
                isValid = false;
            }
            //Validate Regex
            if (!String.IsNullOrEmpty(MembershipProviderConfig.PasswordStrengthRegularExpression) && isValid)
            {
                rx = new Regex(MembershipProviderConfig.PasswordStrengthRegularExpression);
                isValid = rx.IsMatch(password);
            }
            return isValid;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's credentials against the Data Store
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="username">The user name of the User attempting to log in</param>
        /// <param name="password">The password of the User attempting to log in</param>
        /// <param name="verificationCode">The verification code of the User attempting to log in</param>
        /// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
        /// <param name="loginStatus">A UserLoginStatus enumeration that indicates the status of the 
        /// Login attempt.  This value is returned by reference.</param>
        /// <returns>The UserInfo object representing a valid user</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo ValidateUser(int portalId, string username, string password, string verificationCode, string portalName, string ip, ref UserLoginStatus loginStatus)
        {
            return ValidateUser(portalId, username, password, "DNN", verificationCode, portalName, ip, ref loginStatus);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's credentials against the Data Store
        /// </summary>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="username">The user name of the User attempting to log in</param>
        /// <param name="password">The password of the User attempting to log in</param>
        /// <param name="authType">The type of Authentication Used</param>
        /// <param name="verificationCode">The verification code of the User attempting to log in</param>
        /// <param name="portalName">The name of the Portal</param>
        /// <param name="ip">The IP Address of the user attempting to log in</param>
        /// <param name="loginStatus">A UserLoginStatus enumeration that indicates the status of the 
        /// Login attempt.  This value is returned by reference.</param>
        /// <returns>The UserInfo object representing a valid user</returns>
        /// -----------------------------------------------------------------------------
        public static UserInfo ValidateUser(int portalId, string username, string password, string authType, string verificationCode, string portalName, string ip, ref UserLoginStatus loginStatus)
        {
            loginStatus = UserLoginStatus.LOGIN_FAILURE;

            //Try and Log the user in
            var user = MembershipProvider.Instance().UserLogin(GetEffectivePortalId(portalId), username, password, authType, verificationCode, ref loginStatus);
			if (loginStatus == UserLoginStatus.LOGIN_USERLOCKEDOUT || loginStatus == UserLoginStatus.LOGIN_FAILURE || loginStatus == UserLoginStatus.LOGIN_USERNOTAPPROVED)
            {
                //User Locked Out so log to event log
                AddEventLog(portalId, username, Null.NullInteger, portalName, ip, loginStatus);
            }

            //Check Default Accounts
            if (loginStatus == UserLoginStatus.LOGIN_SUCCESS || loginStatus == UserLoginStatus.LOGIN_SUPERUSER)
            {
                CheckInsecurePassword(username, password, ref loginStatus);
            }

            //Reset portalId
            FixMemberPortalId(user, portalId);

            //return the User object
            return user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Validates a User's Password and Profile
        /// </summary>
        /// <remarks>This overload takes a valid User (Credentials check out) and check whether the Password and Profile need updating</remarks>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="objUser">The user attempting to log in</param>
        /// <param name="ignoreExpiring">Ingore expired user.</param>
        /// <returns>The UserLoginStatus</returns>
        /// -----------------------------------------------------------------------------
        public static UserValidStatus ValidateUser(UserInfo objUser, int portalId, bool ignoreExpiring)
        {
            portalId = GetEffectivePortalId(portalId);

            var validStatus = UserValidStatus.VALID;

            //Check if Password needs to be updated
            if (objUser.Membership.UpdatePassword)
            {
                //Admin has forced password update
                validStatus = UserValidStatus.UPDATEPASSWORD;
            }
            else if (PasswordConfig.PasswordExpiry > 0)
            {
                var expiryDate = objUser.Membership.LastPasswordChangeDate.AddDays(PasswordConfig.PasswordExpiry);
                if (expiryDate < DateTime.Now)
                {
                    //Password Expired
                    validStatus = UserValidStatus.PASSWORDEXPIRED;
                }
                else if (expiryDate < DateTime.Now.AddDays(PasswordConfig.PasswordExpiryReminder) && (!ignoreExpiring))
                {
                    //Password update reminder
                    validStatus = UserValidStatus.PASSWORDEXPIRING;
                }
            }

            //Check if Profile needs updating
            if (validStatus == UserValidStatus.VALID)
            {
                var validProfile = Convert.ToBoolean(UserModuleBase.GetSetting(portalId, "Security_RequireValidProfileAtLogin"));
                if (validProfile && (!ProfileController.ValidateProfile(portalId, objUser.Profile)))
                {
                    validStatus = UserValidStatus.UPDATEPROFILE;
                }
            }
            return validStatus;
        }

        /// <summary>
        /// Tries to validate a verification code sent after a user is registered in a portal configured to use a verified registration.
        /// </summary>
        /// <param name="verificationCode">The verification code.</param>
        /// <returns>An null string if the verification code has been validated and the user has been approved. An error message otherwise.</returns>
        /// <exception cref="DotNetNuke.Entities.Users.UserAlreadyVerifiedException">Thrown when provided verification code has been already used.</exception>
        /// <exception cref="DotNetNuke.Entities.Users.InvalidVerificationCodeException">Thrown when the provided verification code is invalid.</exception>
        /// <exception cref="DotNetNuke.Entities.Users.UserDoesNotExistException">Thrown when the user does not exist.</exception>
        public static void VerifyUser(string verificationCode)
        {
            Requires.NotNullOrEmpty("verificationCode", verificationCode);

            var portalSecurity = new PortalSecurity();
            var decryptString = portalSecurity.DecryptString(verificationCode, Config.GetDecryptionkey());
            var strings = decryptString.Split('-');
            
            if (strings.Length != 2)
            {
                throw new InvalidVerificationCodeException();
            }

            int portalId;
            int userId;

            if (!int.TryParse(strings[0], out portalId) || !int.TryParse(strings[1], out userId))
            {
                throw new InvalidVerificationCodeException();
            }

            var user = GetUserById(int.Parse(strings[0]), int.Parse(strings[1]));
            
            if (user == null)
            {
                throw new UserDoesNotExistException();
            }
            
            if (user.Membership.Approved)
            {
                throw new UserAlreadyVerifiedException();
            }

            if (!user.IsInRole("Unverified Users"))
            {
                // A Registered User that has been unapproved has managed to get a valid verification code
                throw new InvalidVerificationCodeException();
            }

            user.Membership.Approved = true;
            UpdateUser(portalId, user);
            ApproveUser(user);
        }

        #endregion

    }
}
