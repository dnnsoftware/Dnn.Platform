// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Exceptions;

    /// <summary>
    /// The UserController class provides Business Layer methods for Users.
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
        [Obsolete("Deprecated in DNN 7.2.2. This method has been replaced by UserController.MoveUserToPortal and UserControllar.CopyUserToPortal. Scheduled removal in v10.0.0.")]
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
        [Obsolete("Deprecated in DNN 7.3. Replaced by UserController.Instance.GetCurrentUserInfo(). Scheduled removal in v10.0.0.")]
        public static UserInfo GetCurrentUserInfo()
        {
            return GetCurrentUserInternal();
        }

        /// <summary>
        /// overload will validate the token and if valid change the password
        /// it does not require an old password as it supports hashed passwords
        /// errorMessage will define why reset failed.
        /// </summary>
        /// <param name="newPassword">The new password.</param>
        /// <param name="resetToken">The reset token, typically supplied through a password reset email.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        [Obsolete("Deprecate in 7.4.2, Use ChangePasswordByToken(int portalid, string username, string newPassword, string answer, string resetToken, out string errorMessage).. Scheduled removal in v10.0.0.")]
        public static bool ChangePasswordByToken(int portalid, string username, string newPassword, string resetToken, out string errorMessage)
        {
            return ChangePasswordByToken(portalid, username, newPassword, string.Empty, resetToken, out errorMessage);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in DNN 6.1, keep this method to compatible with upgrade wizard.. Scheduled removal in v10.0.0.")]
        public static UserInfo FillUserInfo(int portalId, IDataReader dr, bool closeDataReader)
        {
            UserInfo objUserInfo = null;
            try
            {
                // read datareader
                var bContinue = true;
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
                    objUserInfo = new UserInfo
                    {
                        PortalID = portalId,
                        IsSuperUser = Null.SetNullBoolean(dr["IsSuperUser"]),
                        IsDeleted = Null.SetNullBoolean(dr["IsDeleted"]),
                        UserID = Null.SetNullInteger(dr["UserID"]),
                        FirstName = Null.SetNullString(dr["FirstName"]),
                        LastName = Null.SetNullString(dr["LastName"]),
                        DisplayName = Null.SetNullString(dr["DisplayName"]),
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
    }
}
