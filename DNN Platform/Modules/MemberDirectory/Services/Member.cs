// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.MemberDirectory.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Tokens;

    public class Member
    {
        private UserInfo _user;
        private UserInfo _viewer;
        private PortalSettings _settings;

        public Member(UserInfo user, PortalSettings settings)
        {
            this._user = user;
            this._settings = settings;
            this._viewer = settings.UserInfo;
        }

        public int MemberId
        {
            get { return this._user.UserID; }
        }

        public string City
        {
            get { return this.GetProfileProperty("City"); }
        }

        public string Country
        {
            get { return this.GetProfileProperty("Country"); }
        }

        public string DisplayName
        {
            get { return this._user.DisplayName; }
        }

        public string Email
        {
            get { return this._viewer.IsInRole(this._settings.AdministratorRoleName) ? this._user.Email : string.Empty; }
        }

        public string FirstName
        {
            get { return this.GetProfileProperty("FirstName"); }
        }

        public int FollowerStatus
        {
            get { return (this._user.Social.Follower == null) ? 0 : (int)this._user.Social.Follower.Status; }
        }

        public int FollowingStatus
        {
            get { return (this._user.Social.Following == null) ? 0 : (int)this._user.Social.Following.Status; }
        }

        public int FriendId
        {
            get { return (this._user.Social.Friend == null) ? -1 : (int)this._user.Social.Friend.RelatedUserId; }
        }

        public int FriendStatus
        {
            get { return (this._user.Social.Friend == null) ? 0 : (int)this._user.Social.Friend.Status; }
        }

        public string LastName
        {
            get { return this.GetProfileProperty("LastName"); }
        }

        public string Phone
        {
            get { return this.GetProfileProperty("Telephone"); }
        }

        public string PhotoURL
        {
            get { return this._user.Profile.PhotoURL; }
        }

        public Dictionary<string, string> ProfileProperties
        {
            get
            {
                var properties = new Dictionary<string, string>();
                bool propertyNotFound = false;
                var propertyAccess = new ProfilePropertyAccess(this._user);
                foreach (ProfilePropertyDefinition property in this._user.Profile.ProfileProperties)
                {
                    string value = propertyAccess.GetProperty(
                        property.PropertyName,
                        string.Empty,
                        Thread.CurrentThread.CurrentUICulture,
                        this._viewer,
                        Scope.DefaultSettings,
                        ref propertyNotFound);

                    properties[property.PropertyName] = string.IsNullOrEmpty(value) ? string.Empty : Common.Utilities.HtmlUtils.Clean(HttpUtility.HtmlDecode(value), false);
                }

                return properties;
            }
        }

        public string Title
        {
            get { return this._user.Profile.Title; }
        }

        public string UserName
        {
            get { return this._viewer.IsInRole(this._settings.AdministratorRoleName) ? this._user.Username : string.Empty; }
        }

        public string Website
        {
            get { return this.GetProfileProperty("Website"); }
        }

        public string ProfileUrl
        {
            get { return Globals.UserProfileURL(this.MemberId); }
        }

        /// <summary>
        /// This method returns the value of the ProfileProperty if is defined, otherwise it returns an Empty string.
        /// </summary>
        /// <param name="propertyName">property name.</param>
        /// <returns>property value.</returns>
        private string GetProfileProperty(string propertyName)
        {
            var profileProperties = this.ProfileProperties;
            string value;

            return profileProperties.TryGetValue(propertyName, out value) ? value : string.Empty;
        }
    }
}
