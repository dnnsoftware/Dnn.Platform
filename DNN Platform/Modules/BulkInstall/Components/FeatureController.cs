// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.BulkInstall.Encryption;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;

    /// <summary>The Controller class for PolyDeploy.</summary>
    public class FeatureController(IHostSettings hostSettings) : IUpgradeable
    {
        private readonly IHostSettings hostSettings = hostSettings;

        /// <summary>UpgradeModule implements the IUpgradeable Interface.</summary>
        /// <param name="version">The current version of the module.</param>
        /// <returns>A message to log.</returns>
        public string UpgradeModule(string version)
        {
            string result;

            // Determine if we need to run this upgrade logic or if it's already been run.
            bool shouldRun;

            using (IDataContext context = DataContext.Instance(this.hostSettings))
            {
                // See if there is a post-upgrade stored procedure for this version.
                shouldRun = context.ExecuteSingleOrDefault<bool>(
                    System.Data.CommandType.StoredProcedure,
                    $"{{databaseOwner}}[{{objectQualifier}}Cantarus_PolyDeploy_SpExists] '{{databaseOwner}}[{{objectQualifier}}Cantarus_PolyDeploy_PostUpgrade_{version}]'");
            }

            // Should upgrade logic be run?
            if (shouldRun)
            {
                // Yes.
                result = $"Upgrade logic for {version} completed.";

                // Execute appropriate logic.
                switch (version)
                {
                    case "00.09.00":
                        this.Upgrade_00_09_00();
                        break;

                    case "00.09.01":
                        this.Upgrade_00_09_01();
                        break;

                    default:
                        result = $"No upgrade logic for {version}.";
                        break;
                }

                // Clean up and make sure we don't run this logic again.
                using IDataContext context = DataContext.Instance(this.hostSettings);

                // Upgrade complete, execute post-upgrade stored procedure.
                context.Execute(System.Data.CommandType.StoredProcedure, $"{{databaseOwner}}[{{objectQualifier}}Cantarus_PolyDeploy_PostUpgrade_{version}]");

                // Then drop it.
                context.Execute(System.Data.CommandType.Text, $"DROP PROCEDURE {{databaseOwner}}[{{objectQualifier}}Cantarus_PolyDeploy_PostUpgrade_{version}]");
            }
            else
            {
                // No.
                result = $"Upgrade logic for {version} has been run previously.";
            }

            return result;
        }

        /// <summary>
        /// Upgrades to 00.09.00
        ///
        /// Operations:
        /// - Generate a Salt.
        /// - Hash existing APIKeys using the new Salt.
        /// - Encrypt existing EncryptionKeys using plain text APIKey.
        /// - Insert in to new table.
        /// </summary>
        private void Upgrade_00_09_00()
        {
            string oldTableName = "{databaseOwner}[{objectQualifier}Cantarus_PolyDeploy_APIUsers_PreEncryption]";
            string newTableName = "{databaseOwner}[{objectQualifier}Cantarus_PolyDeploy_APIUsers]";

            using IDataContext context = DataContext.Instance(this.hostSettings);

            // Get all existing api user ids.
            IEnumerable<int> apiUserIds = context.ExecuteQuery<int>(System.Data.CommandType.Text, $"SELECT [APIUserID] FROM {oldTableName}");

            foreach (int apiUserId in apiUserIds)
            {
                // Read old data.
                string auName = context.ExecuteQuery<string>(System.Data.CommandType.Text, $"SELECT [Name] FROM {oldTableName} WHERE APIUserID = @0", apiUserId).FirstOrDefault();
                string auApiKey = context.ExecuteQuery<string>(System.Data.CommandType.Text, $"SELECT [APIKey] FROM {oldTableName} WHERE APIUserID = @0", apiUserId).FirstOrDefault();
                string auEncryptionKey = context.ExecuteQuery<string>(System.Data.CommandType.Text, $"SELECT [EncryptionKey] FROM {oldTableName} WHERE APIUserID = @0", apiUserId).FirstOrDefault();
                bool auBypass = context.ExecuteQuery<bool>(System.Data.CommandType.Text, $"SELECT [BypassIPWhitelist] FROM {oldTableName} WHERE APIUserID = @0", apiUserId).FirstOrDefault();

                // Generate a salt.
                string auSalt = APIUser.GenerateSalt();

                // Use existing plain text api key and salt to create a hashed api key.
                string auApiKeySha = APIUser.GenerateHash(auApiKey, auSalt);

                // Encrypt existing plain text encryption key and store in new field.
                string auEncryptionKeyEnc = Crypto.Encrypt(auEncryptionKey, auApiKey);

                // Insert in to new table.
                string insertSql =
                    $"""
                     SET IDENTITY_INSERT {newTableName} ON;
                     INSERT INTO {newTableName} ([APIUserID], [Name], [APIKey_Sha], [EncryptionKey_Enc], [Salt], [BypassIPWhitelist])
                     VALUES (@0, @1, @2, @3, @4, @5);
                     SET IDENTITY_INSERT {newTableName} OFF;
                     """;

                context.Execute(System.Data.CommandType.Text, insertSql, apiUserId, auName, auApiKeySha, auEncryptionKeyEnc, auSalt, auBypass);
            }
        }

        /// <summary>
        /// Upgrades to 00.09.01
        ///
        /// Operations:
        /// - Generate a Salt.
        /// - Hash existing Address' using the new Salt.
        /// - Insert in to new table.
        /// </summary>
        private void Upgrade_00_09_01()
        {
            string oldTableName = "{databaseOwner}[{objectQualifier}Cantarus_PolyDeploy_IPSpecs_PreEncryption]";
            string newTableName = "{databaseOwner}[{objectQualifier}Cantarus_PolyDeploy_IPSpecs]";

            using IDataContext context = DataContext.Instance(this.hostSettings);

            // Get all existing IPSpec ids.
            IEnumerable<int> ipSpecIds = context.ExecuteQuery<int>(System.Data.CommandType.Text, $"SELECT [IPSpecID] FROM {oldTableName}");

            foreach (int ipSpecId in ipSpecIds)
            {
                // Read old data.
                string isAddress = context.ExecuteQuery<string>(
                    System.Data.CommandType.Text,
                    $"SELECT [Address] FROM {oldTableName} WHERE IPSpecID = @0",
                    ipSpecId)
                .FirstOrDefault();

                // Create a name.
                string isName = $"Unnamed#{ipSpecId}";

                // Generate a salt.
                string isSalt = IPSpec.GenerateSalt();

                // Use existing plain text address and salt to create a hashed address.
                string isAddressSha = IPSpec.GenerateHash(isAddress, isSalt);

                // Insert in to new table.
                string insertSql =
                    $"""
                     SET IDENTITY_INSERT {newTableName} ON;
                     INSERT INTO {newTableName} ([IPSpecID], [Name], [Address_Sha], [Salt])
                     VALUES (@0, @1, @2, @3);
                     SET IDENTITY_INSERT {newTableName} OFF;
                     """;

                context.Execute(System.Data.CommandType.Text, insertSql, ipSpecId, isName, isAddressSha, isSalt);
            }
        }
    }
}
