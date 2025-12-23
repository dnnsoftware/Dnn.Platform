// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users.Membership
{
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.Security;

    using DotNetNuke.Security.Membership;

    public class MembershipPasswordSettings
    {
        /// <summary>Initializes a new instance of the <see cref="MembershipPasswordSettings"/> class.</summary>
        /// <param name="portalId">The portal ID.</param>
        public MembershipPasswordSettings(int portalId)
        {
            // portalId not used currently - left in place for potential site specific settings
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
            else
            {
                // setup default values during install process.
                this.EnableStrengthMeter = true;
                this.EnableBannedList = true;
                this.EnablePasswordHistory = true;
            }
        }

        /// <summary>Gets minimum number of non-alphanumeric characters setting for password strength indicator.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int MinNonAlphanumericCharacters => System.Web.Security.Membership.MinRequiredNonAlphanumericCharacters;

        /// <summary>Gets minimum length of password setting for password strength indicator.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public int MinPasswordLength => System.Web.Security.Membership.MinRequiredPasswordLength;

        /// <summary>Gets currently configured password format for installation.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
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

        /// <summary>Gets regular Expression to validate password strength.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ValidationExpression => System.Web.Security.Membership.PasswordStrengthRegularExpression;

        public bool EnableBannedList { get; set; }

        public bool EnableStrengthMeter { get; set; }

        public bool EnableIPChecking { get; set; }

        public bool EnablePasswordHistory { get; set; }

        public int NumberOfPasswordsStored { get; set; }

        public int NumberOfDaysBeforePasswordReuse { get; set; }

        public int ResetLinkValidity { get; set; }

        public int PortalId { get; set; }

        private static bool IsInstallRequest(HttpRequest request)
        {
            var url = request.Url.LocalPath.ToLowerInvariant();

            return url.EndsWith("/install.aspx")
                   || url.Contains("/installwizard.aspx");
        }
    }
}
