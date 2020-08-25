// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Integration.Test.Framework
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Threading;

    using DNN.Integration.Test.Framework.Helpers;
    using NUnit.Framework;

    public abstract class IntegrationTestBase
    {
        // public static string DatabaseName { get; }
        static IntegrationTestBase()
        {
            ServicePointManager.Expect100Continue = false;

            // setup of the whole system: take a snapshot and keep using it for all tests
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            ConnectionString = AppConfigHelper.ConnectionString;

            // DatabaseName = GetDbNameFromConnectionString(ConnectionString);

            // SchedulerController.DisableAllSchedulers(false);
            // SchedulerController.DisableAppStartDelay(false);
        }

        public static string ConnectionString { get; }

        public static void LogText(string text)
        {
            // Don't write anything to console when we run in TeamCity
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
            {
                Console.WriteLine(text);
            }
        }

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
