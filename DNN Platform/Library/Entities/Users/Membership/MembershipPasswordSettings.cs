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
using System.Web;
using System.Web.Security;
using DotNetNuke.Common;
using DotNetNuke.Security.Membership;

namespace DotNetNuke.Entities.Users.Membership
{
    public class MembershipPasswordSettings
    {
        #region Public Properties

        public bool EnableBannedList { get; set; }
        public bool EnableStrengthMeter { get; set; }
        public bool EnableIPChecking { get; set; }
        public bool EnablePasswordHistory { get; set; }

        public int NumberOfPasswordsStored { get; set; }
        public int NumberOfDaysBeforePasswordReuse { get; set; }
        public int ResetLinkValidity { get; set; }

        /// <summary>
        /// minimum number of non-alphanumeric characters setting for password strength indicator
        /// </summary>
        public int MinNonAlphanumericCharacters
        {
            get
            {
                return System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters;
            }
        }

        /// <summary>
        /// minimum length of password setting for password strength indicator
        /// </summary>
        public int MinPasswordLength
        {
            get
            {
                return System.Web.Security.Membership.MinRequiredPasswordLength;
            }
        }

        /// <summary>
        /// currently configured password format for installation
        /// </summary>
        public PasswordFormat PasswordFormat
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
        }

        /// <summary>
        /// Regular Expression to validate password strength.
        /// </summary>
        public string ValidationExpression
        {
            get { return System.Web.Security.Membership.PasswordStrengthRegularExpression; }
        }

        #endregion

        #region initialization methods

        /// <summary>
        /// Initialiser for MembershipPasswordSettings provider object.  
        /// </summary>
        public MembershipPasswordSettings(int portalId)
        {
            //portalId not used currently - left in place for potential site specific settings
            PortalId = portalId;

            if (HttpContext.Current != null && !IsInstallRequest(HttpContext.Current.Request))
            {
                EnableBannedList = Host.Host.EnableBannedList;
                EnableStrengthMeter = Host.Host.EnableStrengthMeter;
                EnableIPChecking = Host.Host.EnableIPChecking;
                EnablePasswordHistory = Host.Host.EnablePasswordHistory;

                ResetLinkValidity = Host.Host.MembershipResetLinkValidity;
                NumberOfPasswordsStored = Host.Host.MembershipNumberPasswords;
                NumberOfDaysBeforePasswordReuse = Host.Host.MembershipDaysBeforePasswordReuse;
            }
            else //setup default values during install process.
            {
                EnableStrengthMeter = true;
                EnableBannedList = true;
                EnablePasswordHistory = true;
            }
        }

        public int PortalId { get; set; }

        #endregion

        #region Private Methods

        private static bool IsInstallRequest(HttpRequest request)
        {
            var url = request.Url.LocalPath.ToLower();

            return url.EndsWith("/install.aspx")
                   || url.Contains("/installwizard.aspx");
        }

        #endregion
    }
}