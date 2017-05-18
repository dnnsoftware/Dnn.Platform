// DotNetNuke® - http://www.dnnsoftware.com
//
// Copyright (c) 2002-2017, DNN Corp.
// All rights reserved.

using System;

namespace DotNetNuke.Tests.Integration.Framework
{
    public static class EvoqConstants
    {
        public static readonly TimeSpan ThreeSeconds = TimeSpan.FromSeconds(3);

        public const string ConMgrFirstName = "Content";
        public const string ConMgrLastName = "Manager";
        public static string ConMgrUserName = ConMgrFirstName + "." + ConMgrLastName;
        public static string ConMgrDisplayName = ConMgrFirstName + " " + ConMgrLastName;
        public const string ConEdtFirstName = "Content";
        public const string ConEdtLastName = "Editor";
        public static string ConEdtUserName = ConEdtFirstName + "." + ConEdtLastName;
        public static string ConEdtDisplayName = ConEdtFirstName + " " + ConEdtLastName;
        public const string AdministratorFirstName = "Admin";
        public const string AdministratorLastName = "User";
        public const string AdministratorUserName = AdministratorFirstName + "." + AdministratorLastName;
        public const string AdministratorDisplayName = AdministratorFirstName + " " + AdministratorLastName;
        public const string AnonymousDisplayName = "Anonymous";
        public const string CmxFirstName = "Community";
        public const string CmxLastName = "Manager";
        public static string CmxUserName = CmxFirstName + "." + CmxLastName;
        public static string CmxDisplayName = CmxFirstName + " " + CmxLastName;
       
    }
}