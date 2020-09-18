// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Models
{
    using Dnn.PersonaBar.Library.Prompt.Common;
    using DotNetNuke.Entities.Users;

    public class UserModel : UserModelBase
    {
        // provide a default field order for use of callers
        public static new string[] FieldOrder =
        {
            "UserId",
            "Username",
            "DisplayName",
            "FirstName",
            "LastName",
            "Email",
            "LastActivity",
            "LastLogin",
            "LastLockout",
            "LastPasswordChange",
            "IsDeleted",
            "IsAuthorized",
            "IsLockedOut",
            "Created"
        };

        public UserModel()
        {
        }

        public UserModel(UserInfo user) : base(user)
        {
            this.LastLogin = user.Membership.LastLoginDate.ToPromptLongDateString();
            this.DisplayName = user.DisplayName;
            this.FirstName = user.FirstName;
            this.LastName = user.LastName;
            this.LastActivity = user.Membership.LastActivityDate.ToPromptLongDateString();
            this.LastLockout = user.Membership.LastLockoutDate.ToPromptLongDateString();
            this.LastPasswordChange = user.Membership.LastPasswordChangeDate.ToPromptLongDateString();
            this.Created = user.CreatedOnDate.ToPromptLongDateString();
        }

        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LastActivity { get; set; }
        public string LastLockout { get; set; }
        public string LastPasswordChange { get; set; }
        public string Created { get; set; }
    }
}
