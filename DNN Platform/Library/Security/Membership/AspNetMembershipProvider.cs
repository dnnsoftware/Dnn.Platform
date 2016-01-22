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
using System.Configuration.Provider;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;
//DNN-4016
using DotNetNuke.Services.Authentication;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Security.Membership
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Provider.AspNetProvider
    /// Class:      AspNetMembershipProvider
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The AspNetMembershipProvider overrides the default MembershipProvider to provide
    /// an AspNet Membership Component (MemberRole) implementation
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class AspNetMembershipProvider : MembershipProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AspNetMembershipProvider));

        #region Private Members

        private readonly DataProvider _dataProvider = DataProvider.Instance();
        private readonly IEnumerable<string> _socialAuthProviders = new  List<string>() {"Facebook", "Google", "Twitter", "LiveID"}; 

        #endregion

        #region Public Properties

        public override bool CanEditProviderProperties
        {
            get { return false; }
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return System.Web.Security.Membership.MaxInvalidPasswordAttempts; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public override int MinNonAlphanumericCharacters
        {
            get { return System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public override int MinPasswordLength
        {
            get { return System.Web.Security.Membership.MinRequiredPasswordLength; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public override int PasswordAttemptWindow
        {
            get { return System.Web.Security.Membership.PasswordAttemptWindow; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

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

        public override bool PasswordResetEnabled
        {
            get { return System.Web.Security.Membership.EnablePasswordReset; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public override bool PasswordRetrievalEnabled
        {
            get { return System.Web.Security.Membership.EnablePasswordRetrieval; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return System.Web.Security.Membership.PasswordStrengthRegularExpression; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return System.Web.Security.Membership.RequiresQuestionAndAnswer; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        public override bool RequiresUniqueEmail
        {
            get { return System.Web.Security.Membership.Provider.RequiresUniqueEmail; }
            set
            {
                throw new NotSupportedException(
                    "Provider properties for AspNetMembershipProvider must be set in web.config");
            }
        }

        #endregion

        #region Private Methods

        private static bool AutoUnlockUser(MembershipUser aspNetUser)
        {
            if (Host.AutoAccountUnlockDuration != 0)
            {
                if (aspNetUser.LastLockoutDate < DateTime.Now.AddMinutes(-1*Host.AutoAccountUnlockDuration))
                {
                    //Unlock user in Data Store
                    if (aspNetUser.UnlockUser())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private UserCreateStatus CreateDNNUser(ref UserInfo user)
        {
            var objSecurity = PortalSecurity.Instance;
            string userName = objSecurity.InputFilter(user.Username,
                                                      PortalSecurity.FilterFlag.NoScripting |
                                                      PortalSecurity.FilterFlag.NoAngleBrackets |
                                                      PortalSecurity.FilterFlag.NoMarkup);
            string email = objSecurity.InputFilter(user.Email,
                                                   PortalSecurity.FilterFlag.NoScripting |
                                                   PortalSecurity.FilterFlag.NoAngleBrackets |
                                                   PortalSecurity.FilterFlag.NoMarkup);
            string lastName = objSecurity.InputFilter(user.LastName,
                                                      PortalSecurity.FilterFlag.NoScripting |
                                                      PortalSecurity.FilterFlag.NoAngleBrackets |
                                                      PortalSecurity.FilterFlag.NoMarkup);
            string firstName = objSecurity.InputFilter(user.FirstName,
                                                       PortalSecurity.FilterFlag.NoScripting |
                                                       PortalSecurity.FilterFlag.NoAngleBrackets |
                                                       PortalSecurity.FilterFlag.NoMarkup);
            var createStatus = UserCreateStatus.Success;
            string displayName = objSecurity.InputFilter(user.DisplayName,
                                                         PortalSecurity.FilterFlag.NoScripting |
                                                         PortalSecurity.FilterFlag.NoAngleBrackets |
                                                         PortalSecurity.FilterFlag.NoMarkup);
            if (displayName.Contains("<") || displayName.Contains(">"))
            {
                displayName = HttpUtility.HtmlEncode(displayName);
            }
            bool updatePassword = user.Membership.UpdatePassword;
            bool isApproved = user.Membership.Approved;
            try
            {
                user.UserID =
                    Convert.ToInt32(_dataProvider.AddUser(user.PortalID,
                                                          userName,
                                                          firstName,
                                                          lastName,
                                                          user.AffiliateID,
                                                          user.IsSuperUser,
                                                          email,
                                                          displayName,
                                                          updatePassword,
                                                          isApproved,
                                                          UserController.Instance.GetCurrentUserInfo().UserID));
            }
            catch (Exception ex)
            {
                //Clear User (duplicate User information)
                Exceptions.LogException(ex);
                user = null;
                createStatus = UserCreateStatus.ProviderError;
            }
            return createStatus;
        }

        private static UserCreateStatus CreateMemberhipUser(UserInfo user)
        {
            var portalSecurity = PortalSecurity.Instance;
            string userName = portalSecurity.InputFilter(user.Username,
                                                         PortalSecurity.FilterFlag.NoScripting |
                                                         PortalSecurity.FilterFlag.NoAngleBrackets |
                                                         PortalSecurity.FilterFlag.NoMarkup);
            string email = portalSecurity.InputFilter(user.Email,
                                                      PortalSecurity.FilterFlag.NoScripting |
                                                      PortalSecurity.FilterFlag.NoAngleBrackets |
                                                      PortalSecurity.FilterFlag.NoMarkup);
            MembershipCreateStatus status;
            if (MembershipProviderConfig.RequiresQuestionAndAnswer)
            {
                System.Web.Security.Membership.CreateUser(userName,
                                                          user.Membership.Password,
                                                          email,
                                                          user.Membership.PasswordQuestion,
                                                          user.Membership.PasswordAnswer,
                                                          true,
                                                          out status);
            }
            else
            {
                System.Web.Security.Membership.CreateUser(userName,
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
            //Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);
                    //add to collection
                    arrUsers.Add(user);
                }
                //Get the next result (containing the total)
                dr.NextResult();

                //Get the total no of records from the second result
                totalRecords = Globals.GetTotalRecords(ref dr);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
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
                    //fill business object
                    UserInfo user = FillUserAndProfile(portalId, dr);
                    //add to collection
                    users.Add(user);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return users;
        }

        private static UserInfo FillUserAndProfile(int portalId, IDataReader dr)
        {
            UserInfo user = null;
            bool bContinue = (String.Equals(dr.GetName(0), "UserID", StringComparison.InvariantCultureIgnoreCase));

            //Ensure the data reader returned is valid
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
                        AffiliateID = Null.SetNullInteger(dr["AffiliateID"])
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
                            //Probably a profile property
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
                //read datareader
                bool bContinue = true;
                if (closeDataReader)
                {
                    bContinue = false;
                    if (dr.Read())
                    {
                        //Ensure the data reader returned is valid
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
                            LastIPAddress = Null.SetNullString(dr["LastIPAddress"])
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
                    }

                    user.AffiliateID = Null.SetNullInteger(Null.SetNull(dr["AffiliateID"], user.AffiliateID));
                    user.Username = Null.SetNullString(dr["Username"]);
                    UserController.GetUserMembership(user);
                    user.Email = Null.SetNullString(dr["Email"]);
                    user.Membership.UpdatePassword = Null.SetNullBoolean(dr["UpdatePassword"]);

                    if (schema != null)
                    {
                        if (schema.Select("ColumnName = 'PasswordResetExpiration'").Length > 0)
                        {
                            user.PasswordResetExpiration = Null.SetNullDateTime(dr["PasswordResetExpiration"]);
                        }
                        if (schema.Select("ColumnName = 'PasswordResetToken'").Length > 0)
                        {
                            user.PasswordResetToken = Null.SetNullGuid(dr["PasswordResetToken"]);
                        }
                    }
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
            //Fill Membership Property
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
                    //For superusers the Approved info is stored in aspnet membership
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
                    new CacheItemArgs(GetCacheKey(userName), DataCache.UserCacheTimeOut, DataCache.UserCachePriority,
                                      userName), GetMembershipUserCallBack);
        }

        private static string GetCacheKey(string userName)
        {
            return String.Format("MembershipUser_{0}", userName);
        }

        private static object GetMembershipUserCallBack(CacheItemArgs cacheItemArgs)
        {
            string userName = cacheItemArgs.ParamList[0].ToString();

            return System.Web.Security.Membership.GetUser(userName);
        }

       
        public override UserInfo GetUserByAuthToken(int portalId, string userToken, string authType)
        {
            IDataReader dr = _dataProvider.GetUserByAuthToken(portalId, userToken, authType);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
            return objUserInfo;
        }

        private static void UpdateUserMembership(UserInfo user)
        {
            var portalSecurity = PortalSecurity.Instance;
            string email = portalSecurity.InputFilter(user.Email,
                                                      PortalSecurity.FilterFlag.NoScripting |
                                                      PortalSecurity.FilterFlag.NoAngleBrackets |
                                                      PortalSecurity.FilterFlag.NoMarkup);

            //Persist the Membership Properties to the AspNet Data Store
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
				throw new Exception(Localization.GetExceptionMessage("UpdateUserMembershipFailed", "Asp.net membership update user failed."), ex);
	        }
            
            DataCache.RemoveCache(GetCacheKey(user.Username));
        }

        private static UserLoginStatus ValidateLogin(string username, string authType, UserInfo user,
                                                     UserLoginStatus loginStatus, string password, ref bool bValid,
                                                     int portalId)
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

        #endregion

        #region Public Methods
        /// <summary>
        /// add new userportal record (used for creating sites with existing user)
        /// </summary>
        /// <param name="portalId">portalid</param>
        /// <param name="userId">userid</param>
        public override void AddUserPortal(int portalId, int userId)
        {
            Requires.NotNullOrEmpty("portalId", portalId.ToString());
            Requires.NotNullOrEmpty("userId", userId.ToString());
            _dataProvider.AddUserPortal(portalId, userId);
        }

        /// <summary>
        /// function supports the ability change username
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="newUsername">updated username</param>
        public override void ChangeUsername(int userId, string newUsername)
        {
            Requires.NotNull("userId", userId);
            Requires.NotNullOrEmpty("newUsername", newUsername);

            _dataProvider.ChangeUsername(userId, newUsername);
            EventLogController.Instance.AddLog("userId",
                               userId.ToString(),
                               PortalController.Instance.GetCurrentPortalSettings(),
                               UserController.Instance.GetCurrentUserInfo().UserID,
                               EventLogController.EventLogType.USERNAME_UPDATED);
            DataCache.ClearCache();          
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
            if (retValue && PasswordRetrievalEnabled && !RequiresQuestionAndAnswer)
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
        public override bool ChangePasswordQuestionAndAnswer(UserInfo user, string password, string passwordQuestion,
                                                             string passwordAnswer)
        {
            MembershipUser aspnetUser = GetMembershipUser(user);
            if (password == Null.NullString)
            {
                password = aspnetUser.GetPassword();
            }
            return aspnetUser.ChangePasswordQuestionAndAnswer(password, passwordQuestion, passwordAnswer);
        }

        private UserCreateStatus ValidateForProfanity(UserInfo user)
        {
            var portalSecurity = PortalSecurity.Instance;
            var createStatus = UserCreateStatus.AddUser;

            Hashtable settings = UserController.GetUserSettings(user.PortalID);
            bool useProfanityFilter = Convert.ToBoolean(settings["Registration_UseProfanityFilter"]);

            //Validate Profanity
            if (useProfanityFilter)
            {
                if (!portalSecurity.ValidateInput(user.Username, PortalSecurity.FilterFlag.NoProfanity))
                {
                    createStatus = UserCreateStatus.InvalidUserName;
                }
                if (!String.IsNullOrEmpty(user.DisplayName))
                {
                    if (!portalSecurity.ValidateInput(user.DisplayName, PortalSecurity.FilterFlag.NoProfanity))
                    {
                        createStatus = UserCreateStatus.InvalidDisplayName;
                    }
                }
            }
            return createStatus;
        }

        private void ValidateForDuplicateDisplayName(UserInfo user, ref UserCreateStatus createStatus)
        {
            Hashtable settings = UserController.GetUserSettings(user.PortalID);
            bool requireUniqueDisplayName = Convert.ToBoolean(settings["Registration_RequireUniqueDisplayName"]);

            if (requireUniqueDisplayName)
            {
                UserInfo duplicateUser = GetUserByDisplayName(Null.NullInteger, user.DisplayName);
                if (duplicateUser != null)
                {
                    createStatus = UserCreateStatus.DuplicateDisplayName;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateUser persists a User to the Data Store
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to persist to the Data Store.</param>
        /// <returns>A UserCreateStatus enumeration indicating success or reason for failure.</returns>
        /// -----------------------------------------------------------------------------
        public override UserCreateStatus CreateUser(ref UserInfo user)
        {
            UserCreateStatus createStatus = ValidateForProfanity(user);
            string service = HttpContext.Current != null ? HttpContext.Current.Request.Params["state"] : string.Empty;

            if (createStatus == UserCreateStatus.AddUser)
            {
                ValidateForDuplicateDisplayName(user, ref createStatus);
            }

            if (createStatus == UserCreateStatus.AddUser)
            {
                try
                {
                    //check if username exists in database for any portal
                    UserInfo objVerifyUser = GetUserByUserName(Null.NullInteger, user.Username);
                    if (objVerifyUser != null)
                    {
                        //DNN-4016
                        //the username exists so we should now verify the password, DNN-4016 or check for oauth user authentication.
                        if (ValidateUser(user.Username, user.Membership.Password))
                        {
                            //check if user exists for the portal specified
                            objVerifyUser = GetUserByUserName(user.PortalID, user.Username);
                            if (objVerifyUser != null)
                            {
                                if (objVerifyUser.PortalID == user.PortalID && (!user.IsSuperUser || user.PortalID == Null.NullInteger))
                                {
                                    createStatus = UserCreateStatus.UserAlreadyRegistered;
                                }
                                else
                                {
                                    //SuperUser who is not part of portal
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
                            //not the same person - prevent registration
                            createStatus = UserCreateStatus.UsernameAlreadyExists;
                        }
                    }
                    else
                    {
                        //the user does not exist
                        createStatus = UserCreateStatus.AddUser;
                    }

                    //If new user - add to aspnet membership
                    if (createStatus == UserCreateStatus.AddUser)
                    {
                        createStatus = CreateMemberhipUser(user);
                    }

                    //If asp user has been successfully created or we are adding a existing user
                    //to a new portal 
                    if (createStatus == UserCreateStatus.Success || createStatus == UserCreateStatus.AddUserToPortal)
                    {
                        //Create the DNN User Record
                        createStatus = CreateDNNUser(ref user);
                        if (createStatus == UserCreateStatus.Success)
                        {
                            //Persist the Profile to the Data Store
                            ProfileController.UpdateUserProfile(user);
                        }
                    }
                }
                catch (Exception exc) //an unexpected error occurred
                {
                    Exceptions.LogException(exc);
                    createStatus = UserCreateStatus.UnexpectedError;
                }
            }

            return createStatus;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteUser deletes a single User from the Data Store
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to delete from the Data Store.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public override bool DeleteUser(UserInfo user)
        {
            bool retValue = true;
            try
            {
                _dataProvider.DeleteUserFromPortal(user.UserID, user.PortalID);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                retValue = false;
            }
            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes all UserOnline inof from the database that has activity outside of the
        /// time window
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="timeWindow">Time Window in Minutes</param>
        /// -----------------------------------------------------------------------------
        public override void DeleteUsersOnline(int timeWindow)
        {
            _dataProvider.DeleteUsersOnline(timeWindow);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates a new random password (Length = Minimum Length + 4)
        /// </summary>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public override string GeneratePassword()
        {
            return GeneratePassword(MinPasswordLength + 4);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generates a new random password
        /// </summary>
        /// <param name="length">The length of password to generate.</param>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public override string GeneratePassword(int length)
        {
            return System.Web.Security.Membership.GeneratePassword(length, MinNonAlphanumericCharacters);
        }

        public override ArrayList GetDeletedUsers(int portalId)
        {
            return FillUserCollection(portalId, _dataProvider.GetDeletedUsers(portalId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a collection of Online Users
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <returns>An ArrayList of UserInfo objects</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetOnlineUsers(int portalId)
        {
            int totalRecords = 0;
            return FillUserCollection(portalId, _dataProvider.GetOnlineUsers(portalId), ref totalRecords);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Current Password Information for the User 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to delete from the Data Store.</param>
        /// <param name="passwordAnswer">The answer to the Password Question, ues to confirm the user
        /// has the right to obtain the password.</param>
        /// <returns>A String</returns>
        /// -----------------------------------------------------------------------------
        public override string GetPassword(UserInfo user, string passwordAnswer)
        {
            MembershipUser aspnetUser = GetMembershipUser(user);
            if (aspnetUser.IsLockedOut)
            {
                AutoUnlockUser(aspnetUser);
            }
            return RequiresQuestionAndAnswer ? aspnetUser.GetPassword(passwordAnswer) : aspnetUser.GetPassword();
        }

        public override ArrayList GetUnAuthorizedUsers(int portalId)
        {
            return GetUnAuthorizedUsers(portalId, false, false);
        }

        public override ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
            return FillUserCollection(portalId,
                                      _dataProvider.GetUnAuthorizedUsers(portalId, includeDeleted, superUsersOnly));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByUserName retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userId">The id of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public override UserInfo GetUser(int portalId, int userId)
        {
            IDataReader dr = _dataProvider.GetUser(portalId, userId);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
            return objUserInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByDisplayName retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="displayName">The displayName of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public override UserInfo GetUserByDisplayName(int portalId, string displayName)
        {
            IDataReader dr = _dataProvider.GetUserByDisplayName(portalId, displayName);
            UserInfo objUserInfo = FillUserInfo(portalId, dr, true);
            return objUserInfo;
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
        public override UserInfo GetUserByUserName(int portalId, string username)
        {
            var objUserInfo = CBO.GetCachedObject<UserInfo>(
                new CacheItemArgs(string.Format(DataCache.UserCacheKey, portalId, username),
                    DataCache.UserCacheTimeOut, DataCache.UserCachePriority),
                _ =>
                {
                    using (var dr = _dataProvider.GetUserByUsername(portalId, username))
                    {
                        return FillUserInfo(portalId, dr, true);
                    }
                });
            return objUserInfo;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByVanityUrl retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="vanityUrl">The vanityUrl of the user being retrieved from the Data Store.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public override UserInfo GetUserByVanityUrl(int portalId, string vanityUrl)
        {
            UserInfo user = null;
            if (!String.IsNullOrEmpty(vanityUrl))
            {
                IDataReader dr = _dataProvider.GetUserByVanityUrl(portalId, vanityUrl);
                user = FillUserInfo(portalId, dr, true);
            }
            return user;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserByPasswordResetToken retrieves a User from the DataStore
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="resetToken">The password reset token.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public override UserInfo GetUserByPasswordResetToken(int portalId, string resetToken)
        {
            UserInfo user = null;
            if (!String.IsNullOrEmpty(resetToken))
            {
                IDataReader dr = _dataProvider.GetUserByPasswordResetToken(portalId, resetToken);
                user = FillUserInfo(portalId, dr, true);
            }
            return user;
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
        public override int GetUserCountByPortal(int portalId)
        {
            return _dataProvider.GetUserCountByPortal(portalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserMembership retrieves the UserMembership information from the Data Store
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user whose Membership information we are retrieving.</param>
        /// -----------------------------------------------------------------------------
        public override void GetUserMembership(ref UserInfo user)
        {
            //Get AspNet MembershipUser
            MembershipUser aspnetUser = GetMembershipUser(user);

            //Fill Membership Property
            FillUserMembership(aspnetUser, user);

            //Get Online Status
            user.Membership.IsOnLine = IsUserOnline(user);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal
        /// </summary>
        /// <remarks>If all records are required, (ie no paging) set pageSize = -1</remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsers(portalId, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsers gets all the users of the portal
        /// </summary>
        /// <remarks>If all records are required, (ie no paging) set pageSize = -1</remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include deleted users.</param>
        /// <param name="superUsersOnly">Only select super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords,
                                           bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(portalId, _dataProvider.GetAllUsers(portalId, pageIndex, pageSize, includeDeleted,
                                                                superUsersOnly), ref totalRecords);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="userId"></param>
        /// <param name="filterUserId"></param>
        /// <param name="filterRoleId"></param>
        /// <param name="relationshipTypeId"> </param>
        /// <param name="isAdmin"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortAscending"></param>
        /// <param name="propertyNames"></param>
        /// <param name="propertyValues"></param>
        /// <returns></returns>
        public override IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId,
                                                               int filterRoleId, int relationshipTypeId,
                                                               bool isAdmin, int pageIndex, int pageSize,
                                                               string sortColumn,
                                                               bool sortAscending, string propertyNames,
                                                               string propertyValues)
        {
            return FillUserList(portalId,
                                _dataProvider.GetUsersAdvancedSearch(portalId, userId, filterUserId, filterRoleId,
                                                                     relationshipTypeId, isAdmin, pageIndex, pageSize,
                                                                     sortColumn, sortAscending, propertyNames,
                                                                     propertyValues));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortAscending"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        public override IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn,
                                                            bool sortAscending, string propertyName,
                                                            string propertyValue)
        {
            return FillUserList(portalId, _dataProvider.GetUsersBasicSearch(portalId, pageIndex, pageSize,
                                                                            sortColumn, sortAscending, propertyName,
                                                                            propertyValue));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByEmail gets all the users of the portal whose email matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>If all records are required, (ie no paging) set pageSize = -1</remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="emailToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize,
                                                  ref int totalRecords)
        {
            return GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByEmail gets all the users of the portal whose email matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>If all records are required, (ie no paging) set pageSize = -1</remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="emailToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include deleted users.</param>
        /// <param name="superUsersOnly">Only select super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize,
                                                  ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(portalId,
                                      _dataProvider.GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize,
                                                                    includeDeleted, superUsersOnly), ref totalRecords);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByUserName gets all the users of the portal whose username matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>If all records are required, (ie no paging) set pageSize = -1</remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userNameToMatch">The username to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize,
                                                     ref int totalRecords)
        {
            return GetUsersByUserName(portalId, userNameToMatch, pageIndex, pageSize, ref totalRecords, false, false);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUsersByUserName gets all the users of the portal whose username matches a provided
        /// filter expression
        /// </summary>
        /// <remarks>If all records are required, (ie no paging) set pageSize = -1</remarks>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userNameToMatch">The username to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <param name="includeDeleted">Include deleted users.</param>
        /// <param name="superUsersOnly">Only select super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize,
                                                     ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(portalId,
                                      _dataProvider.GetUsersByUsername(portalId, userNameToMatch, pageIndex, pageSize,
                                                                       includeDeleted, superUsersOnly), ref totalRecords);
        }

		/// -----------------------------------------------------------------------------
		/// <summary>
		/// GetUsersByDisplayName gets all the users of the portal whose display name matches a provided
		/// filter expression
		/// </summary>
		/// <remarks>If all records are required, (ie no paging) set pageSize = -1</remarks>
		/// <param name="portalId">The Id of the Portal</param>
		/// <param name="nameToMatch">The display name to use to find a match.</param>
		/// <param name="pageIndex">The page of records to return.</param>
		/// <param name="pageSize">The size of the page</param>
		/// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
		/// <param name="includeDeleted">Include deleted users.</param>
		/// <param name="superUsersOnly">Only select super users.</param>
		/// <returns>An ArrayList of UserInfo objects.</returns>
		/// -----------------------------------------------------------------------------
		public override ArrayList GetUsersByDisplayName(int portalId, string nameToMatch, int pageIndex, int pageSize,
													 ref int totalRecords, bool includeDeleted, bool superUsersOnly)
		{
			if (pageIndex == -1)
			{
				pageIndex = 0;
				pageSize = int.MaxValue;
			}

			return FillUserCollection(portalId,
									  _dataProvider.GetUsersByDisplayname(portalId, nameToMatch, pageIndex, pageSize,
																	   includeDeleted, superUsersOnly), ref totalRecords);
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
        public override ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue,
                                                            int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByProfileProperty(portalId, propertyName, propertyValue, pageIndex, pageSize,
                                             ref totalRecords, false, false);
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
        /// <param name="includeDeleted">Include deleted users.</param>
        /// <param name="superUsersOnly">Only select super users.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue,
                                                            int pageIndex, int pageSize, ref int totalRecords,
                                                            bool includeDeleted, bool superUsersOnly)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            return FillUserCollection(portalId,
                                      _dataProvider.GetUsersByProfileProperty(portalId, propertyName, propertyValue,
                                                                              pageIndex, pageSize, includeDeleted,
                                                                              superUsersOnly), ref totalRecords);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the user in question is online
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user.</param>
        /// <returns>A Boolean indicating whether the user is online.</returns>
        /// -----------------------------------------------------------------------------
        public override bool IsUserOnline(UserInfo user)
        {
            bool isOnline = false;
            var objUsersOnline = new UserOnlineController();
            if (objUsersOnline.IsEnabled())
            {
                Hashtable userList = objUsersOnline.GetUserList();
                var onlineUser = (OnlineUserInfo) userList[user.UserID.ToString()];
                if (onlineUser != null)
                {
                    isOnline = true;
                }
                else
                {
                    //Next try the Database
                    onlineUser = CBO.FillObject<OnlineUserInfo>(_dataProvider.GetOnlineUser(user.UserID));
                    if (onlineUser != null)
                    {
                        isOnline = true;
                    }
                }
            }
            return isOnline;
        }

        public override bool RemoveUser(UserInfo user)
        {
            bool retValue = true;

            try
            {
                foreach (var relationship in user.Social.UserRelationships)
                {
                    RelationshipController.Instance.DeleteUserRelationship(relationship);
                }

                _dataProvider.RemoveUser(user.UserID, user.PortalID);

                //Prior to removing membership, ensure user is not present in any other portal
                UserInfo otherUser = GetUserByUserName(Null.NullInteger, user.Username);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ResetPassword resets a user's password and returns the newly created password
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to update.</param>
        /// <param name="passwordAnswer">The answer to the user's password Question.</param>
        /// <returns>The new Password.</returns>
        /// -----------------------------------------------------------------------------
        public override string ResetPassword(UserInfo user, string passwordAnswer)
        {
            //Get AspNet MembershipUser
            MembershipUser aspnetUser = GetMembershipUser(user);

            return RequiresQuestionAndAnswer ? aspnetUser.ResetPassword(passwordAnswer) : aspnetUser.ResetPassword();
        }

        /// <summary>
        /// function sets user specific password reset token and timeout
        /// works for all PasswordFormats as it resets and then changes the password
        /// so old password is not required
        /// method does not support RequiresQuestionAndAnswer
        /// </summary>
        /// <param name="user"></param>
        public override bool ResetAndChangePassword(UserInfo user,string newPassword)
        {
	        return ResetAndChangePassword(user, newPassword, string.Empty);
        }

		public override bool ResetAndChangePassword(UserInfo user, string newPassword, string answer)
		{
			if (RequiresQuestionAndAnswer && string.IsNullOrEmpty(answer))
			{
				return false;
			}

			//Get AspNet MembershipUser
			MembershipUser aspnetUser = GetMembershipUser(user);

			string resetPassword = ResetPassword(user, answer);
			return aspnetUser.ChangePassword(resetPassword, newPassword);
		}

        public override bool RestoreUser(UserInfo user)
        {
            bool retValue = true;

            try
            {
                _dataProvider.RestoreUser(user.UserID, user.PortalID);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                retValue = false;
            }

            return retValue;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Unlocks the User's Account
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user whose account is being Unlocked.</param>
        /// <returns>True if successful, False if unsuccessful.</returns>
        /// -----------------------------------------------------------------------------
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateUser persists a user to the Data Store
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="user">The user to persist to the Data Store.</param>
        /// -----------------------------------------------------------------------------
        public override void UpdateUser(UserInfo user)
        {
            var objSecurity = PortalSecurity.Instance;
            string firstName = objSecurity.InputFilter(user.FirstName,
                                                       PortalSecurity.FilterFlag.NoScripting |
                                                       PortalSecurity.FilterFlag.NoAngleBrackets |
                                                       PortalSecurity.FilterFlag.NoMarkup);
            string lastName = objSecurity.InputFilter(user.LastName,
                                                      PortalSecurity.FilterFlag.NoScripting |
                                                      PortalSecurity.FilterFlag.NoAngleBrackets |
                                                      PortalSecurity.FilterFlag.NoMarkup);
            string email = objSecurity.InputFilter(user.Email,
                                                   PortalSecurity.FilterFlag.NoScripting |
                                                   PortalSecurity.FilterFlag.NoAngleBrackets |
                                                   PortalSecurity.FilterFlag.NoMarkup);
            string displayName = objSecurity.InputFilter(user.DisplayName,
                                                         PortalSecurity.FilterFlag.NoScripting |
                                                         PortalSecurity.FilterFlag.NoAngleBrackets |
                                                         PortalSecurity.FilterFlag.NoMarkup);
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
            if (String.IsNullOrEmpty(displayName))
            {
                displayName = firstName + " " + lastName;
            }

            //Persist the Membership to the Data Store
            UpdateUserMembership(user);

            //Persist the DNN User to the Database
            _dataProvider.UpdateUser(user.UserID,
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

            //Persist the Profile to the Data Store
            ProfileController.UpdateUserProfile(user);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates UserOnline info
        /// time window
        /// </summary>
        /// <param name="userList">List of users to update</param>
        /// -----------------------------------------------------------------------------
        public override void UpdateUsersOnline(Hashtable userList)
        {
            _dataProvider.UpdateUsersOnline(userList);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserLogin attempts to log the user in, and returns the User if successful
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="username">The user name of the User attempting to log in</param>
        /// <param name="password">The password of the User attempting to log in</param>
        /// <param name="verificationCode">The verification code of the User attempting to log in</param>
        /// <param name="loginStatus">An enumerated value indicating the login status.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public override UserInfo UserLogin(int portalId, string username, string password, string verificationCode,
                                           ref UserLoginStatus loginStatus)
        {
            return UserLogin(portalId, username, password, "DNN", verificationCode, ref loginStatus);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UserLogin attempts to log the user in, and returns the User if successful
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">The Id of the Portal the user belongs to</param>
        /// <param name="username">The user name of the User attempting to log in</param>
        /// <param name="password">The password of the User attempting to log in (may not be used by all Auth types)</param>
        /// <param name="authType">The type of Authentication Used</param>
        /// <param name="verificationCode">The verification code of the User attempting to log in</param>
        /// <param name="loginStatus">An enumerated value indicating the login status.</param>
        /// <returns>The User as a UserInfo object</returns>
        /// -----------------------------------------------------------------------------
        public override UserInfo UserLogin(int portalId, string username, string password, string authType,
                                           string verificationCode, ref UserLoginStatus loginStatus)
        {
            //For now, we are going to ignore the possibility that the User may exist in the 
            //Global Data Store but not in the Local DataStore ie. A shared Global Data Store

            //Initialise Login Status to Failure
            loginStatus = UserLoginStatus.LOGIN_FAILURE;

            DataCache.ClearUserCache(portalId, username);
            DataCache.ClearCache(GetCacheKey(username));

            //Get a light-weight (unhydrated) DNN User from the Database, we will hydrate it later if neccessary
            UserInfo user = (authType == "DNN")
                                ? GetUserByUserName(portalId, username)
                                : GetUserByAuthToken(portalId, verificationCode, authType);
            if (user != null && !user.IsDeleted)
            {
                //Get AspNet MembershipUser
                MembershipUser aspnetUser = GetMembershipUser(user);

                //Fill Membership Property from AspNet MembershipUser
                FillUserMembership(aspnetUser, user);

                //Check if the User is Locked Out (and unlock if AutoUnlock has expired)
                if (aspnetUser.IsLockedOut)
                {
                    if (AutoUnlockUser(aspnetUser))
                    {
                        //Unlock User
                        user.Membership.LockedOut = false;
                    }
                    else
                    {
                        loginStatus = UserLoginStatus.LOGIN_USERLOCKEDOUT;
                    }
                }

                //Check in a verified situation whether the user is Approved
                if (user.Membership.Approved == false && user.IsSuperUser == false)
                {
                    
                    //Check Verification code (skip for FB, Google, Twitter, LiveID as it has no verification code)
                        if (_socialAuthProviders.Contains(authType) && String.IsNullOrEmpty(verificationCode))
                    {
                        if (PortalController.Instance.GetCurrentPortalSettings().UserRegistration ==
                            (int) Globals.PortalRegistrationType.PublicRegistration)
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

                //Verify User Credentials
                bool bValid = false;
                loginStatus = ValidateLogin(username, authType, user, loginStatus, password, ref bValid, portalId);
                if (!bValid)
                {
                    //Clear the user object
                    user = null;
                }
            }
            else
            {
                //Clear the user object
                user = null;
            }
            return user;
        }

        #endregion

        public static ArrayList FillUserCollection(int portalId, IDataReader dr)
        {
            //Note:  the DataReader returned from this method should contain 2 result sets.  The first set
            //       contains the TotalRecords, that satisfy the filter, the second contains the page
            //       of data
            var arrUsers = new ArrayList();
            try
            {
                while (dr.Read())
                {
                    //fill business object
                    UserInfo user = FillUserInfo(portalId, dr, false);
                    //add to collection
                    arrUsers.Add(user);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                //close datareader
                CBO.CloseDataReader(dr, true);
            }
            return arrUsers;
        }
    }
}