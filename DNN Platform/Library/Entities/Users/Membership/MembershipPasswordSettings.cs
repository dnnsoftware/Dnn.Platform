// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this.PortalId = portalId;

            if (HttpContext.Current != null && !IsInstallRequest(HttpContext.Current.Request))
            {
                this.EnableBannedList = Host.Host.EnableBannedList;
                this.EnableStrengthMeter = Host.Host.EnableStrengthMeter;
                this.EnableIPChecking = Host.Host.EnableIPChecking;
                this.EnablePasswordHistory = Host.Host.EnablePasswordHistory;

                this.ResetLinkValidity = Host.Host.MembershipResetLinkValidity;
                this.NumberOfPasswordsStored = Host.Host.MembershipNumberPasswords;
                this.NumberOfDaysBeforePasswordReuse = Host.Host.MembershipDaysBeforePasswordReuse;
            }
            else //setup default values during install process.
            {
                this.EnableStrengthMeter = true;
                this.EnableBannedList = true;
                this.EnablePasswordHistory = true;
            }
        }

        public int PortalId { get; set; }

        #endregion

        #region Private Methods

        private static bool IsInstallRequest(HttpRequest request)
        {
            var url = request.Url.LocalPath.ToLowerInvariant();

            return url.EndsWith("/install.aspx")
                   || url.Contains("/installwizard.aspx");
        }

        #endregion
    }
}
