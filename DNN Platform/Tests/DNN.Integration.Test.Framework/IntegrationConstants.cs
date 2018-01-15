// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2018, DNN Corp.
// All rights reserved.

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