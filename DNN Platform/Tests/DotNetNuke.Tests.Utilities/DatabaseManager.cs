#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections.Specialized;
using System.IO;
using System.Threading;

using Microsoft.SqlServer.Management.Smo;

using NUnit.Framework;

namespace DotNetNuke.Tests.Utilities
{
    public static class DatabaseManager
    {
        #region Private Methods

        private static void AttachDatabase(string databaseName, string databaseFile)
        {
            // Connect to the SQL Server
            var server = new Server("(local)");

            // Attach the database
            server.AttachDatabase(databaseName, new StringCollection { databaseFile });
            while (server.Databases[databaseName].State != SqlSmoState.Existing)
            {
                Thread.Sleep(100);
            }
        }

        private static string CopyDatabase(TestContext context, string databaseName)
        {
            // Find the target database file
            string targetDatabasePath = EnsureDatabaseExists(databaseName);

            // Create the test database directory if it does not exist
            string testDatabaseDirectory = Path.GetFullPath(String.Format(@"Databases\TestDatabases\{0}", context.Test.FullName));
            if (!Directory.Exists(testDatabaseDirectory))
            {
                Directory.CreateDirectory(testDatabaseDirectory);
            }

            // Copy the database to the test database directory
            string destinationRoot = Path.Combine(testDatabaseDirectory, databaseName);
            string databasePath = String.Concat(destinationRoot, ".mdf");
            File.Copy(targetDatabasePath, databasePath);
            return databasePath;
        }

        #endregion

        #region Public Methods

        public static void CopyAndAttachDatabase(TestContext context, string databaseName)
        {
            string destinationPath = CopyDatabase(context, databaseName);

            // Attach the copied database to SQL Server
            AttachDatabase(databaseName, destinationPath);
        }

        public static void DropDatabase(string databaseName)
        {
            // Connect to the SQL Server
            var server = new Server("(local)");

            // Drop the database
            Database db = server.Databases[databaseName];
            if (db != null)
            {
                server.KillDatabase(databaseName);
                while (server.Databases[databaseName] != null)
                {
                    Thread.Sleep(100);
                }
            }
        }

        public static string EnsureDatabaseExists(string databaseName)
        {
            string targetDatabasePath = GetTestDatabasePath(databaseName);
            if (!File.Exists(targetDatabasePath))
            {
                throw new InvalidOperationException(String.Format("Could not find test database {0}. Searched: {1}", databaseName, targetDatabasePath));
            }
            return targetDatabasePath;
        }

        public static string GetTestDatabasePath(string databaseName)
        {
            return Path.GetFullPath(String.Format(@"Databases\{0}.mdf", databaseName));
        }

        #endregion
    }
}