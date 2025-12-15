// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration.Provider;
    using System.Data;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Membership;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;

    // DNN-4016
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The AspNetMembershipProvider overrides the default MembershipProvider to provide an AspNet Membership Component (MemberRole) implementation.</summary>
    public partial class AspNetMembershipProvider : MembershipProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AspNetMembershipProvider));
        private static Random random = new Random();

        private readonly DataProvider dataProvider = DataProvider.Instance();
        private readonly IEnumerable<string> socialAuthProviders = new List<string>() { "Facebook", "Google", "Twitter", "LiveID" };
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="AspNetMembershipProvider"/> class.</summary>
        public AspNetMembershipProvider()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AspNetMembershipProvider"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public AspNetMembershipProvider(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <inheritdoc/>
        public override bool CanEditProviderProperties
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override int MaxInvalidPasswordAttempts
        {
            get
            {
                return System.Web.Security.Membership.MaxInvalidPasswordAttempts;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override int MinNonAlphanumericCharacters
        {
            get
            {
                return System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override int MinPasswordLength
        {
            get
            {
                return System.Web.Security.Membership.MinRequiredPasswordLength;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override int PasswordAttemptWindow
        {
            get
            {
                return System.Web.Security.Membership.PasswordAttemptWindow;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override PasswordFormat PasswordFormat
        {
            get
            {
                switch (System.Web.Security.Membership.Provider.PasswordFormat)
                {
                    case MembershipPasswordFormat.Encrypted:
                        return PasswordFormat.Encrypted;
                    case MembershipPasswordFormat.Hashed:
                        return PasswordFormat.Hashed;
                    default:
                        return PasswordFormat.Clear;
                }
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override bool PasswordResetEnabled
        {
            get
            {
                return System.Web.Security.Membership.EnablePasswordReset;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override bool PasswordRetrievalEnabled
        {
            get
            {
                return System.Web.Security.Membership.EnablePasswordRetrieval;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override string PasswordStrengthRegularExpression
        {
            get
            {
                return System.Web.Security.Membership.PasswordStrengthRegularExpression;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override bool RequiresQuestionAndAnswer
        {
            get
            {
                return System.Web.Security.Membership.RequiresQuestionAndAnswer;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        /// <inheritdoc/>
        public override bool RequiresUniqueEmail
        {
            get
            {
                return System.Web.Security.Membership.Provider.RequiresUniqueEmail;
            }

            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public static ArrayList FillUserCollection(int portalId, IDataReader dr)
        {
            // Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);

                    // add to collection
                    arrUsers.Add(user);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return arrUsers;
        }

        /// <inheritdoc/>
        public override UserInfo GetUserByAuthToken(int portalId, string userToken, string authType)
        {
            IDataReader dr = this.dataProvider.GetUserByAuthToken(portalId, userToken, authType);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
            return objUserInfo;
        }

        /// <inheritdoc />
        public override void AddUserPortal(int portalId, int userId)
        {
            Requires.NotNullOrEmpty("portalId", portalId.ToString());
            Requires.NotNullOrEmpty("userId", userId.ToString());
            this.dataProvider.AddUserPortal(portalId, userId);
        }

        /// <inheritdoc />
        public override void ChangeUsername(int userId, string newUsername)
        {
            Requires.NotNull("userId", userId);
            Requires.NotNullOrEmpty("newUsername", newUsername);

            var userName = PortalSecurity.Instance.InputFilter(
                newUsername,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);

            if (!userName.Equals(newUsername))
            {
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
            }

            var valid = UserController.Instance.IsValidUserName(userName);

            if (!valid)
            {
                throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
            }

            // read all the user account settings
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (portalSettings != null)
            {
                var settings = UserController.GetUserSettings(portalSettings.PortalId);

                // User Name Validation
                var userNameValidator = GetStringSetting(settings, "Security_UserNameValidation");
                if (!string.IsNullOrEmpty(userNameValidator))
                {
                    var regExp = RegexUtils.GetCachedRegex(userNameValidator, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    var matches = regExp.Matches(userName);
                    if (matches.Count == 0)
                    {
                        throw new ArgumentException(Localization.GetExceptionMessage("InvalidUserName", "The username specified is invalid."));
                    }
                }
            }

            this.dataProvider.ChangeUsername(userId, userName);

            EventLogController.Instance.AddLog(
                "userId",
                userId.ToString(),
                portalSettings,
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.USERNAME_UPDATED);

            DataCache.ClearCache();
        }

        /// <inheritdoc />
        public override bool ChangePassword(UserInfo user, string oldPassword, string newPassword)
        {
            MembershipUser aspnetUser = GetMembershipUser(user);

            var m = new MembershipPasswordController();
            if (m.IsPasswordInHistory(user.UserID, user.PortalID, newPassword))
            {
                return false;
            }

            if (string.IsNullOrEmpty(oldPassword))
            {
                aspnetUser.UnlockUser();
                oldPassword = aspnetUser.GetPassword();
            }

            bool retValue = aspnetUser.ChangePassword(oldPassword, newPassword);
            if (retValue && this.PasswordRetrievalEnabled && !this.RequiresQuestionAndAnswer)
            {
                string confirmPassword = aspnetUser.GetPassword();
                if (confirmPassword == newPassword)
                {
                    user.Membership.Password = confirmPassword;
                }
                else
                {
                    retValue = false;
                }
            }

            return retValue;
        }

        /// <inheritdoc />
        public override bool ChangePasswordQuestionAndAnswer(UserInfo user, string password, string passwordQuestion, string passwordAnswer)
        {
            MembershipUser aspnetUser = GetMembershipUser(user);
            if (password == Null.NullString)
            {
                password = aspnetUser.GetPassword();
            }

            return aspnetUser.ChangePasswordQuestionAndAnswer(password, passwordQuestion, passwordAnswer);
        }

        /// <inheritdoc />
        public override UserCreateStatus CreateUser(ref UserInfo user)
        {
            UserCreateStatus createStatus = ValidateForProfanity(user);
            string service = HttpContext.Current != null ? HttpContext.Current.Request.Params["state"] : string.Empty;

            if (createStatus == UserCreateStatus.AddUser)
            {
                this.ValidateForDuplicateDisplayName(user, ref createStatus);
            }

            if (createStatus == UserCreateStatus.AddUser)
            {
                try
                {
                    // check if username exists in database for any portal
                    UserInfo objVerifyUser = this.GetUserByUserName(Null.NullInteger, user.Username);
                    if (objVerifyUser != null)
                    {
                        // DNN-4016
                        // the username exists so we should now verify the password, DNN-4016 or check for oauth user authentication.
                        if (ValidateUser(user.Username, user.Membership.Password))
                        {
                            // check if user exists for the portal specified
                            objVerifyUser = this.GetUserByUserName(user.PortalID, user.Username);
                            if (objVerifyUser != null)
                            {
                                if (objVerifyUser.PortalID == user.PortalID && (!user.IsSuperUser || user.PortalID == Null.NullInteger))
                                {
                                    createStatus = UserCreateStatus.UserAlreadyRegistered;
                                }
                                else
                                {
                                    // SuperUser who is not part of portal
                                    createStatus = UserCreateStatus.AddUserToPortal;
                                }
                            }
                            else
                            {
                                createStatus = UserCreateStatus.AddUserToPortal;
                            }
                        }
                        else
                        {
                            // not the same person - prevent registration
                            createStatus = UserCreateStatus.UsernameAlreadyExists;
                        }
                    }
                    else
                    {
                        // the user does not exist
                        createStatus = UserCreateStatus.AddUser;
                    }

                    // If new user - add to aspnet membership
                    if (createStatus == UserCreateStatus.AddUser)
                    {
                        createStatus = CreateMembershipUser(user);
                    }

                    // If asp user has been successfully created or we are adding a existing user
                    // to a new portal
                    if (createStatus == UserCreateStatus.Success || createStatus == UserCreateStatus.AddUserToPortal)
                    {
                        // Create the DNN User Record
                        createStatus = this.CreateDNNUser(ref user);
                        if (createStatus == UserCreateStatus.Success)
                        {
                            // Persist the Profile to the Data Store
                            ProfileController.UpdateUserProfile(user);
                        }
                    }
                }
                catch (Exception exc)
                {
                    // an unexpected error occurred
                    Exceptions.LogException(exc);
                    createStatus = UserCreateStatus.UnexpectedError;
                }
            }

            return createStatus;
        }

        /// <inheritdoc />
        public override bool DeleteUser(UserInfo user)
        {
            bool retValue = true;
            try
            {
                this.dataProvider.DeleteUserFromPortal(user.UserID, user.PortalID);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                retValue = false;
            }

            return retValue;
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial void DeleteUsersOnline(int timeWindow)
        {
            this.dataProvider.DeleteUsersOnline(timeWindow);
        }

        /// <inheritdoc />
        public override string GeneratePassword()
        {
            return this.GeneratePassword(this.MinPasswordLength + 4);
        }

        /// <inheritdoc />
        public override string GeneratePassword(int length)
        {
            return System.Web.Security.Membership.GeneratePassword(length, this.MinNonAlphanumericCharacters);
        }

        /// <inheritdoc/>
        public override ArrayList GetDeletedUsers(int portalId)
        {
            return FillUserCollection(portalId, this.dataProvider.GetDeletedUsers(portalId));
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial ArrayList GetOnlineUsers(int portalId)
        {
            int totalRecords = 0;
            return FillUserCollection(portalId, this.dataProvider.GetOnlineUsers(portalId), ref totalRecords);
        }

        /// <inheritdoc />
        public override string GetPassword(UserInfo user, string passwordAnswer)
        {
            MembershipUser aspnetUser = GetMembershipUser(user);
            if (aspnetUser.IsLockedOut)
            {
                AutoUnlockUser(this.hostSettings, aspnetUser);
            }

            return this.RequiresQuestionAndAnswer ? aspnetUser.GetPassword(passwordAnswer) : aspnetUser.GetPassword();
        }

        /// <inheritdoc/>
        public override ArrayList GetUnAuthorizedUsers(int portalId)
        {
            return this.GetUnAuthorizedUsers(portalId, false, false);
        }

        /// <inheritdoc/>
        public override ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
            return FillUserCollection(
                portalId,
                this.dataProvider.GetUnAuthorizedUsers(portalId, includeDeleted, superUsersOnly));
        }

        /// <inheritdoc />
        public override UserInfo GetUser(int portalId, int userId)
        {
            IDataReader dr = this.dataProvider.GetUser(portalId, userId);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
            return objUserInfo;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByDisplayName(int portalId, string displayName)
        {
            IDataReader dr = this.dataProvider.GetUserByDisplayName(portalId, displayName);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
            return objUserInfo;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByUserName(int portalId, string username)
        {
            return CBO.GetCachedObject<UserInfo>(
                new CacheItemArgs(
                    string.Format(DataCache.UserCacheKey, portalId, username),
                    DataCache.UserCacheTimeOut,
                    DataCache.UserCachePriority),
                _ => this.GetUserByUserNameFromDataStore(portalId, username));
        }

        /// <inheritdoc />
        public override UserInfo GetUserByVanityUrl(int portalId, string vanityUrl)
        {
            UserInfo user = null;
            if (!string.IsNullOrEmpty(vanityUrl))
            {
                IDataReader dr = this.dataProvider.GetUserByVanityUrl(portalId, vanityUrl);
                user = FillUserInfo(portalId, dr, true);
            }

            return user;
        }

        /// <inheritdoc />
        public override UserInfo GetUserByPasswordResetToken(int portalId, string resetToken)
        {
            UserInfo user = null;
            if (!string.IsNullOrEmpty(resetToken))
            {
                IDataReader dr = this.dataProvider.GetUserByPasswordResetToken(portalId, resetToken);
                user = FillUserInfo(portalId, dr, true);
            }

            return user;
        }

        /// <inheritdoc/>
        public override string GetProviderUserKey(UserInfo user)
        {
            return GetMembershipUser(user).ProviderUserKey?.ToString().Replace("-", string.Empty) ?? string.Empty;
        }

        /// <inheritdoc/>
        public override UserInfo GetUserByProviderUserKey(int portalId, string providerUserKey)
        {
            var userName = GetMembershipUserByUserKey(providerUserKey)?.UserName ?? string.Empty;
            if (string.IsNullOrEmpty(userName))
            {
                return null;
            }

            return this.GetUserByUserName(portalId, userName);
        }

        /// <inheritdoc />
        public override int GetUserCountByPortal(int portalId)
        {
            return this.dataProvider.GetUserCountByPortal(portalId);
        }

        /// <inheritdoc />
        public override void GetUserMembership(ref UserInfo user)
        {
            // Get AspNet MembershipUser
            MembershipUser aspnetUser = GetMembershipUser(user);

            // Fill Membership Property
            FillUserMembership(aspnetUser, user);

            // Get Online Status
            user.Membership.IsOnLine = this.IsUserOnline(user);
        }

        /// <inheritdoc />
        public override ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords)
        {
            return this.GetUsers(portalId, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetAllUsers(
                    portalId,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc/>
        public override IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationshipTypeId, bool isAdmin, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyNames, string propertyValues)
        {
            return FillUserList(
                portalId,
                this.dataProvider.GetUsersAdvancedSearch(
                    portalId,
                    userId,
                    filterUserId,
                    filterRoleId,
                    relationshipTypeId,
                    isAdmin,
                    pageIndex,
                    pageSize,
                    sortColumn,
                    sortAscending,
                    propertyNames,
                    propertyValues));
        }

        /// <inheritdoc/>
        public override IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyName, string propertyValue)
        {
            return FillUserList(
                portalId,
                this.dataProvider.GetUsersBasicSearch(
                    portalId,
                    pageIndex,
                    pageSize,
                    sortColumn,
                    sortAscending,
                    propertyName,
                    propertyValue));
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return this.GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByEmail(
                    portalId,
                    emailToMatch,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return this.GetUsersByUserName(portalId, userNameToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByUsername(
                    portalId,
                    userNameToMatch,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByDisplayName(int portalId, string nameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByDisplayname(
                    portalId,
                    nameToMatch,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords)
        {
            return this.GetUsersByProfileProperty(
                portalId,
                propertyName,
                propertyValue,
                pageIndex,
                pageSize,
                ref totalRecords,
                false,
                false);
        }

        /// <inheritdoc />
        public override ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(
                portalId,
                this.dataProvider.GetUsersByProfileProperty(
                    portalId,
                    propertyName,
                    propertyValue,
                    pageIndex,
                    pageSize,
                    includeDeleted,
                    superUsersOnly),
                ref totalRecords);
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial bool IsUserOnline(UserInfo user)
        {
            bool isOnline = false;
            var objUsersOnline = new UserOnlineController();
            if (objUsersOnline.IsEnabled())
            {
                Hashtable userList = objUsersOnline.GetUserList();
                var onlineUser = (OnlineUserInfo)userList[user.UserID.ToString()];
                if (onlineUser != null)
                {
                    isOnline = true;
                }
                else
                {
                    // Next try the Database
                    onlineUser = CBO.FillObject<OnlineUserInfo>(this.dataProvider.GetOnlineUser(user.UserID));
                    if (onlineUser != null)
                    {
                        isOnline = true;
                    }
                }
            }

            return isOnline;
        }

        /// <inheritdoc/>
        public override bool RemoveUser(UserInfo user)
        {
            bool retValue = true;

            try
            {
                foreach (var relationship in user.Social.UserRelationships)
                {
                    RelationshipController.Instance.DeleteUserRelationship(relationship);
                }

                this.dataProvider.RemoveUser(user.UserID, user.PortalID);

                // Prior to removing membership, ensure user is not present in any other portal
                UserInfo otherUser = this.GetUserByUserNameFromDataStore(Null.NullInteger, user.Username);
                if (otherUser == null)
                {
                    DeleteMembershipUser(user);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                retValue = false;
            }

            return retValue;
        }

        /// <inheritdoc />
        public override string ResetPassword(UserInfo user, string passwordAnswer)
        {
            // Get AspNet MembershipUser
            MembershipUser aspnetUser = GetMembershipUser(user);

            return this.RequiresQuestionAndAnswer ? aspnetUser.ResetPassword(passwordAnswer) : aspnetUser.ResetPassword();
        }

        /// <inheritdoc />
        public override bool ResetAndChangePassword(UserInfo user, string newPassword)
        {
            return this.ResetAndChangePassword(user, newPassword, string.Empty);
        }

        /// <inheritdoc/>
        public override bool ResetAndChangePassword(UserInfo user, string newPassword, string answer)
        {
            if (this.RequiresQuestionAndAnswer && string.IsNullOrEmpty(answer))
            {
                return false;
            }

            // Get AspNet MembershipUser
            MembershipUser aspnetUser = GetMembershipUser(user);
            if (aspnetUser.IsLockedOut)
            {
                aspnetUser.UnlockUser();
            }

            string resetPassword = this.ResetPassword(user, answer);
            return aspnetUser.ChangePassword(resetPassword, newPassword);
        }

        /// <inheritdoc/>
        public override bool RestoreUser(UserInfo user)
        {
            bool retValue = true;

            try
            {
                this.dataProvider.RestoreUser(user.UserID, user.PortalID);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                retValue = false;
            }

            return retValue;
        }

        /// <inheritdoc />
        public override bool UnLockUser(UserInfo user)
        {
            MembershipUser membershipUser = System.Web.Security.Membership.GetUser(user.Username);
            bool retValue = false;
            if (membershipUser != null)
            {
                retValue = membershipUser.UnlockUser();
            }

            DataCache.RemoveCache(GetCacheKey(user.Username));
            return retValue;
        }

        /// <inheritdoc />
        public override void UserAgreedToTerms(UserInfo user)
        {
            this.dataProvider.UserAgreedToTerms(PortalController.GetEffectivePortalId(user.PortalID), user.UserID);
        }

        /// <inheritdoc />
        public override void ResetTermsAgreement(int portalId)
        {
            this.dataProvider.ResetTermsAgreement(portalId);
        }

        /// <inheritdoc />
        public override void UserRequestsRemoval(UserInfo user, bool remove)
        {
            this.dataProvider.UserRequestsRemoval(user.PortalID, user.UserID, remove);
        }

        /// <inheritdoc />
        public override void UpdateUser(UserInfo user)
        {
            var objSecurity = PortalSecurity.Instance;
            string firstName = objSecurity.InputFilter(
                user.FirstName,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            string lastName = objSecurity.InputFilter(
                user.LastName,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            string email = objSecurity.InputFilter(
                user.Email,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            string displayName = objSecurity.InputFilter(
                user.DisplayName,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            if (displayName.Contains("<"))
            {
                displayName = HttpUtility.HtmlEncode(displayName);
            }

            if (!firstName.Equals(user.FirstName))
            {
                user.FirstName = firstName;
            }

            if (!lastName.Equals(user.LastName))
            {
                user.LastName = lastName;
            }

            bool updatePassword = user.Membership.UpdatePassword;
            bool isApproved = user.Membership.Approved;
            if (string.IsNullOrEmpty(displayName))
            {
                displayName = firstName + " " + lastName;
            }

            // Persist the Membership to the Data Store
            UpdateUserMembership(user);

            // Persist the DNN User to the Database
            this.dataProvider.UpdateUser(
                user.UserID,
                user.PortalID,
                firstName,
                lastName,
                user.IsSuperUser,
                email,
                displayName,
                user.VanityUrl,
                updatePassword,
                isApproved,
                false,
                user.LastIPAddress,
                user.PasswordResetToken,
                user.PasswordResetExpiration,
                user.IsDeleted,
                UserController.Instance.GetCurrentUserInfo().UserID);

            // Persist the Profile to the Data Store
            ProfileController.UpdateUserProfile(user);
        }

        /// <inheritdoc />
        [DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 11)]
        public override partial void UpdateUsersOnline(Hashtable userList)
        {
            this.dataProvider.UpdateUsersOnline(userList);
        }

        /// <inheritdoc />
        public override UserInfo UserLogin(int portalId, string username, string password, string verificationCode, ref UserLoginStatus loginStatus)
        {
            return this.UserLogin(portalId, username, password, "DNN", verificationCode, ref loginStatus);
        }

        /// <inheritdoc />
        public override UserInfo UserLogin(int portalId, string username, string password, string authType, string verificationCode, ref UserLoginStatus loginStatus)
        {
            // For now, we are going to ignore the possibility that the User may exist in the
            // Global Data Store but not in the Local DataStore ie. A shared Global Data Store

            // Initialise Login Status to Failure
            loginStatus = UserLoginStatus.LOGIN_FAILURE;

            DataCache.ClearUserCache(portalId, username);
            DataCache.ClearCache(GetCacheKey(username));

            // Get a light-weight (unhydrated) DNN User from the Database, we will hydrate it later if neccessary
            UserInfo user = (authType == "DNN")
                                ? this.GetUserByUserName(portalId, username)
                                : this.GetUserByAuthToken(portalId, verificationCode, authType);
            if (user != null && !user.IsDeleted)
            {
                // Get AspNet MembershipUser
                MembershipUser aspnetUser = GetMembershipUser(user);

                // Fill Membership Property from AspNet MembershipUser
                FillUserMembership(aspnetUser, user);

                // Check if the User is Locked Out (and unlock if AutoUnlock has expired)
                if (aspnetUser.IsLockedOut)
                {
                    if (AutoUnlockUser(this.hostSettings, aspnetUser))
                    {
                        // Unlock User
                        user.Membership.LockedOut = false;
                    }
                    else
                    {
                        loginStatus = UserLoginStatus.LOGIN_USERLOCKEDOUT;
                    }
                }

                // Check in a verified situation whether the user is Approved
                if (user.Membership.Approved == false && user.IsSuperUser == false)
                {
                    // Check Verification code (skip for FB, Google, Twitter, LiveID as it has no verification code)
                    if (this.socialAuthProviders.Contains(authType) && string.IsNullOrEmpty(verificationCode))
                    {
                        if (PortalController.Instance.GetCurrentPortalSettings().UserRegistration ==
                            (int)Globals.PortalRegistrationType.PublicRegistration)
                        {
                            user.Membership.Approved = true;
                            UserController.UpdateUser(portalId, user);
                            UserController.ApproveUser(user);
                        }
                        else
                        {
                            loginStatus = UserLoginStatus.LOGIN_USERNOTAPPROVED;
                        }
                    }
                    else
                    {
                        var ps = PortalSecurity.Instance;
                        if (verificationCode == ps.Encrypt(Config.GetDecryptionkey(), portalId + "-" + user.UserID))
                        {
                            UserController.ApproveUser(user);
                        }
                        else
                        {
                            loginStatus = UserLoginStatus.LOGIN_USERNOTAPPROVED;
                        }
                    }
                }

                // Verify User Credentials
                bool bValid = false;
                loginStatus = ValidateLogin(username, authType, user, loginStatus, password, ref bValid, portalId);
                if (!bValid)
                {
                    // Clear the user object
                    user = null;

                    // Clear cache for user so that locked out & other status could be updated
                    DataCache.ClearUserCache(portalId, username);
                    DataCache.ClearCache(GetCacheKey(username));

                    aspnetUser = System.Web.Security.Membership.GetUser(username);

                    // If user has been locked out for current invalid attempt
                    // return locked out status
                    if (aspnetUser.IsLockedOut)
                    {
                        loginStatus = UserLoginStatus.LOGIN_USERLOCKEDOUT;
                    }
                }
            }
            else
            {
                // Clear the user object
                user = null;
            }

            return user;
        }

        private static bool AutoUnlockUser(IHostSettings hostSettings, MembershipUser aspNetUser)
        {
            if (hostSettings.AutoAccountUnlockDuration > TimeSpan.Zero)
            {
                if (aspNetUser.LastLockoutDate < DateTime.Now.Subtract(hostSettings.AutoAccountUnlockDuration))
                {
                    // Unlock user in Data Store
                    if (aspNetUser.UnlockUser())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static UserCreateStatus CreateMembershipUser(UserInfo user)
        {
            var portalSecurity = PortalSecurity.Instance;
            string userName = portalSecurity.InputFilter(
                user.Username,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            string email = portalSecurity.InputFilter(
                user.Email,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            MembershipCreateStatus status;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                System.Web.Security.Membership.CreateUser(
                    userName,
                    user.Membership.Password,
                    email,
                    user.Membership.PasswordQuestion,
                    user.Membership.PasswordAnswer,
                    true,
                    out status);
            }
            else
            {
                System.Web.Security.Membership.CreateUser(
                    userName,
                    user.Membership.Password,
                    email,
                    null,
                    null,
                    true,
                    out status);
            }

            var createStatus = UserCreateStatus.Success;
            switch (status)
            {
                case MembershipCreateStatus.DuplicateEmail:
                    createStatus = UserCreateStatus.DuplicateEmail;
                    break;
                case MembershipCreateStatus.DuplicateProviderUserKey:
                    createStatus = UserCreateStatus.DuplicateProviderUserKey;
                    break;
                case MembershipCreateStatus.DuplicateUserName:
                    createStatus = UserCreateStatus.DuplicateUserName;
                    break;
                case MembershipCreateStatus.InvalidAnswer:
                    createStatus = UserCreateStatus.InvalidAnswer;
                    break;
                case MembershipCreateStatus.InvalidEmail:
                    createStatus = UserCreateStatus.InvalidEmail;
                    break;
                case MembershipCreateStatus.InvalidPassword:
                    createStatus = UserCreateStatus.InvalidPassword;
                    break;
                case MembershipCreateStatus.InvalidProviderUserKey:
                    createStatus = UserCreateStatus.InvalidProviderUserKey;
                    break;
                case MembershipCreateStatus.InvalidQuestion:
                    createStatus = UserCreateStatus.InvalidQuestion;
                    break;
                case MembershipCreateStatus.InvalidUserName:
                    createStatus = UserCreateStatus.InvalidUserName;
                    break;
                case MembershipCreateStatus.ProviderError:
                    createStatus = UserCreateStatus.ProviderError;
                    break;
                case MembershipCreateStatus.UserRejected:
                    createStatus = UserCreateStatus.UserRejected;
                    break;
            }

            return createStatus;
        }

        private static void DeleteMembershipUser(UserInfo user)
        {
            try
            {
                System.Web.Security.Membership.DeleteUser(user.Username, true);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        private static ArrayList FillUserCollection(int portalId, IDataReader dr, ref int totalRecords)
        {
            // Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);

                    // add to collection
                    arrUsers.Add(user);
                }

                // Get the next result (containing the total)
                dr.NextResult();

                // Get the total no of records from the second result
                totalRecords = Globals.GetTotalRecords(ref dr);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return arrUsers;
        }

        private static IList<UserInfo> FillUserList(int portalId, IDataReader dr)
        {
            var users = new List<UserInfo>();
            try
            {
                while (dr.Read())
                {
                    // fill business object
                    UserInfo user = FillUserAndProfile(portalId, dr);

                    // add to collection
                    users.Add(user);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                // close datareader
                CBO.CloseDataReader(dr, true);
            }

            return users;
        }

        private static UserInfo FillUserAndProfile(int portalId, IDataReader dr)
        {
            UserInfo user = null;
            bool bContinue = string.Equals(dr.GetName(0), "UserID", StringComparison.InvariantCultureIgnoreCase);

            // Ensure the data reader returned is valid
            if (bContinue)
            {
                user = new UserInfo
                {
                    PortalID = Null.SetNullInteger(dr["PortalID"]),
                    IsSuperUser = Null.SetNullBoolean(dr["IsSuperUser"]),
                    IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]),
                    UserID = Null.SetNullInteger(dr["UserID"]),
                    DisplayName = Null.SetNullString(dr["DisplayName"]),
                    Username = Null.SetNullString(dr["Username"]),
                    Email = Null.SetNullString(dr["Email"]),
                    AffiliateID = Null.SetNullInteger(dr["AffiliateID"]),
                };
                user.AffiliateID = Null.SetNullInteger(Null.SetNull(dr["AffiliateID"], user.AffiliateID));

                UserController.GetUserMembership(user);
                user.Membership.UpdatePassword = Null.SetNullBoolean(dr["UpdatePassword"]);
                if (!user.IsSuperUser)
                {
                    user.Membership.Approved = Null.SetNullBoolean(dr["Authorised"]);
                }

                if (user.PortalID == Null.NullInteger)
                {
                    user.PortalID = portalId;
                }

                var userProfile = new UserProfile(user);
                userProfile.InitialiseProfile(portalId);

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    switch (dr.GetName(i))
                    {
                        case "PortalID":
                        case "IsSuperUser":
                        case "IsDeleted":
                        case "UserID":
                        case "DisplayName":
                        case "Username":
                        case "Email":
                        case "AffiliateID":
                        case "UpdatePassword":
                        case "Authorised":
                        case "CreateDate":
                        case "LastActivityDate":
                        case "LastLockoutDate":
                        case "LastLoginDate":
                        case "LastPasswordChangedDate":
                        case "IsLockedOut":
                        case "PasswordQuestion":
                        case "IsApproved":
                        case "PasswordResetToken":
                        case "PasswordResetExpiration":
                            break;
                        default:
                            // Probably a profile property
                            string name = dr.GetName(i);
                            userProfile.SetProfileProperty(name, Null.SetNullString(dr[name]));
                            break;
                    }
                }

                user.Profile = userProfile;
            }

            return user;
        }

        private static UserInfo FillUserInfo(int portalId, IDataReader dr, bool closeDataReader)
        {
            UserInfo user = null;
            try
            {
                // read datareader
                bool bContinue = true;
                if (closeDataReader)
                {
                    bContinue = false;
                    if (dr.Read())
                    {
                        // Ensure the data reader returned is valid
                        if (string.Equals(dr.GetName(0), "UserID", StringComparison.InvariantCultureIgnoreCase))
                        {
                            bContinue = true;
                        }
                    }
                }

                if (bContinue)
                {
                    user = new UserInfo
                    {
                        PortalID = Null.SetNullInteger(dr["PortalID"]),
                        IsSuperUser = Null.SetNullBoolean(dr["IsSuperUser"]),
                        UserID = Null.SetNullInteger(dr["UserID"]),
                        DisplayName = Null.SetNullString(dr["DisplayName"]),
                        LastIPAddress = Null.SetNullString(dr["LastIPAddress"]),
                    };

                    var schema = dr.GetSchemaTable();
                    if (schema != null)
                    {
                        if (schema.Select("ColumnName = 'IsDeleted'").Length > 0)
                        {
                            user.IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]);
                        }

                        if (schema.Select("ColumnName = 'VanityUrl'").Length > 0)
                        {
                            user.VanityUrl = Null.SetNullString(dr["VanityUrl"]);
                        }

                        if (schema.Select("ColumnName = 'HasAgreedToTerms'").Length > 0)
                        {
                            user.HasAgreedToTerms = Null.SetNullBoolean(dr["HasAgreedToTerms"]);
                        }

                        if (schema.Select("ColumnName = 'HasAgreedToTermsOn'").Length > 0)
                        {
                            user.HasAgreedToTermsOn = Null.SetNullDateTime(dr["HasAgreedToTermsOn"]);
                        }
                        else
                        {
                            user.HasAgreedToTermsOn = Null.NullDate;
                        }

                        if (schema.Select("ColumnName = 'RequestsRemoval'").Length > 0)
                        {
                            user.RequestsRemoval = Null.SetNullBoolean(dr["RequestsRemoval"]);
                        }

                        if (schema.Select("ColumnName = 'PasswordResetExpiration'").Length > 0)
                        {
                            user.PasswordResetExpiration = Null.SetNullDateTime(dr["PasswordResetExpiration"]);
                        }

                        if (schema.Select("ColumnName = 'PasswordResetToken'").Length > 0)
                        {
                            user.PasswordResetToken = Null.SetNullGuid(dr["PasswordResetToken"]);
                        }
                    }

                    user.AffiliateID = Null.SetNullInteger(Null.SetNull(dr["AffiliateID"], user.AffiliateID));
                    user.Username = Null.SetNullString(dr["Username"]);
                    UserController.GetUserMembership(user);
                    user.Email = Null.SetNullString(dr["Email"]);
                    user.Membership.UpdatePassword = Null.SetNullBoolean(dr["UpdatePassword"]);

                    if (!user.IsSuperUser)
                    {
                        user.Membership.Approved = Null.SetNullBoolean(dr["Authorised"]);
                    }

                    if (user.PortalID == Null.NullInteger)
                    {
                        user.PortalID = portalId;
                    }

                    user.FillBaseProperties(dr);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, closeDataReader);
            }

            return user;
        }

        private static void FillUserMembership(MembershipUser aspNetUser, UserInfo user)
        {
            // Fill Membership Property
            if (aspNetUser != null)
            {
                if (user.Membership == null)
                {
                    user.Membership = new UserMembership(user);
                }

                user.Membership.CreatedDate = aspNetUser.CreationDate;
                user.Membership.LastActivityDate = aspNetUser.LastActivityDate;
                user.Membership.LastLockoutDate = aspNetUser.LastLockoutDate;
                user.Membership.LastLoginDate = aspNetUser.LastLoginDate;
                user.Membership.LastPasswordChangeDate = aspNetUser.LastPasswordChangedDate;
                user.Membership.LockedOut = aspNetUser.IsLockedOut;
                user.Membership.PasswordQuestion = aspNetUser.PasswordQuestion;
                user.Membership.IsDeleted = user.IsDeleted;

                if (user.IsSuperUser)
                {
                    // For superusers the Approved info is stored in aspnet membership
                    user.Membership.Approved = aspNetUser.IsApproved;
                }
            }
        }

        private static MembershipUser GetMembershipUser(UserInfo user)
        {
            return GetMembershipUser(user.Username);
        }

        private static MembershipUser GetMembershipUser(string userName)
        {
            return
                CBO.GetCachedObject<MembershipUser>(
                    new CacheItemArgs(
                        GetCacheKey(userName),
                        DataCache.UserCacheTimeOut,
                        DataCache.UserCachePriority,
                        userName),
                    GetMembershipUserCallBack);
        }

        private static MembershipUser GetMembershipUserByUserKey(string userKey)
        {
            return
                CBO.GetCachedObject<MembershipUser>(
                    new CacheItemArgs(
                        GetCacheKey(userKey),
                        DataCache.UserCacheTimeOut,
                        DataCache.UserCachePriority,
                        userKey),
                    GetMembershipUserByUserKeyCallBack);
        }

        private static string GetCacheKey(string cacheKey)
        {
            return $"MembershipUser_{cacheKey}";
        }

        private static object GetMembershipUserCallBack(CacheItemArgs cacheItemArgs)
        {
            string userName = cacheItemArgs.ParamList[0].ToString();

            return System.Web.Security.Membership.GetUser(userName);
        }

        private static object GetMembershipUserByUserKeyCallBack(CacheItemArgs cacheItemArgs)
        {
            string userKey = cacheItemArgs.ParamList[0].ToString();

            return System.Web.Security.Membership.GetUser(new Guid(userKey));
        }

        private static void UpdateUserMembership(UserInfo user)
        {
            var portalSecurity = PortalSecurity.Instance;
            string email = portalSecurity.InputFilter(
                user.Email,
                PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);

            // Persist the Membership Properties to the AspNet Data Store
            MembershipUser membershipUser = System.Web.Security.Membership.GetUser(user.Username);
            membershipUser.Email = email;
            membershipUser.LastActivityDate = DateTime.Now;
            if (user.IsSuperUser)
            {
                membershipUser.IsApproved = user.Membership.Approved;
            }

            try
            {
                System.Web.Security.Membership.UpdateUser(membershipUser);
            }
            catch (ProviderException ex)
            {
                throw new UpdateUserException(Localization.GetExceptionMessage("UpdateUserMembershipFailed", "Asp.net membership update user failed."), ex);
            }

            DataCache.RemoveCache(GetCacheKey(user.Username));
        }

        private static UserLoginStatus ValidateLogin(string username, string authType, UserInfo user, UserLoginStatus loginStatus, string password, ref bool bValid, int portalId)
        {
            if (loginStatus != UserLoginStatus.LOGIN_USERLOCKEDOUT &&
                (loginStatus != UserLoginStatus.LOGIN_USERNOTAPPROVED || user.IsInRole("Unverified Users")))
            {
                if (authType == "DNN")
                {
                    if (user.IsSuperUser)
                    {
                        if (ValidateUser(username, password))
                        {
                            loginStatus = UserLoginStatus.LOGIN_SUPERUSER;
                            bValid = true;
                        }
                    }
                    else
                    {
                        if (ValidateUser(username, password))
                        {
                            loginStatus = UserLoginStatus.LOGIN_SUCCESS;
                            bValid = true;
                        }
                    }
                }
                else
                {
                    if (user.IsSuperUser)
                    {
                        loginStatus = UserLoginStatus.LOGIN_SUPERUSER;
                        bValid = true;
                    }
                    else
                    {
                        loginStatus = UserLoginStatus.LOGIN_SUCCESS;
                        bValid = true;
                    }
                }
            }

            return loginStatus;
        }

        private static bool ValidateUser(string username, string password)
        {
            return System.Web.Security.Membership.ValidateUser(username, password);
        }

        private static string GetStringSetting(Hashtable settings, string settingKey)
        {
            return settings[settingKey] == null ? string.Empty : settings[settingKey].ToString();
        }

        private static UserCreateStatus ValidateForProfanity(UserInfo user)
        {
            var portalSecurity = PortalSecurity.Instance;
            var createStatus = UserCreateStatus.AddUser;

            Hashtable settings = UserController.GetUserSettings(user.PortalID);
            bool useProfanityFilter = Convert.ToBoolean(settings["Registration_UseProfanityFilter"]);

            // Validate Profanity
            if (useProfanityFilter)
            {
                if (!portalSecurity.ValidateInput(user.Username, PortalSecurity.FilterFlag.NoProfanity))
                {
                    createStatus = UserCreateStatus.InvalidUserName;
                }

                if (!string.IsNullOrEmpty(user.DisplayName))
                {
                    if (!portalSecurity.ValidateInput(user.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        createStatus = UserCreateStatus.InvalidDisplayName;
                    }
                }
            }

            return createStatus;
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private UserCreateStatus CreateDNNUser(ref UserInfo user)
        {
            var objSecurity = PortalSecurity.Instance;
            var filterFlags = PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup;
            user.Username = objSecurity.InputFilter(user.Username, filterFlags);
            user.Email = objSecurity.InputFilter(user.Email, filterFlags);
            user.LastName = objSecurity.InputFilter(user.LastName, filterFlags);
            user.FirstName = objSecurity.InputFilter(user.FirstName, filterFlags);
            user.DisplayName = objSecurity.InputFilter(user.DisplayName, filterFlags);
            if (user.DisplayName.Contains("<") || user.DisplayName.Contains(">"))
            {
                user.DisplayName = HttpUtility.HtmlEncode(user.DisplayName);
            }

            var updatePassword = user.Membership.UpdatePassword;
            var isApproved = user.Membership.Approved;
            var createStatus = UserCreateStatus.Success;
            try
            {
                user.UserID =
                    Convert.ToInt32(this.dataProvider.AddUser(
                        user.PortalID,
                        user.Username,
                        user.FirstName,
                        user.LastName,
                        user.AffiliateID,
                        user.IsSuperUser,
                        user.Email,
                        user.DisplayName,
                        updatePassword,
                        isApproved,
                        UserController.Instance.GetCurrentUserInfo().UserID));

                // Save the user password history
                new MembershipPasswordController().IsPasswordInHistory(user.UserID, user.PortalID, user.Membership.Password);
            }
            catch (Exception ex)
            {
                // Clear User (duplicate User information)
                Exceptions.LogException(ex);
                user = null;
                createStatus = UserCreateStatus.ProviderError;
            }

            return createStatus;
        }

        private void ValidateForDuplicateDisplayName(UserInfo user, ref UserCreateStatus createStatus)
        {
            Hashtable settings = UserController.GetUserSettings(user.PortalID);
            bool requireUniqueDisplayName = Convert.ToBoolean(settings["Registration_RequireUniqueDisplayName"]);

            if (requireUniqueDisplayName)
            {
                UserInfo duplicateUser = this.GetUserByDisplayName(user.PortalID, user.DisplayName);
                if (duplicateUser != null)
                {
                    createStatus = UserCreateStatus.DuplicateDisplayName;
                }
            }
        }

        /// <summary>GetUserByUserNameFromDataStore retrieves a User from the DataStore.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="username">The username of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object.</returns>
        private UserInfo GetUserByUserNameFromDataStore(int portalId, string username)
        {
            using var dr = this.dataProvider.GetUserByUsername(portalId, username);
            return FillUserInfo(portalId, dr, true);
        }
    }
}
