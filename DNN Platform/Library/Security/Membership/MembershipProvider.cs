// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Membership;

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

    /// <summary>ChangePassword attempts to change the user's password.</summary>
    /// <param name="user">The user to update.</param>
    /// <param name="oldPassword">The old password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>A Boolean indicating success or failure.</returns>
    public abstract bool ChangePassword(UserInfo user, string oldPassword, string newPassword);

    /// <summary>ChangePasswordQuestionAndAnswer attempts to change the users password Question and PasswordAnswer.</summary>
    /// <param name="user">The user to update.</param>
    /// <param name="password">The password.</param>
    /// <param name="passwordQuestion">The new password question.</param>
    /// <param name="passwordAnswer">The new password answer.</param>
    /// <returns>A Boolean indicating success or failure.</returns>
    public abstract bool ChangePasswordQuestionAndAnswer(UserInfo user, string password, string passwordQuestion, string passwordAnswer);

    /// <summary>CreateUser persists a User to the Data Store.</summary>
    /// <param name="user">The user to persist to the Data Store.</param>
    /// <returns>A UserCreateStatus enumeration indicating success or reason for failure.</returns>
    public abstract UserCreateStatus CreateUser(ref UserInfo user);

    /// <summary>DeleteUser deletes a single User from the Data Store.</summary>
    /// <param name="user">The user to delete from the Data Store.</param>
    /// <returns>A Boolean indicating success or failure.</returns>
    public abstract bool DeleteUser(UserInfo user);

    public abstract bool RestoreUser(UserInfo user);

    public abstract bool RemoveUser(UserInfo user);

    /// <summary>Generates a new random password (Length = Minimum Length + 4).</summary>
    /// <returns>A String.</returns>
    public abstract string GeneratePassword();

    /// <summary>Generates a new random password.</summary>
    /// <param name="length">The length of password to generate.</param>
    /// <returns>A String.</returns>
    public abstract string GeneratePassword(int length);

    /// <summary>Gets the Current Password Information for the User. Throws an exception if password retrieval is not supported.</summary>
    /// <param name="user">The user for which to get the password.</param>
    /// <param name="passwordAnswer">The answer to the Password Question, used to confirm the user has the right to obtain the password.</param>
    /// <returns>A String.</returns>
    public abstract string GetPassword(UserInfo user, string passwordAnswer);

    /// <summary>GetUserCountByPortal gets the number of users in the portal.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <returns>The number of users.</returns>
    public abstract int GetUserCountByPortal(int portalId);

    /// <summary>GetUserMembership retrieves the UserMembership information from the Data Store.</summary>
    /// <param name="user">The user whose Membership information we are retrieving.</param>
    public abstract void GetUserMembership(ref UserInfo user);

    /// <summary>ResetPassword resets a user's password and returns the newly created password.</summary>
    /// <param name="user">The user to update.</param>
    /// <param name="passwordAnswer">The answer to the user's password Question.</param>
    /// <returns>The new Password.</returns>
    public abstract string ResetPassword(UserInfo user, string passwordAnswer);

    /// <summary>Unlocks the User's Account.</summary>
    /// <param name="user">The user whose account is being Unlocked.</param>
    /// <returns>True if successful, False if unsuccessful.</returns>
    public abstract bool UnLockUser(UserInfo user);

    /// <summary>User has agreed to terms and conditions for the portal.</summary>
    /// <param name="user">The agreeing user.</param>
    public abstract void UserAgreedToTerms(UserInfo user);

    /// <summary>Reset all agreements on portal so all users need to agree again at next login.</summary>
    /// <param name="portalId">Portal for which to reset agreements.</param>
    public abstract void ResetTermsAgreement(int portalId);

    /// <summary>Sets a boolean on the user portal to indicate this user has requested that their account be deleted.</summary>
    /// <param name="user">User requesting removal.</param>
    /// <param name="remove">True if user requests removal, false if the value needs to be reset.</param>
    public abstract void UserRequestsRemoval(UserInfo user, bool remove);

    /// <summary>UpdateUser persists a user to the Data Store.</summary>
    /// <param name="user">The user to persist to the Data Store.</param>
    public abstract void UpdateUser(UserInfo user);

    /// <summary>UserLogin attempts to log the user in, and returns the User if successful.</summary>
    /// <param name="portalId">The Id of the Portal the user belongs to.</param>
    /// <param name="username">The user name of the User attempting to log in.</param>
    /// <param name="password">The password of the User attempting to log in.</param>
    /// <param name="verificationCode">The verification code of the User attempting to log in.</param>
    /// <param name="loginStatus">An enumerated value indicating the login status.</param>
    /// <returns>The User as a UserInfo object.</returns>
    public abstract UserInfo UserLogin(int portalId, string username, string password, string verificationCode, ref UserLoginStatus loginStatus);

    /// <summary>UserLogin attempts to log the user in, and returns the User if successful.</summary>
    /// <param name="portalId">The Id of the Portal the user belongs to.</param>
    /// <param name="username">The user name of the User attempting to log in.</param>
    /// <param name="password">The password of the User attempting to log in (may not be used by all Auth types).</param>
    /// <param name="authType">The type of Authentication Used.</param>
    /// <param name="verificationCode">The verification code of the User attempting to log in.</param>
    /// <param name="loginStatus">An enumerated value indicating the login status.</param>
    /// <returns>The User as a UserInfo object.</returns>
    public abstract UserInfo UserLogin(int portalId, string username, string password, string authType, string verificationCode, ref UserLoginStatus loginStatus);

    // Users Online

    /// <summary>Deletes all UserOnline info from the database that has activity outside of the time window.</summary>
    /// <param name="timeWindow">Time Window in Minutes.</param>
    [Obsolete("Deprecated in DotNetNuke 8.0.0. Other solutions exist outside of the DNN Platform. Scheduled for removal in v11.0.0.")]
    public abstract void DeleteUsersOnline(int timeWindow);

    /// <summary>Gets a collection of Online Users.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    [Obsolete("Deprecated in DotNetNuke 8.0.0. Other solutions exist outside of the DNN Platform. Scheduled for removal in v11.0.0.")]
    public abstract ArrayList GetOnlineUsers(int portalId);

    /// <summary>Gets whether the user in question is online.</summary>
    /// <param name="user">The user.</param>
    /// <returns>A Boolean indicating whether the user is online.</returns>
    [Obsolete("Deprecated in DotNetNuke 8.0.0. Other solutions exist outside of the DNN Platform. Scheduled for removal in v11.0.0.")]
    public abstract bool IsUserOnline(UserInfo user);

    /// <summary>Updates UserOnline info time window.</summary>
    /// <param name="userList">List of users to update.</param>
    [Obsolete("Deprecated in DotNetNuke 8.0.0. Other solutions exist outside of the DNN Platform. Scheduled for removal in v11.0.0.")]
    public abstract void UpdateUsersOnline(Hashtable userList);

    // Legacy
    public virtual void TransferUsersToMembershipProvider()
    {
    }

    // Get Users

    /// <summary>GetUserByUserName retrieves a User from the DataStore.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="userId">The id of the user being retrieved from the Data Store.</param>
    /// <returns>The User as a UserInfo object.</returns>
    public abstract UserInfo GetUser(int portalId, int userId);

    /// <summary>GetUserByUserName retrieves a User from the DataStore. Supports user caching in memory cache.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="username">The username of the user being retrieved from the Data Store.</param>
    /// <returns>The User as a UserInfo object.</returns>
    public abstract UserInfo GetUserByUserName(int portalId, string username);

    public abstract ArrayList GetUnAuthorizedUsers(int portalId);

    public abstract ArrayList GetDeletedUsers(int portalId);

    /// <summary>GetUsers gets all the users of the portal.</summary>
    /// <remarks>If all records are required, (ie no paging) set pageSize = -1.</remarks>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public abstract ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords);

    /// <summary>GetUsersByEmail gets all the users of the portal whose email matches a provided filter expression.</summary>
    /// <remarks>If all records are required, (ie no paging) set pageSize = -1.</remarks>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="emailToMatch">The email address to use to find a match.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public abstract ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords);

    /// <summary>GetUsersByUserName gets all the users of the portal whose username matches a provided filter expression.</summary>
    /// <remarks>If all records are required, (ie no paging) set pageSize = -1.</remarks>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="userNameToMatch">The username to use to find a match.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public abstract ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords);

    /// <summary>GetUsersByProfileProperty gets all the users of the portal whose profile matches the profile property passed as a parameter.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="propertyName">The name of the property being matched.</param>
    /// <param name="propertyValue">The value of the property being matched.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public abstract ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords);

    /// <inheritdoc cref="IUserController.GetUserByDisplayname"/>
    public virtual UserInfo GetUserByDisplayName(int portalId, string displayName)
    {
        return null;
    }

    /// <summary>GetUserByVanityUrl retrieves a User from the DataStore.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="vanityUrl">The vanityUrl of the user being retrieved from the Data Store.</param>
    /// <returns>The User as a UserInfo object or <see langword="null"/>.</returns>
    public virtual UserInfo GetUserByVanityUrl(int portalId, string vanityUrl)
    {
        return null;
    }

    /// <summary>GetUserByPasswordResetToken retrieves a User from the DataStore.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="resetToken">The password reset token.</param>
    /// <returns>The User as a UserInfo object.</returns>
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

    /// <summary>GetUsers gets all the users of the portal.</summary>
    /// <remarks>If all records are required, (ie no paging) set pageSize = -1.</remarks>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <param name="includeDeleted">Include deleted users.</param>
    /// <param name="superUsersOnly">Only select super users.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public virtual ArrayList GetUsers(int portalId, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
    {
        throw new NotImplementedException();
    }

    /// <summary>GetUsersByEmail gets all the users of the portal whose email matches a provided filter expression.</summary>
    /// <remarks>If all records are required, (ie no paging) set pageSize = -1.</remarks>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="emailToMatch">The email address to use to find a match.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <param name="includeDeleted">Include deleted users.</param>
    /// <param name="superUsersOnly">Only select super users.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public virtual ArrayList GetUsersByEmail(int portalId, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
    {
        throw new NotImplementedException();
    }

    /// <summary>GetUsersByUserName gets all the users of the portal whose username matches a provided filter expression.</summary>
    /// <remarks>If all records are required, (ie no paging) set pageSize = -1.</remarks>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="userNameToMatch">The username to use to find a match.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <param name="includeDeleted">Include deleted users.</param>
    /// <param name="superUsersOnly">Only select super users.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public virtual ArrayList GetUsersByUserName(int portalId, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
    {
        throw new NotImplementedException();
    }

    /// <summary>GetUsersByDisplayName gets all the users of the portal whose display name matches a provided filter expression.</summary>
    /// <remarks>If all records are required, (ie no paging) set pageSize = -1.</remarks>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="nameToMatch">The display name to use to find a match.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <param name="includeDeleted">Include deleted users.</param>
    /// <param name="superUsersOnly">Only select super users.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public virtual ArrayList GetUsersByDisplayName(int portalId, string nameToMatch, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
    {
        throw new NotImplementedException();
    }

    /// <summary>GetUsersByProfileProperty gets all the users of the portal whose profile matches the profile property passed as a parameter.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="propertyName">The name of the property being matched.</param>
    /// <param name="propertyValue">The value of the property being matched.</param>
    /// <param name="pageIndex">The page of records to return.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
    /// <param name="includeDeleted">Include deleted users.</param>
    /// <param name="superUsersOnly">Only select super users.</param>
    /// <returns>An ArrayList of UserInfo objects.</returns>
    public virtual ArrayList GetUsersByProfileProperty(int portalId, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords, bool includeDeleted, bool superUsersOnly)
    {
        throw new NotImplementedException();
    }

    public virtual ArrayList GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="IUserController.GetUsersAdvancedSearch" />
    public virtual IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationTypeId, bool isAdmin, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyNames, string propertyValues)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="IUserController.GetUsersBasicSearch" />
    public virtual IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyName, string propertyValue)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// function sets user specific password reset token and timeout
    /// works for all PasswordFormats as it resets and then changes the password
    /// so old password is not required
    /// method does not support RequiresQuestionAndAnswer.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns><see langword="true"/> if the password successfully changed, otherwise <see langword="false"/>.</returns>
    public virtual bool ResetAndChangePassword(UserInfo user, string newPassword)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// function sets user specific password reset token and timeout
    /// works for all PasswordFormats as it resets and then changes the password
    /// so old password is not required.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns><see langword="true"/> if the password successfully changed, otherwise <see langword="false"/>.</returns>
    public virtual bool ResetAndChangePassword(UserInfo user, string newPassword, string answer)
    {
        throw new NotImplementedException();
    }

    /// <summary>function supports the ability change username.</summary>
    /// <param name="userId">User ID.</param>
    /// <param name="newUsername">updated username.</param>
    public virtual void ChangeUsername(int userId, string newUsername)
    {
        throw new NotImplementedException();
    }

    /// <summary>Add new User-Portal record (used for creating sites with existing user).</summary>
    /// <param name="portalId">Portal ID.</param>
    /// <param name="userId">User ID.</param>
    public virtual void AddUserPortal(int portalId, int userId)
    {
        throw new NotImplementedException();
    }
}
