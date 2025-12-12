// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.AzureConnector.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Data;
    using DotNetNuke.Services.Connections;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Services.Localization;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;

    /// <inheritdoc/>
    public class AzureConnector : IConnector
    {
        private const string DefaultDisplayName = "Azure Storage";
        private static readonly DataProvider DataProvider = DataProvider.Instance();

        private readonly IFolderMappingController folderMappingController;
        private string displayName;

        /// <summary>Initializes a new instance of the <see cref="AzureConnector"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IFolderMappingController. Scheduled removal in v12.0.0.")]
        public AzureConnector()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="AzureConnector"/> class.</summary>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        public AzureConnector(IFolderMappingController folderMappingController)
        {
            this.folderMappingController = folderMappingController ?? FolderMappingController.Instance;
        }

        /// <inheritdoc/>
        public string Name
        {
            get { return "Azure"; }
        }

        /// <inheritdoc/>
        public string IconUrl
        {
            get { return "~/DesktopModules/Connectors/Azure/Images/Azure.png"; }
        }

        /// <inheritdoc/>
        public string PluginFolder
        {
            get { return "~/DesktopModules/Connectors/Azure/"; }
        }

        /// <inheritdoc/>
        public bool IsEngageConnector
        {
            get
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public ConnectorCategories Type => ConnectorCategories.FileSystem;

        /// <inheritdoc/>
        public bool SupportsMultiple => true;

        /// <inheritdoc/>
        public string DisplayName
        {
            get
            {
                return
                    string.IsNullOrEmpty(this.displayName) ? DefaultDisplayName : this.displayName;
            }

            set
            {
                this.displayName = value;
            }
        }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public IEnumerable<IConnector> GetConnectors(int portalId)
        {
            var connectors = this.FindAzureFolderMappings(portalId);
            if (connectors != null && connectors.Any())
            {
                connectors.ForEach(x => { this.Id = x.FolderMappingID.ToString(); });
                var finalCon = connectors.Select(x => (IConnector)Activator.CreateInstance(this.GetType())).ToList();
                finalCon.ForEach(x =>
                {
                    x.Id = connectors[finalCon.IndexOf(x)].FolderMappingID.ToString();
                    x.DisplayName = connectors[finalCon.IndexOf(x)].MappingName;
                });
                return finalCon;
            }

            return new List<IConnector> { this };
        }

        /// <inheritdoc/>
        public void DeleteConnector(int portalId)
        {
            if (!string.IsNullOrEmpty(this.Id))
            {
                int mappingId;
                if (int.TryParse(this.Id, out mappingId))
                {
                    this.DeleteAzureFolders(portalId, mappingId);
                    this.DeleteAzureFolderMapping(portalId, mappingId);
                }
            }
        }

        /// <inheritdoc/>
        public bool HasConfig(int portalId)
        {
            var folderMapping = this.FindAzureFolderMapping(portalId, false, true);
            this.Id = Convert.ToString(folderMapping?.FolderMappingID);
            return this.GetConfig(portalId)["Connected"] == "true";
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetConfig(int portalId)
        {
            var configs = new Dictionary<string, string>();

            var folderMapping = this.FindAzureFolderMapping(portalId, false);

            var settings = folderMapping != null ? folderMapping.FolderMappingSettings : new Hashtable();

            configs.Add("AccountName", this.GetSetting(settings, Constants.AzureAccountName, true));
            configs.Add("AccountKey", this.GetSetting(settings, Constants.AzureAccountKey, true));
            configs.Add("Container", this.GetSetting(settings, Constants.AzureContainerName));
            configs.Add("Connected", !string.IsNullOrEmpty(this.GetSetting(settings, Constants.AzureAccountName)) && !string.IsNullOrEmpty(this.GetSetting(settings, Constants.AzureContainerName)) ? "true" : "false");

            // This setting will improve the UI to set password-type inputs on secure settings
            configs.Add("SecureSettings", "AccountKey");
            configs.Add("Id", Convert.ToString(folderMapping?.FolderMappingID));
            return configs;
        }

        /// <inheritdoc/>
        public bool SaveConfig(int portalId, IDictionary<string, string> values, ref bool validated, out string customErrorMessage)
        {
            customErrorMessage = string.Empty;
            var azureAccountName = values[Constants.AzureAccountName];
            var azureAccountKey = values[Constants.AzureAccountKey];
            var azureContainerName = values.ContainsKey(Constants.AzureContainerName) ? values[Constants.AzureContainerName] : string.Empty;

            var emptyFields = string.IsNullOrEmpty(azureAccountKey) && string.IsNullOrEmpty(azureAccountName);

            validated = true;
            if (emptyFields)
            {
                if (this.SupportsMultiple)
                {
                    throw new AzureConnectorException(Localization.GetString("ErrorRequiredFields", Constants.LocalResourceFile));
                }

                this.DeleteAzureFolderMapping(portalId);
                return true;
            }

            if (!this.Validation(azureAccountName, azureAccountKey, azureContainerName))
            {
                validated = false;
                return true;
            }

            if (this.FolderMappingNameExists(portalId, this.DisplayName, Convert.ToInt32(!string.IsNullOrEmpty(this.Id) ? this.Id : null)))
            {
                throw new AzureConnectorException(Localization.GetString("ErrorMappingNameExists", Constants.LocalResourceFile));
            }

            try
            {
                var folderMappings = this.FindAzureFolderMappings(portalId);
                FolderMappingInfo folderMapping;
                if (this.SupportsMultiple && !string.IsNullOrEmpty(this.Id))
                {
                    folderMapping = folderMappings.FirstOrDefault(x => x.FolderMappingID.ToString() == this.Id);
                }
                else
                {
                    folderMapping = this.FindAzureFolderMapping(portalId);
                }

                var settings = folderMapping.FolderMappingSettings;

                var savedAccount = this.GetSetting(settings, Constants.AzureAccountName, true);
                var savedKey = this.GetSetting(settings, Constants.AzureAccountKey, true);
                var savedContainer = this.GetSetting(settings, Constants.AzureContainerName);

                var accountChanged = savedAccount != azureAccountName || savedKey != azureAccountKey;

                if (accountChanged)
                {
                    this.DeleteAzureFolderMapping(portalId, folderMapping.FolderMappingID);
                    folderMapping = this.FindAzureFolderMapping(portalId);
                    settings = folderMapping.FolderMappingSettings;
                }
                else if (savedContainer != azureContainerName)
                {
                    this.DeleteAzureFolders(portalId, folderMapping.FolderMappingID);
                }

                var folderProvider = FolderProvider.Instance(Constants.FolderProviderType);

                settings[Constants.AzureAccountName] = folderProvider.EncryptValue(azureAccountName);
                settings[Constants.AzureAccountKey] = folderProvider.EncryptValue(azureAccountKey);

                if (values.ContainsKey(Constants.AzureContainerName) && !string.IsNullOrEmpty(values[Constants.AzureContainerName]))
                {
                    settings[Constants.AzureContainerName] = values[Constants.AzureContainerName];
                }
                else
                {
                    folderMapping.FolderMappingSettings[Constants.AzureContainerName] = string.Empty;
                }

                if (!folderMapping.FolderMappingSettings.ContainsKey(Constants.DirectLink))
                {
                    folderMapping.FolderMappingSettings[Constants.DirectLink] = "True";
                }

                if (!folderMapping.FolderMappingSettings.ContainsKey(Constants.UseHttps))
                {
                    folderMapping.FolderMappingSettings[Constants.UseHttps] = "True";
                }

                if (folderMapping.MappingName != this.DisplayName && !string.IsNullOrEmpty(this.DisplayName) &&
                   this.DisplayName != DefaultDisplayName)
                {
                    folderMapping.MappingName = this.DisplayName;
                }

                if (!folderMapping.FolderMappingSettings.ContainsKey(Constants.SyncBatchSize))
                {
                    folderMapping.FolderMappingSettings[Constants.SyncBatchSize] = Constants.DefaultSyncBatchSize.ToString();
                }

                this.folderMappingController.UpdateFolderMapping(folderMapping);

                return true;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }

        /// <summary>Find the folder mapping.</summary>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="folderMappingId">The folder mapping ID.</param>
        /// <param name="autoCreate">Whether to auto-create the folder mapping.</param>
        /// <returns>A <see cref="FolderMappingInfo"/> instance.</returns>
        internal static FolderMappingInfo FindAzureFolderMappingStatic(IFolderMappingController folderMappingController, int portalId, int? folderMappingId = null, bool autoCreate = true)
        {
            var folderMappings = folderMappingController.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType);

            if (folderMappingId != null)
            {
                return folderMappings.FirstOrDefault(x => x.FolderMappingID == folderMappingId);
            }

            if (!folderMappings.Any() && autoCreate)
            {
                return CreateAzureFolderMappingStatic(folderMappingController, portalId);
            }

            return folderMappings.FirstOrDefault();
        }

        private static FolderMappingInfo CreateAzureFolderMappingStatic(IFolderMappingController folderMappingController, int portalId, string mappingName = "")
        {
            var folderMapping = new FolderMappingInfo
            {
                PortalID = portalId,
                MappingName =
                    string.IsNullOrEmpty(mappingName)
                        ? $"{DefaultDisplayName}_{DateTime.Now.Ticks}"
                        : mappingName,
                FolderProviderType = Constants.FolderProviderType,
            };
            folderMapping.FolderMappingID = folderMappingController.AddFolderMapping(folderMapping);
            return folderMapping;
        }

        private void DeleteAzureFolderMapping(int portalId, int folderMappingId)
        {
            this.folderMappingController.DeleteFolderMapping(portalId, folderMappingId);
        }

        private void DeleteAzureFolderMapping(int portalId)
        {
            var folderMapping = this.folderMappingController.GetFolderMappings(portalId)
                .FirstOrDefault(f => f.FolderProviderType == Constants.FolderProviderType);

            if (folderMapping != null)
            {
                this.folderMappingController.DeleteFolderMapping(portalId, folderMapping.FolderMappingID);
            }
        }

        private void DeleteAzureFolders(int portalId, int folderMappingId)
        {
            var folderManager = FolderManager.Instance;
            var folders = folderManager.GetFolders(portalId);

            var folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingId);

            if (folderMappingFolders.Any())
            {
                // Delete files in folders with the provided mapping (only in the database)
                foreach (
                    var file in
                    folderMappingFolders.Select<IFolderInfo, IEnumerable<IFileInfo>>(folderManager.GetFiles)
                        .SelectMany(files => files))
                {
                    DataProvider.DeleteFile(portalId, file.FileName, file.FolderId);
                }

                // Remove the folders with the provided mapping that doesn't have child folders with other mapping (only in the database and filesystem)
                var folders1 = folders; // copy the variable to not access a modified closure
                var removableFolders =
                    folders.Where(
                        f => f.FolderMappingID == folderMappingId && !folders1.Any(f2 => f2.FolderID != f.FolderID &&
                                                                                         f2.FolderPath.StartsWith(
                                                                                             f.FolderPath) &&
                                                                                         f2.FolderMappingID !=
                                                                                         folderMappingId));

                if (removableFolders.Count() > 0)
                {
                    foreach (var removableFolder in removableFolders.OrderByDescending(rf => rf.FolderPath))
                    {
                        DirectoryWrapper.Instance.Delete(removableFolder.PhysicalPath, false);
                        DataProvider.DeleteFolder(portalId, removableFolder.FolderPath);
                    }
                }

                // Update the rest of folders with the provided mapping to use the standard mapping
                folders = folderManager.GetFolders(portalId, false); // re-fetch the folders

                folderMappingFolders = folders.Where(f => f.FolderMappingID == folderMappingId);

                if (folderMappingFolders.Count() > 0)
                {
                    var defaultFolderMapping = this.folderMappingController.GetDefaultFolderMapping(portalId);

                    foreach (var folderMappingFolder in folderMappingFolders)
                    {
                        folderMappingFolder.FolderMappingID = defaultFolderMapping.FolderMappingID;
                        folderManager.UpdateFolder(folderMappingFolder);
                    }
                }
            }
        }

        private bool Validation(string azureAccountName, string azureAccountKey, string azureContainerName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(azureAccountName))
                {
                    throw new ConnectorArgumentException(Localization.GetString(
                        "AccountNameCannotBeEmpty.ErrorMessage",
                        Constants.LocalResourceFile));
                }

                if (string.IsNullOrWhiteSpace(azureAccountKey))
                {
                    throw new ConnectorArgumentException(Localization.GetString(
                        "AccountKeyCannotBeEmpty.ErrorMessage",
                        Constants.LocalResourceFile));
                }

                StorageCredentials sc = new StorageCredentials(azureAccountName, azureAccountKey);
                var csa = new CloudStorageAccount(sc, true);
                var blobClient = csa.CreateCloudBlobClient();

                blobClient.DefaultRequestOptions.RetryPolicy = new NoRetry();

                var containers = blobClient.ListContainers();
                if (containers.Any())
                {
                    if (!string.IsNullOrEmpty(azureContainerName))
                    {
                        if (!containers.Any(container => container.Name == azureContainerName))
                        {
                            throw new AzureConnectorException(Localization.GetString("ErrorInvalidContainerName", Constants.LocalResourceFile));
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    throw new AzureConnectorException(Localization.GetString("AccountNotFound.ErrorMessage", Constants.LocalResourceFile));
                }
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "AccountNotFound")
                    {
                        throw new AzureConnectorException(Localization.GetString("AccountNotFound.ErrorMessage", Constants.LocalResourceFile), ex);
                    }
                    else if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode == "AccessDenied")
                    {
                        throw new AzureConnectorException(Localization.GetString("AccessDenied.ErrorMessage", Constants.LocalResourceFile), ex);
                    }
                    else
                    {
                        throw new AzureConnectorException(ex.RequestInformation.HttpStatusMessage, ex);
                    }
                }

                throw new AzureConnectorException(ex.RequestInformation.HttpStatusMessage ?? ex.Message, ex);
            }
            catch (FormatException ex)
            {
                if (ex.GetType() == typeof(UriFormatException))
                {
                    throw new ConnectorArgumentException(Localization.GetString("InvalidAccountName.ErrorMessage", Constants.LocalResourceFile));
                }

                throw new ConnectorArgumentException(Localization.GetString("InvalidAccountKey.ErrorMessage", Constants.LocalResourceFile));
            }
        }

        private string GetSetting(Hashtable settings, string name, bool encrypt = false)
        {
            if (!settings.ContainsKey(name))
            {
                return string.Empty;
            }

            if (encrypt)
            {
                var folderProvider = FolderProvider.Instance(Constants.FolderProviderType);
                return folderProvider.GetEncryptedSetting(settings, name);
            }

            return settings[name].ToString();
        }

        private FolderMappingInfo FindAzureFolderMapping(int portalId, bool autoCreate = true, bool checkId = false)
        {
            var folderMappings = this.folderMappingController.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType).ToList();

            // Create new mapping if none is found.
            if (!folderMappings.Any() && autoCreate)
            {
                return this.CreateAzureFolderMapping(portalId, DefaultDisplayName);
            }

            if ((checkId && string.IsNullOrEmpty(this.Id)) || !this.SupportsMultiple)
            {
                return folderMappings.FirstOrDefault();
            }

            var folderMapping = folderMappings.FirstOrDefault(x => x.FolderMappingID.ToString() == this.Id);

            if (folderMapping == null && autoCreate)
            {
                folderMapping = this.CreateAzureFolderMapping(portalId);
            }

            return folderMapping;
        }

        private IList<FolderMappingInfo> FindAzureFolderMappings(int portalId)
        {
            return this.folderMappingController.GetFolderMappings(portalId)
                .Where(f => f.FolderProviderType == Constants.FolderProviderType).ToList();
        }

        private bool FolderMappingNameExists(int portalId, string mappingName, int? exceptMappingId)
        {
            return this.folderMappingController.GetFolderMappings(portalId)
                .Any(
                    f =>
                        f.MappingName.ToLowerInvariant() == mappingName.ToLowerInvariant() &&
                        (f.FolderMappingID != exceptMappingId));
        }

        private FolderMappingInfo CreateAzureFolderMapping(int portalId, string mappingName = "")
        {
            var folderMapping = CreateAzureFolderMappingStatic(this.folderMappingController, portalId, mappingName);
            this.Id = folderMapping.FolderMappingID.ToString();
            return folderMapping;
        }
    }
}
