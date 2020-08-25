// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Users
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public interface IUserInfo
    {
        int AffiliateID { get; set; }
        //CacheLevel Cacheability { get; }
        string DisplayName { get; set; }
        string Email { get; set; }
        string FirstName { get; set; }
        bool HasAgreedToTerms { get; set; }
        DateTime HasAgreedToTermsOn { get; set; }
        bool IsAdmin { get; }
        bool IsDeleted { get; set; }
        bool IsSuperUser { get; set; }
        string LastIPAddress { get; set; }
        string LastName { get; set; }
        //UserMembership Membership { get; set; }
        DateTime PasswordResetExpiration { get; set; }
        Guid PasswordResetToken { get; set; }
        int PortalID { get; set; }
        //UserProfile Profile { get; set; }
        bool RequestsRemoval { get; set; }
        string[] Roles { get; set; }
        //UserSocial Social { get; }
        int UserID { get; set; }
        string Username { get; set; }
        string VanityUrl { get; set; }

        //string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope currentScope, ref bool propertyNotFound);
        bool IsInRole(string role);
        DateTime LocalTime();
        DateTime LocalTime(DateTime utcTime);
        void UpdateDisplayName(string format);
    }
}
