// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    public static class IntegrationConstants
    {
        public const string RuFirstName = "Registered";
        public const string RuLastName = "User";
        public const string RuUserName = RuFirstName + "." + RuLastName;
        public const string RuDisplayName = RuFirstName + " " + RuLastName;

        public const string AdminFirstName = "Admin";
        public const string AdminLastName = "User";
        public const string AdminUserName = AdminFirstName + "." + AdminLastName;
        public const string AdminDisplayName = AdminFirstName + " " + AdminLastName;

        public const int AllUsersRoleId = -1;
    }
}
