#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Globalization;

using DotNetNuke.Services.Tokens;

#endregion

namespace DotNetNuke.Entities.Users
{
    public class MembershipPropertyAccess : IPropertyAccess
    {
        private readonly UserInfo objUser;

        public MembershipPropertyAccess(UserInfo User)
        {
            objUser = User;
        }

        #region IPropertyAccess Members

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo AccessingUser, Scope CurrentScope, ref bool PropertyNotFound)
        {
            UserMembership objMembership = objUser.Membership;
            bool UserQueriesHimself = (objUser.UserID == AccessingUser.UserID && objUser.UserID != -1);
            if (CurrentScope < Scope.DefaultSettings || (CurrentScope == Scope.DefaultSettings && !UserQueriesHimself) ||
                ((CurrentScope != Scope.SystemMessages || objUser.IsSuperUser) 
                    && (propertyName.ToLower() == "password" || propertyName.ToLower() == "passwordanswer" || propertyName.ToLower() == "passwordquestion")
                ))
            {
                PropertyNotFound = true;
                return PropertyAccess.ContentLocked;
            }
            string OutputFormat = string.Empty;
            if (format == string.Empty)
            {
                OutputFormat = "g";
            }
            switch (propertyName.ToLower())
            {
                case "approved":
                    return (PropertyAccess.Boolean2LocalizedYesNo(objMembership.Approved, formatProvider));
                case "createdondate":
                    return (objMembership.CreatedDate.ToString(OutputFormat, formatProvider));
                case "isonline":
                    return (PropertyAccess.Boolean2LocalizedYesNo(objMembership.IsOnLine, formatProvider));
                case "lastactivitydate":
                    return (objMembership.LastActivityDate.ToString(OutputFormat, formatProvider));
                case "lastlockoutdate":
                    return (objMembership.LastLockoutDate.ToString(OutputFormat, formatProvider));
                case "lastlogindate":
                    return (objMembership.LastLoginDate.ToString(OutputFormat, formatProvider));
                case "lastpasswordchangedate":
                    return (objMembership.LastPasswordChangeDate.ToString(OutputFormat, formatProvider));
                case "lockedout":
                    return (PropertyAccess.Boolean2LocalizedYesNo(objMembership.LockedOut, formatProvider));
                case "objecthydrated":
                    return (PropertyAccess.Boolean2LocalizedYesNo(true, formatProvider));
                case "password":
                    return PropertyAccess.FormatString(objMembership.Password, format);
                case "passwordanswer":
                    return PropertyAccess.FormatString(objMembership.PasswordAnswer, format);
                case "passwordquestion":
                    return PropertyAccess.FormatString(objMembership.PasswordQuestion, format);
                case "passwordresettoken":
                    return PropertyAccess.FormatString(Convert.ToString(objUser.PasswordResetToken), format);
                case "passwordresetexpiration":
                    return PropertyAccess.FormatString(objUser.PasswordResetExpiration.ToString(formatProvider), format);
                case "updatepassword":
                    return (PropertyAccess.Boolean2LocalizedYesNo(objMembership.UpdatePassword, formatProvider));
                case "username":
                    return (PropertyAccess.FormatString(objUser.Username, format));
                case "email":
                    return (PropertyAccess.FormatString(objUser.Email, format));
            }
            return PropertyAccess.GetObjectProperty(objMembership, propertyName, format, formatProvider, ref PropertyNotFound);
        }

        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        #endregion
    }
}