// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Globalization;

    using DotNetNuke.Services.Tokens;

    public class MembershipPropertyAccess : IPropertyAccess
    {
        private readonly UserInfo objUser;

        /// <summary>Initializes a new instance of the <see cref="MembershipPropertyAccess"/> class.</summary>
        /// <param name="user">The user info.</param>
        public MembershipPropertyAccess(UserInfo user)
        {
            this.objUser = user;
        }

        /// <inheritdoc/>
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// <inheritdoc/>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            UserMembership objMembership = this.objUser.Membership;
            bool userQueriesHimself = this.objUser.UserID == accessingUser.UserID && this.objUser.UserID != -1;
            if (currentScope < Scope.DefaultSettings || (currentScope == Scope.DefaultSettings && !userQueriesHimself) ||
                ((currentScope != Scope.SystemMessages || this.objUser.IsSuperUser)
                    && (propertyName.Equals("password", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("passwordanswer", StringComparison.OrdinalIgnoreCase) || propertyName.Equals("passwordquestion", StringComparison.OrdinalIgnoreCase))))
            {
                propertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }

            string outputFormat = string.Empty;
            if (format == string.Empty)
            {
                outputFormat = "g";
            }

            switch (propertyName.ToLowerInvariant())
            {
                case "approved":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.Approved, formatProvider);
                case "createdondate":
                    return objMembership.CreatedDate.ToString(outputFormat, formatProvider);
                case "isonline":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.IsOnLine, formatProvider);
                case "lastactivitydate":
                    return objMembership.LastActivityDate.ToString(outputFormat, formatProvider);
                case "lastlockoutdate":
                    return objMembership.LastLockoutDate.ToString(outputFormat, formatProvider);
                case "lastlogindate":
                    return objMembership.LastLoginDate.ToString(outputFormat, formatProvider);
                case "lastpasswordchangedate":
                    return objMembership.LastPasswordChangeDate.ToString(outputFormat, formatProvider);
                case "lockedout":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.LockedOut, formatProvider);
                case "objecthydrated":
                    return PropertyAccess.Boolean2LocalizedYesNo(true, formatProvider);
                case "password":
                    return PropertyAccess.FormatString(objMembership.Password, format);
                case "passwordanswer":
                    return PropertyAccess.FormatString(objMembership.PasswordAnswer, format);
                case "passwordquestion":
                    return PropertyAccess.FormatString(objMembership.PasswordQuestion, format);
                case "passwordresettoken":
                    return PropertyAccess.FormatString(Convert.ToString(this.objUser.PasswordResetToken, CultureInfo.InvariantCulture), format);
                case "passwordresetexpiration":
                    return PropertyAccess.FormatString(this.objUser.PasswordResetExpiration.ToString(formatProvider), format);
                case "updatepassword":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.UpdatePassword, formatProvider);
                case "username":
                    return PropertyAccess.FormatString(this.objUser.Username, format);
                case "email":
                    return PropertyAccess.FormatString(this.objUser.Email, format);
            }

            return PropertyAccess.GetObjectProperty(objMembership, propertyName, format, formatProvider, ref propertyNotFound);
        }
    }
}
