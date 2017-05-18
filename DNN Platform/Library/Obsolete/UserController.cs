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

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;

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
    public partial class UserController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.CreateUser")]
        public int AddUser(UserInfo objUser)
        {
            CreateUser(ref objUser);
            return objUser.UserID;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.CreateUser")]
        public int AddUser(UserInfo objUser, bool addToMembershipProvider)
        {
            CreateUser(ref objUser);
            return objUser.UserID;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.2.2. This method has been replaced by UserController.MoveUserToPortal and UserControllar.CopyUserToPortal")]
        public static void CopyUserToPortal(UserInfo user, PortalInfo portal, bool mergeUser, bool deleteUser)
        {
            if (deleteUser)
            {
                MoveUserToPortal(user, portal, mergeUser);
            }
            else
            {
                CopyUserToPortal(user, portal, mergeUser);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.DeleteUsers")]
        public void DeleteAllUsers(int portalId)
        {
            DeleteUsers(portalId, false, true);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.DeleteUser")]
        public bool DeleteUser(int portalId, int userId)
        {
            var objUser = GetUser(portalId, userId);

            //Call Shared method with notify=true, deleteAdmin=false
            return DeleteUser(ref objUser, true, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.DeleteUsers")]
        public void DeleteUsers(int portalId)
        {
            DeleteUsers(portalId, true, false);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.1.")]
        public static ArrayList FillUserCollection(int portalId, IDataReader dr, ref int totalRecords)
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.1.")]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.1.")]
        public static UserInfo FillUserInfo(int portalId, IDataReader dr, bool closeDataReader)
        {
            UserInfo objUserInfo = null;
            try
            {
                //read datareader
                var bContinue = true;
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
                    objUserInfo = new UserInfo
                    {
                        PortalID = portalId,
                        IsSuperUser = Null.SetNullBoolean(dr["IsSuperUser"]),
                        IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]),
                        UserID = Null.SetNullInteger(dr["UserID"]),
                        FirstName = Null.SetNullString(dr["FirstName"]),
                        LastName = Null.SetNullString(dr["LastName"]),
                        RefreshRoles = Null.SetNullBoolean(dr["RefreshRoles"]),
                        DisplayName = Null.SetNullString(dr["DisplayName"])
                    };
                    objUserInfo.AffiliateID = Null.SetNullInteger(Null.SetNull(dr["AffiliateID"], objUserInfo.AffiliateID));
                    objUserInfo.Username = Null.SetNullString(dr["Username"]);
                    GetUserMembership(objUserInfo);
                    objUserInfo.Email = Null.SetNullString(dr["Email"]);
                    objUserInfo.Membership.UpdatePassword = Null.SetNullBoolean(dr["UpdatePassword"]);

					var schema = dr.GetSchemaTable();
					if (schema != null)
					{
						if (schema.Select("ColumnName = 'PasswordResetExpiration'").Length > 0)
						{
							objUserInfo.PasswordResetExpiration = Null.SetNullDateTime(dr["PasswordResetExpiration"]);
						}
						if (schema.Select("ColumnName = 'PasswordResetToken'").Length > 0)
						{
							objUserInfo.PasswordResetToken = Null.SetNullGuid(dr["PasswordResetToken"]);
						}
					}

	                if (!objUserInfo.IsSuperUser)
                    {
                        objUserInfo.Membership.Approved = Null.SetNullBoolean(dr["Authorised"]);
                    }
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, closeDataReader);
            }
            return objUserInfo;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public UserInfo FillUserInfo(int portalID, string username)
        {
            return GetCachedUser(portalID, username);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function should be replaced by String.Format(DataCache.UserCacheKey, portalId, username)")]
        public string GetCacheKey(int portalID, string username)
        {
            return string.Format(DataCache.UserCacheKey, portalID, username);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function should be replaced by String.Format(DataCache.UserCacheKey, portalId, username)")]
        public static string CacheKey(int portalId, string username)
        {
            return string.Format(DataCache.UserCacheKey, portalId, username);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 7.3. Replaced by UserController.Instance.GetCurrentUserInfo()")]
        public static UserInfo GetCurrentUserInfo()
        {
            return GetCurrentUserInternal();
        }


        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. Not needed any longer for due to autohydration")]
        public static ArrayList GetUnAuthorizedUsers(int portalId, bool isHydrated)
        {
            return GetUnAuthorizedUsers(portalId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. Not needed any longer for due to autohydration")]
        public static UserInfo GetUser(int portalId, int userId, bool isHydrated)
        {
            return GetUserById(portalId, userId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. Not needed any longer for single users due to autohydration")]
        public static UserInfo GetUser(int portalId, int userId, bool isHydrated, bool hydrateRoles)
        {
            return GetUserById(portalId, userId);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public UserInfo GetUserByUsername(int portalID, string username)
        {
            return GetCachedUser(portalID, username);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public UserInfo GetUserByUsername(int portalID, string username, bool synchronizeUsers)
        {
            return GetCachedUser(portalID, username);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUserByName")]
        public static UserInfo GetUserByName(int portalId, string username, bool isHydrated)
        {
            return GetCachedUser(portalId, username);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public ArrayList GetSuperUsers()
        {
            return GetUsers(Null.NullInteger);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public ArrayList GetUsers(bool synchronizeUsers, bool progressiveHydration)
        {
            return GetUsers(Null.NullInteger);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public ArrayList GetUsers(int portalId, bool synchronizeUsers, bool progressiveHydration)
        {
            var totalRecords = -1;
            return GetUsers(portalId, -1, -1, ref totalRecords);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public static ArrayList GetUsers(int portalId, bool isHydrated)
        {
            var totalRecords = -1;
            return GetUsers(portalId, -1, -1, ref totalRecords);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsers")]
        public static ArrayList GetUsers(int portalId, bool isHydrated, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsers(portalId, pageIndex, pageSize, ref totalRecords);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsersByEmail")]
        public static ArrayList GetUsersByEmail(int portalId, bool isHydrated, string emailToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByEmail(portalId, emailToMatch, pageIndex, pageSize, ref totalRecords);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsersByUserName")]
        public static ArrayList GetUsersByUserName(int portalId, bool isHydrated, string userNameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByUserName(portalId, userNameToMatch, pageIndex, pageSize, ref totalRecords);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.GetUsersByProfileProperty")]
        public static ArrayList GetUsersByProfileProperty(int portalId, bool isHydrated, string propertyName, string propertyValue, int pageIndex, int pageSize, ref int totalRecords)
        {
            return GetUsersByProfileProperty(portalId, propertyName, propertyValue, pageIndex, pageSize, ref totalRecords);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.1. The method had no implementation !!!")]
        public static void SetAuthCookie(string username, bool createPersistentCookie)
        {
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.ChangePassword")]
        public bool SetPassword(UserInfo objUser, string newPassword)
        {
            return ChangePassword(objUser, Null.NullString, newPassword);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.ChangePassword")]
        public bool SetPassword(UserInfo objUser, string oldPassword, string newPassword)
        {
            return ChangePassword(objUser, oldPassword, newPassword);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.UnlockUserAccount")]
        public void UnlockUserAccount(UserInfo objUser)
        {
            UnLockUser(objUser);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 5.1. This function has been replaced by UserController.UpdateUser")]
        public void UpdateUser(UserInfo objUser)
        {
            UpdateUser(objUser.PortalID, objUser);
        }


		/// <summary>
		/// overload will validate the token and if valid change the password
		/// it does not require an old password as it supports hashed passwords
		/// errorMessage will define why reset failed
		/// </summary>
		/// <param name="newPassword">The new password.</param>
		/// <param name="resetToken">The reset token, typically supplied through a password reset email.</param>
		/// <returns>A Boolean indicating success or failure.</returns>
		[Obsolete("Deprecate in 7.4.2, Use ChangePasswordByToken(int portalid, string username, string newPassword, string answer, string resetToken, out string errorMessage).")]
		public static bool ChangePasswordByToken(int portalid, string username, string newPassword, string resetToken, out string errorMessage)
		{
			return ChangePasswordByToken(portalid, username, newPassword, string.Empty, resetToken, out errorMessage);
		}
    }
}
