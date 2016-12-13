// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017, DNN Corp.
// All Rights Reserved

using System;
using System.Globalization;
using System.Threading;
using DotNetNuke.Tests.Integration.Framework.Helpers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Framework
{
    public abstract class IntegrationTestBase
    {
        public static string ConnectionString { get; }
        //public static string DatabaseName { get; }

        #region static constructor

        static IntegrationTestBase()
        {
            // setup of the whole system: take a snapshot and keep using it for all tests
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            ConnectionString = AppConfigHelper.ConnectionString;
            //DatabaseName = GetDbNameFromConnectionString(ConnectionString);

            //SchedulerController.DisableAllSchedulers(false);
            //SchedulerController.DisableAppStartDelay(false);
        }

        #endregion

        [TestFixtureSetUp]
        public virtual void TestFixtureSetUp()
        {
        }

        [TestFixtureTearDown]
        public virtual void TestFixtureTearDown()
        {
        }

        private static string GetDbNameFromConnectionString(string connectionString)
        {
            var builder = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = connectionString };
            return builder["Database"] as string;
        }
    }
}
