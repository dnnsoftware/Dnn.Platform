// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Build.Tasks
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Cake.Common.Diagnostics;
    using Cake.Common.IO;
    using Cake.Common.Xml;
    using Cake.Frosting;

    using Microsoft.Data.SqlClient;

    /// <summary>A cake task to crete a localdb database named <c>Dnn_Platform</c>.</summary>
    public sealed class CreateDatabase : FrostingTask<Context>
    {
        private string connectionString = @"server=(localdb)\MSSQLLocalDB";

        /// <inheritdoc/>
        public override void Run(Context context)
        {
            const string deleteScript = "if db_id('Dnn_Platform') is not null DROP DATABASE Dnn_Platform;";

            context.Information("Dropping LocalDb: {0}", this.ExecuteSqlScript(context, deleteScript));

            var createDbScript =
                $"""
                 CREATE DATABASE
                     [Dnn_Platform]
                 ON PRIMARY (
                    NAME=Dnn_data,
                    FILENAME = '{context.TempDir}\Dnn_Platform.mdf'
                 )
                 LOG ON (
                     NAME=Dnn_log,
                     FILENAME = '{context.TempDir}\Dnn_Platform.ldf'
                 )
                 """;
            var createDbStatus = this.ExecuteSqlScript(context, createDbScript);
            context.Information("Created LocalDb: {0}", createDbStatus);

            if (createDbStatus)
            {
                this.connectionString = @"server=(localdb)\MSSQLLocalDB;Database=Dnn_Platform;Trusted_Connection=True;";

                var schemaScriptName = context.XmlPeek(
                    "./Website/Install/DotNetNuke.install.config.resources",
                    "/dotnetnuke/scripts/script[@name='Schema']");
                var dataScriptName = context.XmlPeek(
                    "./Website/Install/DotNetNuke.install.config.resources",
                    "/dotnetnuke/scripts/script[@name='Data']");
                var schemaVersion = context.XmlPeek(
                    "./Website/Install/DotNetNuke.install.config.resources",
                    "/dotnetnuke/version");

                // #####################################################################
                // run initial schema first
                // #####################################################################
                var fileContents = System.IO.File.ReadAllText(
                    "./Website/Providers/DataProviders/SqlDataProvider/"
                    + schemaScriptName.ToString()
                    + ".SqlDataProvider");

                var sqlDelimiterRegex = new System.Text.RegularExpressions.Regex(
                    @"(?<=(?:[^\w]+|^))GO(?=(?: |\t)*?(?:\r?\n|$))",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
                string[] sqlStatements = sqlDelimiterRegex.Split(fileContents);
                foreach (string statement in sqlStatements)
                {
                    this.ExecuteSqlScript(context, statement);
                }

                context.Information("Initial Schema for v{0}", schemaVersion);

                // #####################################################################
                // populate with data next
                // #####################################################################
                fileContents = System.IO.File.ReadAllText(
                    "./Website/Providers/DataProviders/SqlDataProvider/"
                    + dataScriptName.ToString()
                    + ".SqlDataProvider");

                sqlStatements = sqlDelimiterRegex.Split(fileContents);
                foreach (string statement in sqlStatements)
                {
                    context.Information("Test Data: {1}", schemaVersion, this.ExecuteSqlScript(context, statement));
                }

                var createDummyPortalStatement =
                    "INSERT [dbo].[dnn_Portals] ([ExpiryDate], [UserRegistration], [BannerAdvertising], [AdministratorId], [Currency], [HostFee], [HostSpace], [AdministratorRoleId], [RegisteredRoleId], [GUID], [PaymentProcessor], [ProcessorUserId], [ProcessorPassword], [SiteLogHistory], [DefaultLanguage], [TimezoneOffset], [HomeDirectory], [PageQuota], [UserQuota], [CreatedByUserID], [CreatedOnDate], [LastModifiedByUserID], [LastModifiedOnDate], [PortalGroupID]) VALUES (NULL, 1, 0, 1, N'USD', 0.0000, 0, 0, 1, N'97debbc9-4643-4bd9-b0a0-b14170b38b0f', N'PayPal', NULL, NULL, 0, N'en-US', -8, N'Portals/0', 0, 0, -1, CAST(N'2015-02-05 14:49:37.873' AS DateTime), 1, CAST(N'2015-10-13 11:08:13.513' AS DateTime), -1)";

                context.Information(
                    "Test Portal: {1}",
                    schemaVersion,
                    this.ExecuteSqlScript(context, createDummyPortalStatement));

                // #####################################################################
                // now get all other SqlDataProvider files and run those....
                // #####################################################################
                var files = context.GetFiles("./Website/Providers/DataProviders/SqlDataProvider/*.SqlDataProvider");

                var currentFileToProcess = string.Empty;

                foreach (var file in files)
                {
                    currentFileToProcess = file.GetFilenameWithoutExtension()
                        .ToString();
                    var fileBits = currentFileToProcess.Split('.');

                    if (int.TryParse(fileBits[0], out var firstBit)
                        && int.TryParse(fileBits[1], out var secondBit)
                        && int.TryParse(fileBits[2], out var thirdBit))
                    {
                        var schemaVersionBits = schemaVersion.Split('.');

                        int schemaFirstBit = int.Parse(schemaVersionBits[0], CultureInfo.InvariantCulture);
                        int schemaSecondBit = int.Parse(schemaVersionBits[1], CultureInfo.InvariantCulture);
                        int schemaThirdBit = int.Parse(schemaVersionBits[2], CultureInfo.InvariantCulture);

                        if ((firstBit == schemaFirstBit && (secondBit >= schemaSecondBit && thirdBit >= schemaThirdBit))
                            || firstBit > schemaFirstBit)
                        {
                            context.Information("Updated to v{0}", currentFileToProcess);

                            fileContents = System.IO.File.ReadAllText(file.ToString());

                            sqlStatements = sqlDelimiterRegex.Split(fileContents);
                            foreach (string statement in sqlStatements)
                            {
                                var statementSuccess = this.ExecuteSqlScript(context, statement);
                            }
                        }
                    }
                }
            }
            else
            {
                context.Information("An Error has occured. Please review and try again.");
            }
        }

        private bool ExecuteSqlScript(Context context, string scriptStatement)
        {
            try
            {
                using (var connection = new SqlConnection(this.connectionString))
                {
                    connection.Open();

                    var cmdText = scriptStatement.Replace("{databaseOwner}", "dbo.").Replace("{objectQualifier}", "dnn_");
                    var command = new SqlCommand(cmdText, connection);
                    command.ExecuteNonQuery();

                    connection.Close();
                }
            }
            catch (Exception err)
            {
                context.Error(err);

                return false;
            }

            return true;
        }
    }
}
