#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.SystemDateTime;
using DotNetNuke.Services.Tokens;
using DotNetNuke.UI.WebControls;

#endregion

namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      UserInfo
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The UserInfo class provides Business Layer model for Users
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    ///     [cnurse]	12/13/2005	documented
    /// </history>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class UserInfo : BaseEntityInfo, IPropertyAccess
    {
        #region Private Members

        private string _administratorRoleName;
        private string _fullName;
        private UserMembership _membership;
        private UserProfile _profile;
        //private IDictionary<int, UserSocial> _social;
        private readonly SharedDictionary<int, UserSocial> _social = new SharedDictionary<int, UserSocial>();

        #endregion

        #region Constructors

        public UserInfo()
        {
            IsDeleted = Null.NullBoolean;
            UserID = Null.NullInteger;
            PortalID = Null.NullInteger;
            IsSuperUser = Null.NullBoolean;
            AffiliateID = Null.NullInteger;
        }

        #endregion

        #region Public Properties

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the AffiliateId for this user
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public int AffiliateID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Display Name
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [SortOrder(3), Required(true), MaxLength(128)]
        public string DisplayName { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Email Address
        /// </summary>
        /// <history>
        ///     [cnurse]	02/27/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [SortOrder(4), MaxLength(256), Required(true), RegularExpressionValidator(Globals.glbEmailRegEx)]
        public string Email { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the First Name
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [SortOrder(1), MaxLength(50)]
        public string FirstName
        {
            get { return Profile.FirstName; }
            set { Profile.FirstName = value; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the User is deleted
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsDeleted { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets whether the User is a SuperUser
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public bool IsSuperUser { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Last IP address used by user
        /// </summary>
        /// <history>
        ///     [cnurse]	02/13/2009	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public string LastIPAddress { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Last Name
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [SortOrder(2), MaxLength(50)]
        public string LastName
        {
            get { return Profile.LastName; }
            set { Profile.LastName = value; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Membership object
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public UserMembership Membership
        {
            get
            {
                if (_membership == null)
                {
                    _membership = new UserMembership(this);
                    if ((Username != null) && (!String.IsNullOrEmpty(Username)))
                    {
                        UserController.GetUserMembership(this);
                    }
                }
                return _membership;
            }
            set { _membership = value; }
        }

        /// <summary>
        /// gets and sets the token created for resetting passwords
        /// </summary>
        [Browsable(false)]
        public Guid PasswordResetToken { get; set; }

        /// <summary>
        /// gets and sets the datetime that the PasswordResetToken is valid
        /// </summary>
        [Browsable(false)]
        public DateTime PasswordResetExpiration { get; set; }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the PortalId
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public int PortalID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Profile Object
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public UserProfile Profile
        {
            get
            {
                if (_profile == null)
                {
                    _profile = new UserProfile(this);
                    UserInfo userInfo = this;
                    ProfileController.GetUserProfile(ref userInfo);
                }
                return _profile;
            }
            set { _profile = value; }
        }

        [Browsable(false)]
        public string[] Roles
        {
            get
            {
                return (from r in Social.Roles
                        where
                            r.Status == RoleStatus.Approved &&
                            (r.EffectiveDate < DateTime.Now || Null.IsNull(r.EffectiveDate)) &&
                            (r.ExpiryDate > DateTime.Now || Null.IsNull(r.ExpiryDate))
                        select r.RoleName
                        ).ToArray();    
            }
            set { }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the Social property
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public UserSocial Social
        {
            get
            {
                UserSocial userSocial;
                var exists = false;
                using (_social.GetReadLock())
                {
                    exists = _social.TryGetValue(PortalID, out userSocial);
                }

                if (!exists)
                {
                    using (_social.GetWriteLock())
                    {
                        userSocial = new UserSocial(this);
                        _social.Add(PortalID, userSocial);
                    }
                }

                return userSocial;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User Id
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public int UserID { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets and sets the User Name
        /// </summary>
        /// <history>
        ///     [cnurse]	02/24/2006	Documented
        /// </history>
        /// -----------------------------------------------------------------------------
        [SortOrder(0), MaxLength(100), IsReadOnly(true), Required(true)]
        public string Username { get; set; }

        public string VanityUrl { get; set; }


        #region IPropertyAccess Members

        /// <summary>
        /// Property access, initially provided for TokenReplace
        /// </summary>
        /// <param name="propertyName">Name of the Property</param>
        /// <param name="format">format string</param>
        /// <param name="formatProvider">format provider for numbers, dates, currencies</param>
        /// <param name="accessingUser">userinfo of the user, who queries the data (used to determine permissions)</param>
        /// <param name="currentScope">requested maximum access level, might be restricted due to user level</param>
        /// <param name="propertyNotFound">out: flag, if property could be retrieved.</param>
        /// <returns>current value of the property for this userinfo object</returns>
        /// <history>
        ///    2007-10-20   [sleupold]   documented and extended with differenciated access permissions
        ///    2007-10-20   [sleupold]   role access added (for user himself or admin only).
        /// </history>
        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound)
        {
            Scope internScope;
            if (UserID == -1 && currentScope > Scope.Configuration)
            {
                internScope = Scope.Configuration; //anonymous users only get access to displayname
            }
            else if (UserID != accessingUser.UserID && !isAdminUser(ref accessingUser) && currentScope > Scope.DefaultSettings)
            {
                internScope = Scope.DefaultSettings; //registerd users can access username and userID as well
            }
            else
            {
                internScope = currentScope; //admins and user himself can access all data
            }
            string outputFormat = format == string.Empty ? "g" : format;
            switch (propertyName.ToLower())
            {
                case "verificationcode":
                    if (internScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    var ps = new PortalSecurity();
                    var code = ps.EncryptString(PortalID + "-" + UserID, Config.GetDecryptionkey());
                    return code.Replace("+", ".").Replace("/", "-").Replace("=", "_");
                case "affiliateid":
                    if (internScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (AffiliateID.ToString(outputFormat, formatProvider));
                case "displayname":
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return PropertyAccess.FormatString(DisplayName, format);
                case "email":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (PropertyAccess.FormatString(Email, format));
                case "firstname": //using profile property is recommended!
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (PropertyAccess.FormatString(FirstName, format));
                case "issuperuser":
                    if (internScope < Scope.Debug)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (IsSuperUser.ToString(formatProvider));
                case "lastname": //using profile property is recommended!
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (PropertyAccess.FormatString(LastName, format));
                case "portalid":
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (PortalID.ToString(outputFormat, formatProvider));
                case "userid":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (UserID.ToString(outputFormat, formatProvider));
                case "username":
                    if (internScope < Scope.DefaultSettings)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (PropertyAccess.FormatString(Username, format));
                case "fullname": //fullname is obsolete, it will return DisplayName
                    if (internScope < Scope.Configuration)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (PropertyAccess.FormatString(DisplayName, format));
                case "roles":
                    if (currentScope < Scope.SystemMessages)
                    {
                        propertyNotFound = true;
                        return PropertyAccess.ContentLocked;
                    }
                    return (PropertyAccess.FormatString(string.Join(", ", Roles), format));
            }
            propertyNotFound = true;
            return string.Empty;
        }

        [Browsable(false)]
        public CacheLevel Cacheability
        {
            get
            {
                return CacheLevel.notCacheable;
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Determine, if accessing user is Administrator
        /// </summary>
        /// <param name="accessingUser">userinfo of the user to query</param>
        /// <returns>true, if user is portal administrator or superuser</returns>
        /// <history>
        ///    2007-10-20 [sleupold] added
        /// </history>
        private bool isAdminUser(ref UserInfo accessingUser)
        {
            if (accessingUser == null || accessingUser.UserID == -1)
            {
                return false;
            }
            if (String.IsNullOrEmpty(_administratorRoleName))
            {
                PortalInfo ps = PortalController.Instance.GetPortal(accessingUser.PortalID);
                _administratorRoleName = ps.AdministratorRoleName;
            }
            return accessingUser.IsInRole(_administratorRoleName) || accessingUser.IsSuperUser;
        }

        #endregion

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// IsInRole determines whether the user is in the role passed
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// <history>
        ///     [cnurse]	12/13/2005	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool IsInRole(string role)
        {
            if (IsSuperUser || role == Globals.glbRoleAllUsersName)
            {
                return true;
            }
            if (UserID == Null.NullInteger && role == Globals.glbRoleUnauthUserName)
            {
                return true;
            }
            if ("[" + UserID + "]" == role)
            {
                return true;
            }
            if (Roles != null)
            {
                return Roles.Any(s => s == role);
            }
            return false;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets current time in User's timezone
        /// </summary>
        /// <history>
        ///     [aprasad]	07/19/2011	Added
        /// </history>
        /// -----------------------------------------------------------------------------        
        public DateTime LocalTime()
        {
            return LocalTime(DateUtils.GetDatabaseTime());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Convert utc time in User's timezone
        /// </summary>
        /// <param name="utcTime">Utc time to convert</param>
        /// <history>
        ///     [aprasad]	07/19/2011	Added
        /// </history>
        /// -----------------------------------------------------------------------------       
        public DateTime LocalTime(DateTime utcTime)
        {
            if (UserID > Null.NullInteger)
            {
                return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, Profile.PreferredTimeZone);
            }
            return TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, PortalController.Instance.GetCurrentPortalSettings().TimeZone);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateDisplayName updates the displayname to the format provided
        /// </summary>
        /// <param name="format">The format to use</param>
        /// <history>
        ///     [cnurse]	02/21/2007	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void UpdateDisplayName(string format)
        {
            //Replace Tokens
            format = format.Replace("[USERID]", UserID.ToString(CultureInfo.InvariantCulture));
            format = format.Replace("[FIRSTNAME]", FirstName);
            format = format.Replace("[LASTNAME]", LastName);
            format = format.Replace("[USERNAME]", Username);
            DisplayName = format;
        }

        #endregion

        #region Obsolete

        [Obsolete("Deprecated in DNN 6.2. Roles are no longer stored in a cookie")]
        public void ClearRoles() { }

        [Browsable(false), Obsolete("Deprecated in DNN 5.1. This property has been deprecated in favour of Display Name")]
        public string FullName
        {
            get
            {
                if (String.IsNullOrEmpty(_fullName))
                {
                    _fullName = FirstName + " " + LastName;
                }
                return _fullName;
            }
            set { _fullName = value; }
        }

        [Browsable(false)]
        [Obsolete("Deprecated in DNN 6.2. Roles are no longer stored in a cookie so this property is no longer neccessary")]
        public bool RefreshRoles { get; set; }

        #endregion
    }
}
