// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Users;

    public abstract class MembershipProvider
    {
        public abstract bool CanEditProviderProperties { get; }

        public abstract int MaxInvalidPasswordAttempts { get; set; }

        public abstract int MinPasswordLength { get; set; }

        public abstract int MinNonAlphanumericCharacters { get; set; }

        public abstract int PasswordAttemptWindow { get; set; }

        public abstract PasswordFormat PasswordFormat { get; set; }

        public abstract bool PasswordResetEnabled { get; set; }

        public abstract bool PasswordRetrievalEnabled { get; set; }

        public abstract string PasswordStrengthRegularExpression { get; set; }

        public abstract bool RequiresQuestionAndAnswer { get; set; }

        public abstract bool RequiresUniqueEmail { get; set; }

        // return the provider
        public static MembershipProvider Instance()
        {
            return ComponentFactory.GetComponent<MembershipProvider>();
        }

        // Users
        public abstract bool ChangePassword(UserInfo user, string oldPassword, string newPassword);

        public abstract bool ChangePasswordQuestionAndAnswer(UserInfo user, string password, string passwordQuestion, string passwordAnswer);

        public abstract UserCreateStatus CreateUser(ref UserInfo user);

        public abstract bool DeleteUser(UserInfo user);

        public abstract bool RestoreUser(UserInfo user);

        public abstract bool RemoveUser(UserInfo user);

        public abstract string GeneratePassword();

        public abstract string GeneratePassword(int length);

        public abstract string GetPassword(UserInfo user, string passwordAnswer);

        public abstract int GetUserCountByPortal(int portalId);

        public abstract void GetUserMembership(ref UserInfo user);

        public abstract string ResetPassword(UserInfo user, string passwordAnswer);

        public abstract bool UnLockUser(UserInfo user);

        public abstract void UserAgreedToTerms(UserInfo user);

        public abstract void ResetTermsAgreement(int portalId);

        public abstract void UserRequestsRemoval(UserInfo user, bool remove);

        public abstract void UpdateUser(UserInfo user);

        public abstract UserInfo UserLogin(int portalId, string username, string password, string verificationCode, ref UserLoginStatus loginStatus);

        public abstract UserInfo UserLogin(int portalId, string username, string password, string authType, string verificationCode, ref UserLoginStatus loginStatus);

        // Users Online
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public abstract void DeleteUsersOnline(int TimeWindow);

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public abstract ArrayList GetOnlineUsers(int PortalId);

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public abstract bool IsUserOnline(UserInfo user);

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public abstract void UpdateUsersOnline(Hashtable UserList);

        // Legacy
        public virtual void TransferUsersToMembershipProvider()
        {
        }

        // Get Users
        public abstract UserInfo GetUser(int portalId, int userId);

        public abstract UserInfo GetUserByUserName(int portalId, string username);

        public abstract ArrayList GetUnAuthorizedUsers(int portalId);

        public abstract ArrayList GetDeletedUsers(int portalId);

        public abstract ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords);

        public abstract ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords);

        public abstract ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords);

        public abstract ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords);

        public virtual UserInfo GetUserByDisplayName(int portalId, string displayName)
        {
            return null;
        }

        public virtual UserInfo GetUserByVanityUrl(int portalId, string vanityUrl)
        {
            return null;
        }

        public virtual UserInfo GetUserByPasswordResetToken(int portalId, string resetToken)
        {
            return null;
        }

        public virtual UserInfo GetUserByAuthToken(int portalId, string userToken, string authType)
        {
            throw new NotImplementedException();
        }

        public virtual string GetProviderUserKey(UserInfo user)
        {
            return null;
        }

        public virtual UserInfo GetUserByProviderUserKey(int portalId, string providerUserKey)
        {
            return null;
        }

        public virtual ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            throw new NotImplementedException();
        }

        public virtual ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            throw new NotImplementedException();
        }

        public virtual ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            throw new NotImplementedException();
        }

        public virtual ArrayList GetUsersByDisplayName(int portalId, string nameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            throw new NotImplementedException();
        }

        public virtual ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
        {
            throw new NotImplementedException();
        }

        public virtual ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
            throw new NotImplementedException();
        }

        public virtual IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationTypeId,
                                                    bool isAdmin, int pageIndex, int pageSize, string sortColumn,
                                                    bool sortAscending, string propertyNames, string propertyValues)
        {
            throw new NotImplementedException();
        }

        public virtual IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn,
                                            bool sortAscending, string propertyName, string propertyValue)
        {
            throw new NotImplementedException();
        }

        public virtual bool ResetAndChangePassword(UserInfo user, string newPassword)
        {
            throw new NotImplementedException();
        }

        public virtual bool ResetAndChangePassword(UserInfo user, string newPassword, string answer)
        {
            throw new NotImplementedException();
        }

        public virtual void ChangeUsername(int userId, string newUsername)
        {
            throw new NotImplementedException();
        }

        public virtual void AddUserPortal(int portalId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
