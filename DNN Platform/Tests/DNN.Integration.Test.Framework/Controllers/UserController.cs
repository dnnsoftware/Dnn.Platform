// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework.Controllers
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DNN.Integration.Test.Framework.Helpers;
    using DNN.Integration.Test.Framework.Scripts;

    public static class UserController
    {
        private const string PortalIdMarker = @"'$[portal_id]'";
        private const string UserToCopyMarker = @"$[users_to_copy]";

        private const string FirstNameMarker = @"$[first_name]";
        private const string LastNameMarker = @"$[last_name]";
        private const string IsSuperUserMarker = @"'$[isSuperUser]'";
        private const string RoleMarker = @"$[role_name]";
        private const string UserIdMarker = @"'$[user_id]'";
        private const string CreateFriendsMarker = @"'$[CreateFriends]'";
        private const string CreateFollowersMarker = @"'$[CreateFollowers]'";
        private static readonly string UserToCopy = AppConfigHelper.HostUserName;

        /// <summary>
        /// Creates a new user with Registered User Role and with the same password as the host user.
        /// </summary>
        /// <param name="firstName">First name of the user.</param>
        /// <param name="lastName">Last name of the user.</param>
        /// <param name="portalId">PortalID to create the user under.</param>
        /// <returns>Id of the created user.</returns>
        /// <remarks>Username created is firstname.lastname.</remarks>
        public static int CreateRegisteredUser(
            string firstName = IntegrationConstants.RuFirstName,
            string lastName = IntegrationConstants.RuLastName, int portalId = 0)
        {
            return CreateUser(new CreateUserParams { FirstName = firstName, LastName = lastName, PortalId = portalId });
        }

        /// <summary>
        /// Creates a new SuperUser and with the same password as the host user.
        /// </summary>
        /// <param name="firstName">First name of the user.</param>
        /// <param name="lastName">Last name of the user.</param>
        /// <param name="portalId">PortalID to create the user under.</param>
        /// <returns>Id of the created user.</returns>
        /// <remarks>Username created is firstname.lastname.</remarks>
        public static int CreateSuperUser(string firstName, string lastName, int portalId = 0)
        {
            return CreateUser(new CreateUserParams { FirstName = firstName, LastName = lastName, PortalId = portalId, SuperUser = true });
        }

        /// <summary>
        /// Creates a new user with Administrator Role and with the same password as the host user.
        /// </summary>
        /// <param name="firstName">First name of the administrator user.</param>
        /// <param name="lastName">Last name of the administrator user.</param>
        /// <param name="portalId">PortalID to create the user under.</param>
        /// <returns>Id of the created user.</returns>
        /// <remarks>Username created is firstname.lastname.</remarks>
        public static int CreateAdministratorUser(
            string firstName = IntegrationConstants.AdminFirstName,
            string lastName = IntegrationConstants.AdminLastName, int portalId = 0)
        {
            return CreateUser(new CreateUserParams { FirstName = firstName, LastName = lastName, PortalId = portalId, Role = "Administrators" });
        }

        public static int GetUserId(string username)
        {
            var results = DatabaseHelper.ExecuteStoredProcedure("GetUserByUsername", 0, username.Replace("'", string.Empty));
            return results.SelectMany(x => x.Where(y => y.Key == "UserId").Select(y => (int)y.Value)).FirstOrDefault();
        }

        public static void DeleteUser(int userId)
        {
            var fileContent = SqlScripts.UserDelete;
            var script = new StringBuilder(fileContent)
                .Replace(UserIdMarker, userId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            DatabaseHelper.ExecuteQuery(script);
        }

        public static TimeZoneInfo GetUserPreferredTimeZone(int userId, int portalId)
        {
            var fileContent = SqlScripts.UserGetPreferredTimeZone;
            var script = new StringBuilder(fileContent)
                .Replace(UserIdMarker, userId.ToString(CultureInfo.InvariantCulture))
                .Replace(PortalIdMarker, portalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            var timeZoneString = DatabaseHelper.ExecuteQuery(script).First()["PropertyValue"].ToString();

            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneString);
        }

        private static int CreateUser(CreateUserParams parms)
        {
            parms.FirstName = parms.FirstName.Replace("'", "''");
            parms.LastName = parms.LastName.Replace("'", "''");
            var username = parms.FirstName + "." + parms.LastName;
            var uid = GetUserId(username);
            if (uid > 0)
            {
                return uid;
            }

            // create role if not present
            RoleController.CreateRoleIfNotPresent(parms.Role);

            var fileContent = SqlScripts.SingleUserCreation;
            var masterScript = new StringBuilder(fileContent)
                .Replace(UserToCopyMarker, UserToCopy)
                .Replace(PortalIdMarker, parms.PortalId.ToString(CultureInfo.InvariantCulture))
                .Replace("{objectQualifier}", AppConfigHelper.ObjectQualifier)
                .ToString();

            // make sure to create at least one SU (see comments at end of files below)
            // the create super-users is proportional to number of total users
            var script = new StringBuilder(masterScript)
                .Replace(FirstNameMarker, parms.FirstName.Replace("'", "''"))
                .Replace(LastNameMarker, parms.LastName.Replace("'", "''"))
                .Replace(IsSuperUserMarker, parms.SuperUser ? "1" : "0")
                .Replace(CreateFriendsMarker, parms.AutoCreateFriends ? "1" : "0")
                .Replace(CreateFollowersMarker, parms.AutoCreateFollowersAndFollowings ? "1" : "0")
                .Replace(RoleMarker, string.IsNullOrEmpty(parms.Role) ? string.Empty : parms.Role);
            DatabaseHelper.ExecuteQuery(script.ToString());
            return GetUserId(username);
        }

        public class CreateUserParams
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int PortalId { get; set; }

            public bool SuperUser { get; set; }

            public string Role { get; set; }

            public bool AutoCreateFriends { get; set; }

            public bool AutoCreateFollowersAndFollowings { get; set; }
        }
    }
}
