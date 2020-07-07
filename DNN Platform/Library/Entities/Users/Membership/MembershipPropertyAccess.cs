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

        public MembershipPropertyAccess(UserInfo User)
        {
            this.objUser = User;
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope CurrentScope, ref bool PropertyNotFound)
        {
            UserMembership objMembership = this.objUser.Membership;
            bool UserQueriesHimself = this.objUser.UserID == AccessingUser.UserID && this.objUser.UserID != -1;
            if (CurrentScope < Scope.DefaultSettings || (CurrentScope == Scope.DefaultSettings && !UserQueriesHimself) ||
                ((CurrentScope != Scope.SystemMessages || this.objUser.IsSuperUser)
                    && (propertyName.Equals("password", StringComparison.InvariantCultureIgnoreCase) || propertyName.Equals("passwordanswer", StringComparison.InvariantCultureIgnoreCase) || propertyName.Equals("passwordquestion", StringComparison.InvariantCultureIgnoreCase))))
            {
                PropertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }

            string OutputFormat = string.Empty;
            if (format == string.Empty)
            {
                OutputFormat = "g";
            }

            switch (propertyName.ToLowerInvariant())
            {
                case "approved":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.Approved, formatProvider);
                case "createdondate":
                    return objMembership.CreatedDate.ToString(OutputFormat, formatProvider);
                case "isonline":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.IsOnLine, formatProvider);
                case "lastactivitydate":
                    return objMembership.LastActivityDate.ToString(OutputFormat, formatProvider);
                case "lastlockoutdate":
                    return objMembership.LastLockoutDate.ToString(OutputFormat, formatProvider);
                case "lastlogindate":
                    return objMembership.LastLoginDate.ToString(OutputFormat, formatProvider);
                case "lastpasswordchangedate":
                    return objMembership.LastPasswordChangeDate.ToString(OutputFormat, formatProvider);
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
                    return PropertyAccess.FormatString(Convert.ToString(this.objUser.PasswordResetToken), format);
                case "passwordresetexpiration":
                    return PropertyAccess.FormatString(this.objUser.PasswordResetExpiration.ToString(formatProvider), format);
                case "updatepassword":
                    return PropertyAccess.Boolean2LocalizedYesNo(objMembership.UpdatePassword, formatProvider);
                case "username":
                    return PropertyAccess.FormatString(this.objUser.Username, format);
                case "email":
                    return PropertyAccess.FormatString(this.objUser.Email, format);
            }

            return PropertyAccess.GetObjectProperty(objMembership, propertyName, format, formatProvider, ref PropertyNotFound);
        }
    }
}
