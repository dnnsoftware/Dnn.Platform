// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Prompt.Models
{
    using System.Diagnostics.CodeAnalysis;

    using Dnn.PersonaBar.Library.Prompt.Common;
    using DotNetNuke.Entities.Users;

    public class UserModelBase
    {
        // provide a default field order for use of callers
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public static readonly string[] FieldOrder =
        [
            "UserId",
            "Username",
            "Email",
            "LastLogin",
            "IsDeleted",
            "IsAuthorized",
            "IsLockedOut"
        ];

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

        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        public string __Email => $"get-user '{this.Email}'";

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        public string __UserId => $"get-user {this.UserId}";

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        public string __Username => $"get-user '{this.Username}'";

        // ReSharper restore InconsistentNaming
        public int UserId { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string LastLogin { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsAuthorized { get; set; }

        public bool IsLockedOut { get; set; }
    }
}
