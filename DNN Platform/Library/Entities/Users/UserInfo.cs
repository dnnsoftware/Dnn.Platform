// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Tokens;
    using DotNetNuke.UI.WebControls;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserInfo class provides Business Layer model for Users.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserInfo : BaseEntityInfo, IPropertyAccess
    {
        private readonly ConcurrentDictionary<int, UserSocial> _social = new ConcurrentDictionary<int, UserSocial>();
        private string _administratorRoleName;
        private UserMembership _membership;
        private UserProfile _profile;

        public UserInfo()
        {
            this.IsDeleted = Null.NullBoolean;
            this.UserID = Null.NullInteger;
            this.PortalID = Null.NullInteger;
            this.IsSuperUser = Null.NullBoolean;
            this.AffiliateID = Null.NullInteger;
        }

        /// <summary>
        /// Gets a value indicating whether gets whether the user is in the portal's administrators role.
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                if (this.IsSuperUser)
                {
                    return true;
                }

                PortalInfo ps = PortalController.Instance.GetPortal(this.PortalID);
                return ps != null && this.IsInRole(ps.AdministratorRoleName);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Social property.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public UserSocial Social
        {
            get
            {
                return this._social.GetOrAdd(this.PortalID, i => new UserSocial(this));
            }
        }

        [Browsable(false)]
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the AffiliateId for this user.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public int AffiliateID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Display Name.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(3)]
        [Required(true)]
        [MaxLength(128)]
        public string DisplayName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Email Address.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(4)]
        [MaxLength(256)]
        [Required(true)]
        [RegularExpressionValidator(Globals.glbEmailRegEx)]
        public string Email { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the First Name.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(1)]
        [MaxLength(50)]
        public string FirstName
        {
            get { return this.Profile.FirstName; }
            set { this.Profile.FirstName = value; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the User is deleted.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsDeleted { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the User is a SuperUser.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsSuperUser { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Last IP address used by user.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public string LastIPAddress { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Last Name.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(2)]
        [MaxLength(50)]
        public string LastName
        {
            get { return this.Profile.LastName; }
            set { this.Profile.LastName = value; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Membership object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public UserMembership Membership
        {
            get
            {
                if (this._membership == null)
                {
                    this._membership = new UserMembership(this);
                    if ((this.Username != null) && (!string.IsNullOrEmpty(this.Username)))
                    {
                        UserController.GetUserMembership(this);
                    }
                }

                return this._membership;
            }

            set { this._membership = value; }
        }

        /// <summary>
        /// Gets or sets and sets the token created for resetting passwords.
        /// </summary>
        [Browsable(false)]
        public Guid PasswordResetToken { get; set; }

        /// <summary>
        /// Gets or sets and sets the datetime that the PasswordResetToken is valid.
        /// </summary>
        [Browsable(false)]
        public DateTime PasswordResetExpiration { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the PortalId.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public int PortalID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the user has agreed to the terms and conditions.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool HasAgreedToTerms { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets when the user last agreed to the terms and conditions.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public DateTime HasAgreedToTermsOn { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether gets and sets whether the user has requested they be removed from the site.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool RequestsRemoval { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the Profile Object.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public UserProfile Profile
        {
            get
            {
                if (this._profile == null)
                {
                    this._profile = new UserProfile(this);
                    UserInfo userInfo = this;
                    ProfileController.GetUserProfile(ref userInfo);
                }

                return this._profile;
            }

            set { this._profile = value; }
        }

        [Browsable(false)]
        public string[] Roles
        {
            get
            {
                var socialRoles = this.Social.Roles;
                if (socialRoles.Count == 0)
                {
                    return new string[0];
                }

                return (from r in this.Social.Roles
                        where
                            r.Status == RoleStatus.Approved &&
                            (r.EffectiveDate < DateTime.Now || Null.IsNull(r.EffectiveDate)) &&
                            (r.ExpiryDate > DateTime.Now || Null.IsNull(r.ExpiryDate))
                        select r.RoleName)
                        .ToArray();
            }

            set { }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User Id.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public int UserID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets and sets the User Name.
        /// </summary>
        /// -----------------------------------------------------------------------------
        [SortOrder(0)]
        [MaxLength(100)]
        [IsReadOnly(true)]
        [Required(true)]
        public string Username { get; set; }

        public string VanityUrl { get; set; }

        /// <summary>
        /// Property access, initially provided for TokenReplace.
        /// </summary>
        /// <param name="propertyName">Name of the Property.</param>
        /// <param name="format">format string.</param>
        /// <param name="formatProvider">format provider for numbers, dates, currencies.</param>
        /// <param name="accessingUser">userinfo of the user, who queries the data (used to determine permissions).</param>
        /// <param name="currentScope">requested maximum access level, might be restricted due to user level.</param>
        /// <param name="propertyNotFound">out: flag, if property could be retrieved.</param>
        /// <returns>current value of the property for this userinfo object.</returns>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            Scope internScope;
            if (this.UserID == -1 && currentScope > Scope.Configuration)
            {
                internScope = Scope.Configuration; // anonymous users only get access to displayname
            }
            else if (this.UserID != accessingUser.UserID && !this.isAdminUser(ref accessingUser) && currentScope > Scope.DefaultSettings)
            {
                internScope = Scope.DefaultSettings; // registerd users can access username and userID as well
            }
            else
            {
                internScope = currentScope; // admins and user himself can access all data
            }

            string outputFormat = format == string.Empty ? "g" : format;
            switch (propertyName.ToLowerInvariant())
            {
                case "verificationcode":
                    if (internScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    var ps = PortalSecurity.Instance;
                    var code = ps.Encrypt(Config.GetDecryptionkey(), this.PortalID + "-" + this.GetMembershipUserId());
                    return code.Replace("+", ".").Replace("/", "-").Replace("=", "_");
                case "affiliateid":
                    if (internScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.AffiliateID.ToString(outputFormat, formatProvider);
                case "displayname":
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.DisplayName, format);
                case "email":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.Email, format);
                case "firstname": // using profile property is recommended!
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.FirstName, format);
                case "issuperuser":
                    if (internScope < Scope.Debug)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.IsSuperUser.ToString(formatProvider);
                case "lastname": // using profile property is recommended!
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.LastName, format);
                case "portalid":
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.PortalID.ToString(outputFormat, formatProvider);
                case "userid":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return this.UserID.ToString(outputFormat, formatProvider);
                case "username":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.Username, format);
                case "fullname": // fullname is obsolete, it will return DisplayName
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(this.DisplayName, format);
                case "roles":
                    if (currentScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }

                    return PropertyAccess.FormatString(string.Join(", ", this.Roles), format);
            }

            propertyNotFound = true;
            return string.Empty;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IsInRole determines whether the user is in the role passed.
        /// </summary>
        /// <param name="role">The role to check.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public bool IsInRole(string role)
        {
            // super users should always be verified.
            if (this.IsSuperUser)
            {
                return role != "Unverified Users";
            }

            if (role == Globals.glbRoleAllUsersName)
            {
                return true;
            }

            if (this.UserID == Null.NullInteger && role == Globals.glbRoleUnauthUserName)
            {
                return true;
            }

            if ("[" + this.UserID + "]" == role)
            {
                return true;
            }

            var roles = this.Roles;
            if (roles != null)
            {
                return roles.Any(s => s == role);
            }

            return false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets current time in User's timezone.
        /// </summary>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public DateTime LocalTime()
        {
            return this.LocalTime(DateUtils.GetDatabaseUtcTime());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Convert utc time in User's timezone.
        /// </summary>
        /// <param name="utcTime">Utc time to convert.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public DateTime LocalTime(DateTime utcTime)
        {
            if (this.UserID > Null.NullInteger)
            {
                return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, this.Profile.PreferredTimeZone);
            }

            return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, PortalController.Instance.GetCurrentPortalSettings().TimeZone);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateDisplayName updates the displayname to the format provided.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// -----------------------------------------------------------------------------
        public void UpdateDisplayName(string format)
        {
            // Replace Tokens
            format = format.Replace("[USERID]", this.UserID.ToString(CultureInfo.InvariantCulture));
            format = format.Replace("[FIRSTNAME]", this.FirstName);
            format = format.Replace("[LASTNAME]", this.LastName);
            format = format.Replace("[USERNAME]", this.Username);
            this.DisplayName = format;
        }

        /// <summary>
        /// Determine, if accessing user is Administrator.
        /// </summary>
        /// <param name="accessingUser">userinfo of the user to query.</param>
        /// <returns>true, if user is portal administrator or superuser.</returns>
        private bool isAdminUser(ref UserInfo accessingUser)
        {
            if (accessingUser == null || accessingUser.UserID == -1)
            {
                return false;
            }

            if (string.IsNullOrEmpty(this._administratorRoleName))
            {
                PortalInfo ps = PortalController.Instance.GetPortal(accessingUser.PortalID);
                this._administratorRoleName = ps.AdministratorRoleName;
            }

            return accessingUser.IsInRole(this._administratorRoleName) || accessingUser.IsSuperUser;
        }

        private string GetMembershipUserId()
        {
            return MembershipProvider.Instance().GetProviderUserKey(this)?.Replace("-", string.Empty) ?? string.Empty;
        }
    }
}
