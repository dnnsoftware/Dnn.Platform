﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            var url = request.Url.LocalPath.ToLowerInvariant();

            return url.EndsWith("/install.aspx")
                   || url.Contains("/installwizard.aspx");
        }

        #endregion
    }
}
