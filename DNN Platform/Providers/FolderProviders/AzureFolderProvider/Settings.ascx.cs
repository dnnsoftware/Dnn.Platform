// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Windows Azure Storage Settings Control.
    /// </summary>
    public partial class Settings : FolderMappingSettingsControlBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Settings));

        /// <summary>
        /// Loads concrete settings.
        /// </summary>
        /// <param name="folderMappingSettings">The Hashtable containing the folder mapping settings.</param>
        public override void LoadSettings(Hashtable folderMappingSettings)
        {
            var folderProvider = FolderProvider.Instance(Constants.FolderProviderType);
            if (folderMappingSettings.ContainsKey(Constants.AccountName))
            {
                this.tbAccountName.Text = folderProvider.GetEncryptedSetting(folderMappingSettings, Constants.AccountName);
            }

            if (folderMappingSettings.ContainsKey(Constants.AccountKey))
            {
                this.tbAccountKey.Text = folderProvider.GetEncryptedSetting(folderMappingSettings, Constants.AccountKey);
            }

            if (this.tbAccountName.Text.Length > 0 && this.tbAccountKey.Text.Length > 0)
            {
                var bucketName = string.Empty;

                if (folderMappingSettings.ContainsKey(Constants.Container))
                {
                    bucketName = folderMappingSettings[Constants.Container].ToString();
                }

                this.LoadContainers();

                var bucket = this.ddlContainers.Items.FindByText(bucketName);

                if (bucket != null)
                {
                    this.ddlContainers.SelectedIndex = this.ddlContainers.Items.IndexOf(bucket);
                }
            }

            if (folderMappingSettings.ContainsKey(Constants.UseHttps))
            {
                this.chkUseHttps.Checked = bool.Parse(folderMappingSettings[Constants.UseHttps].ToString());
            }

            this.chkDirectLink.Checked = !folderMappingSettings.ContainsKey(Constants.DirectLink) || folderMappingSettings[Constants.DirectLink].ToString().ToLowerInvariant() == "true";

            if (folderMappingSettings.ContainsKey(Constants.CustomDomain))
            {
                this.tbCustomDomain.Text = folderProvider.GetEncryptedSetting(folderMappingSettings, Constants.CustomDomain);
            }

            if (folderMappingSettings.ContainsKey(Constants.SyncBatchSize) && folderMappingSettings[Constants.SyncBatchSize] != null)
            {
                this.tbSyncBatchSize.Text = folderMappingSettings[Constants.SyncBatchSize].ToString();
            }
            else
            {
                this.tbSyncBatchSize.Text = Constants.DefaultSyncBatchSize.ToString();
            }
        }

        /// <summary>
        /// Updates concrete settings for the specified folder mapping.
        /// </summary>
        /// <param name="folderMappingID">The folder mapping identifier.</param>
        public override void UpdateSettings(int folderMappingID)
        {
            this.Page.Validate();

            if (!this.Page.IsValid)
            {
                throw new Exception();
            }

            var folderMappingController = FolderMappingController.Instance;
            var folderMapping = folderMappingController.GetFolderMapping(folderMappingID);

            var accountName = this.GetAccountName();
            var accountKey = this.GetAccountKey();
            var container = this.GetContainer();
            var useHttps = this.GetUseHttps();
            var customDomain = this.GetCustomDomain();
            var synchBatchSize = this.GetSynchBatchSize();

            if (AreThereFolderMappingsWithSameSettings(folderMapping, accountName, container))
            {
                this.valContainerName.ErrorMessage = Localization.GetString("MultipleFolderMappingsWithSameSettings.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;

                throw new Exception();
            }

            folderMapping.FolderMappingSettings[Constants.AccountName] = accountName;
            folderMapping.FolderMappingSettings[Constants.AccountKey] = accountKey;
            folderMapping.FolderMappingSettings[Constants.Container] = container;
            folderMapping.FolderMappingSettings[Constants.UseHttps] = useHttps;
            folderMapping.FolderMappingSettings[Constants.DirectLink] = this.chkDirectLink.Checked;
            folderMapping.FolderMappingSettings[Constants.CustomDomain] = customDomain;
            folderMapping.FolderMappingSettings[Constants.SyncBatchSize] = synchBatchSize;

            folderMappingController.UpdateFolderMapping(folderMapping);
        }

        /// <summary>
        /// </summary>
        protected void ddlContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ddlContainers.SelectedIndex != 1)
            {
                return;
            }

            if (this.tbAccountName.Text.Trim().Length > 0 && this.tbAccountKey.Text.Trim().Length > 0)
            {
                this.ddlContainers.Items.Clear();

                this.ddlContainers.Items.Add(Localization.GetString("SelectContainer", this.LocalResourceFile));
                this.ddlContainers.Items.Add(Localization.GetString("RefreshContainerList", this.LocalResourceFile));

                this.LoadContainers();

                if (this.ddlContainers.Items.Count == 3)
                {
                    // If there is only one container, then select it
                    this.ddlContainers.SelectedValue = this.ddlContainers.Items[2].Value;
                }
            }
            else
            {
                this.valContainerName.ErrorMessage = Localization.GetString("CredentialsRequired.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;
            }
        }

        /// <summary>
        /// </summary>
        protected void btnNewContainer_Click(object sender, EventArgs e)
        {
            this.SelectContainerPanel.Visible = false;
            this.CreateContainerPanel.Visible = true;
        }

        /// <summary>
        /// </summary>
        protected void btnSelectExistingContainer_Click(object sender, EventArgs e)
        {
            this.SelectContainerPanel.Visible = true;
            this.CreateContainerPanel.Visible = false;
        }

        /// <summary>
        /// </summary>
        protected void valContainerName_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (this.SelectContainerPanel.Visible)
            {
                if (this.ddlContainers.SelectedIndex > 1)
                {
                    args.IsValid = true;
                    return;
                }
            }
            else
            {
                if (this.tbContainerName.Text.Trim().Length > 0)
                {
                    args.IsValid = true;
                    return;
                }
            }

            this.valContainerName.ErrorMessage = Localization.GetString("valContainerName.ErrorMessage", this.LocalResourceFile);
            args.IsValid = false;
        }

        private static bool AreThereFolderMappingsWithSameSettings(FolderMappingInfo folderMapping, string accountName, string container)
        {
            var folderMappingController = FolderMappingController.Instance;
            var folderMappings = folderMappingController.GetFolderMappings(folderMapping.PortalID);

            return folderMappings
                .Where(fm => fm.FolderMappingID != folderMapping.FolderMappingID && fm.FolderProviderType == folderMapping.FolderProviderType)
                .Any(fm => fm.FolderMappingSettings[Constants.AccountName].ToString().Equals(accountName, StringComparison.InvariantCulture) &&
                           fm.FolderMappingSettings[Constants.Container].ToString().Equals(container, StringComparison.InvariantCultureIgnoreCase));
        }

        private string GetUseHttps()
        {
            return this.chkUseHttps.Checked.ToString();
        }

        private string GetContainer()
        {
            string container;

            if (this.SelectContainerPanel.Visible)
            {
                container = this.ddlContainers.SelectedValue;
            }
            else
            {
                container = this.tbContainerName.Text.Trim().ToLowerInvariant();
                if (!this.CreateContainer(container))
                {
                    throw new Exception();
                }
            }

            return container;
        }

        private bool CreateContainer(string containerName)
        {
            var accountName = this.tbAccountName.Text.Trim();
            var accountKey = this.tbAccountKey.Text.Trim();
            var useHttps = this.chkUseHttps.Checked;

            StorageCredentials sc;

            try
            {
                sc = new StorageCredentials(accountName, accountKey);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                this.valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;

                return false;
            }

            var csa = new CloudStorageAccount(sc, useHttps);
            var blobClient = csa.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            try
            {
                if (container.CreateIfNotExists())
                {
                    var permissions = container.GetPermissions();
                    permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                    container.SetPermissions(permissions);
                }

                return true;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    switch (ex.RequestInformation.ExtendedErrorInformation.ErrorCode)
                    {
                        case "AccountNotFound":
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "AccountNotFound.ErrorMessage",
                                this.LocalResourceFile);
                            break;
                        case "AuthenticationFailure":
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                            break;
                        case "AccessDenied":
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "AccessDenied.ErrorMessage",
                                this.LocalResourceFile);
                            break;
                        case "ContainerAlreadyExists":
                            return true;
                        default:
                            Logger.Error(ex);
                            this.valContainerName.ErrorMessage = Localization.GetString(
                                "NewContainer.ErrorMessage",
                                this.LocalResourceFile);
                            break;
                    }
                }
                else
                {
                    this.valContainerName.ErrorMessage = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                this.valContainerName.ErrorMessage = Localization.GetString("NewContainer.ErrorMessage", this.LocalResourceFile);
            }

            this.valContainerName.IsValid = false;
            return false;
        }

        private string GetAccountKey()
        {
            return FolderProvider.Instance(Constants.FolderProviderType).EncryptValue(this.tbAccountKey.Text);
        }

        private string GetAccountName()
        {
            return FolderProvider.Instance(Constants.FolderProviderType).EncryptValue(this.tbAccountName.Text);
        }

        private string GetCustomDomain()
        {
            return FolderProvider.Instance(Constants.FolderProviderType).EncryptValue(this.tbCustomDomain.Text);
        }

        private string GetSynchBatchSize()
        {
            return this.tbSyncBatchSize.Text;
        }

        private void LoadContainers()
        {
            var accountName = this.tbAccountName.Text.Trim();
            var accountKey = this.tbAccountKey.Text.Trim();
            var useHttps = this.chkUseHttps.Checked;

            StorageCredentials sc;

            try
            {
                sc = new StorageCredentials(accountName, accountKey);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                this.valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;

                return;
            }

            var csa = new CloudStorageAccount(sc, useHttps);
            var blobClient = csa.CreateCloudBlobClient();

            try
            {
                foreach (var container in blobClient.ListContainers())
                {
                    this.ddlContainers.Items.Add(container.Name);
                }
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.ExtendedErrorInformation != null)
                {
                    switch (ex.RequestInformation.ExtendedErrorInformation.ErrorCode)
                    {
                        case "AuthenticationFailure":
                            this.valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", this.LocalResourceFile);
                            break;
                        default:
                            Logger.Error(ex);
                            this.valContainerName.ErrorMessage = Localization.GetString("ListContainers.ErrorMessage", this.LocalResourceFile);
                            break;
                    }
                }
                else
                {
                    this.valContainerName.ErrorMessage = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
                }

                this.valContainerName.IsValid = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                this.valContainerName.ErrorMessage = Localization.GetString("ListContainers.ErrorMessage", this.LocalResourceFile);
                this.valContainerName.IsValid = false;
            }
        }
    }
}
