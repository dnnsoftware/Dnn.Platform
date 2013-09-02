#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System.Globalization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Services.Subscriptions.Common;

namespace DotNetNuke.Services.Subscriptions.Entities.Templates
{
    public class AuthorContent : IPropertyAccess
    {
        #region Constructors

        public AuthorContent(UserInfo authorUser)
        {
            _userInfo = authorUser;
        }

        #endregion

        #region Private members

        private readonly UserInfo _userInfo;

        #endregion

        #region Implementation of IPropertyAccess

        public string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound)
        {
            switch (propertyName.ToLowerInvariant())
            {
                case "displayname":
                    return string.IsNullOrEmpty(_userInfo.DisplayName)
                        ? string.Empty
                        : _userInfo.DisplayName.ToString(formatProvider);
                case "email":
                    return string.IsNullOrEmpty(_userInfo.Email)
                        ? string.Empty
                        : _userInfo.Email.ToString(formatProvider);
                case "firstname":
                    return string.IsNullOrEmpty(_userInfo.FirstName)
                        ? string.Empty
                        : _userInfo.FirstName.ToString(formatProvider);
                case "lastname":
                    return string.IsNullOrEmpty(_userInfo.LastName)
                        ? string.Empty
                        : _userInfo.LastName.ToString(formatProvider);
                case "profileurl":

                    string profileUrl = "";

                    //var portalController = new PortalController();
                    //var profileTabID = portalController.GetPortal(_userInfo.PortalID).UserTabId;
                    
                    //profileUrl = DotNetNuke.Common.Globals.NavigateURL(profileTabID, "", string.Format("userId={0}", _userInfo.UserID));

                    return profileUrl;

                default:
                    propertyNotFound = true;
                    return null;
            }
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