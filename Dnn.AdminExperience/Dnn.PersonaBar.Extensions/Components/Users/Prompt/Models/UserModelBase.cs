// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Models
{
    using Dnn.PersonaBar.Library.Prompt.Common;
    using DotNetNuke.Entities.Users;

    public class UserModelBase
    {
        // provide a default field order for use of callers
        public static string[] FieldOrder =
        {
            "UserId",
            "Username",
            "Email",
            "LastLogin",
            "IsDeleted",
            "IsAuthorized",
            "IsLockedOut"
        };

        public UserModelBase()
        {
        }

        public UserModelBase(UserInfo userInfo)
        {
            this.Email = userInfo.Email;
            this.LastLogin = userInfo.Membership.LastLoginDate.ToPromptShortDateString();
            this.UserId = userInfo.UserID;
            this.Username = userInfo.Username;
            this.IsDeleted = userInfo.IsDeleted;
            this.IsAuthorized = userInfo.Membership.Approved;
            this.IsLockedOut = userInfo.Membership.LockedOut;
        }

        public string __Email => $"get-user '{this.Email}'";
        public string __UserId => $"get-user {this.UserId}";
        public string __Username => $"get-user '{this.Username}'";
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string LastLogin { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsLockedOut { get; set; }
    }
}
