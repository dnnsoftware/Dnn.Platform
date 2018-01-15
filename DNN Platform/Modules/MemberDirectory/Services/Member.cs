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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Tokens;
using System.Web;

namespace DotNetNuke.Modules.MemberDirectory.Services
{
    public class Member
    {
        private UserInfo _user;
        private UserInfo _viewer;
        private PortalSettings _settings;

        public Member(UserInfo user, PortalSettings settings)
        {
            _user = user;
            _settings = settings;
            _viewer = settings.UserInfo;
        }

        public int MemberId
        {
            get { return _user.UserID; }
        }

        public string City
        {
            get { return GetProfileProperty("City"); }
        }

        public string Country
        {
            get { return GetProfileProperty("Country"); }
        }

        public string DisplayName
        {
            get { return _user.DisplayName; }
        }

        public string Email
        {
            get { return (_viewer.IsInRole(_settings.AdministratorRoleName)) ? _user.Email : String.Empty; }
        }

        public string FirstName
        {
            get { return GetProfileProperty("FirstName"); }
        }

        public int FollowerStatus
        {
            get { return (_user.Social.Follower == null) ? 0 : (int)_user.Social.Follower.Status; }
        }

        public int FollowingStatus
        {
            get { return (_user.Social.Following == null) ? 0 : (int)_user.Social.Following.Status; }
        }

        public int FriendId
        {
            get { return (_user.Social.Friend == null) ? -1 : (int)_user.Social.Friend.RelatedUserId; }
        }

        public int FriendStatus
        {
            get { return (_user.Social.Friend == null) ? 0 : (int)_user.Social.Friend.Status; }
        }

        public string LastName
        {
            get { return GetProfileProperty("LastName"); }
        }

        public string Phone
        {
            get { return GetProfileProperty("Telephone"); }
        }

        public string PhotoURL
        {
            get { return _user.Profile.PhotoURL; }
        }

        public Dictionary<string, string> ProfileProperties
        {
            get
            {
                var properties = new Dictionary<string, string>();
                bool propertyNotFound = false;
                var propertyAccess = new ProfilePropertyAccess(_user);
                foreach(ProfilePropertyDefinition property in _user.Profile.ProfileProperties)
                {
                    string value = propertyAccess.GetProperty(property.PropertyName,
                                                             String.Empty,
                                                             Thread.CurrentThread.CurrentUICulture,
                                                             _viewer,
                                                             Scope.DefaultSettings,
                                                             ref propertyNotFound);

                    properties[property.PropertyName] = string.IsNullOrEmpty(value) ? "" : Common.Utilities.HtmlUtils.Clean(HttpUtility.HtmlDecode(value), false);
                }
                return properties;
            }
        }

        public string Title
        {
            get { return _user.Profile.Title; }
        }

        public string UserName
        {
            get { return (_viewer.IsInRole(_settings.AdministratorRoleName)) ? _user.Username : String.Empty; }
        }

        public string Website
        {
            get { return GetProfileProperty("Website"); }
        }

        public string ProfileUrl
        {
            get { return Globals.UserProfileURL(MemberId); }
        }

        /// <summary>
        /// This method returns the value of the ProfileProperty if is defined, otherwise it returns an Empty string
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>property value</returns>
        private string GetProfileProperty(string propertyName)
        {
            var profileProperties = ProfileProperties;
            string value;

            return profileProperties.TryGetValue(propertyName, out value) ? value : string.Empty;
        }
    }
}