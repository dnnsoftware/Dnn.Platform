#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    ///<summary>
    /// Windows Azure Storage Settings Control
    ///</summary>
    public partial class Settings : FolderMappingSettingsControlBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (Settings));

        #region Overrided Methods

        /// <summary>
        /// Loads concrete settings.
        /// </summary>
        /// <param name="folderMappingSettings">The Hashtable containing the folder mapping settings.</param>
        public override void LoadSettings(Hashtable folderMappingSettings)
        {
            var folderProvider = FolderProvider.Instance(Constants.FolderProviderType);
            if (folderMappingSettings.ContainsKey(Constants.AccountName))
            {
                tbAccountName.Text = folderProvider.GetEncryptedSetting(folderMappingSettings, Constants.AccountName);
            }

            if (folderMappingSettings.ContainsKey(Constants.AccountKey))
            {
                tbAccountKey.Text = folderProvider.GetEncryptedSetting(folderMappingSettings, Constants.AccountKey);                
            }

            if (tbAccountName.Text.Length > 0 && tbAccountKey.Text.Length > 0)
            {
                var bucketName = "";

                if (folderMappingSettings.ContainsKey(Constants.Container))
                {
                    bucketName = folderMappingSettings[Constants.Container].ToString();
                }

                LoadContainers();

                var bucket = ddlContainers.Items.FindByText(bucketName);

                if (bucket != null)
                {
                    ddlContainers.SelectedIndex = ddlContainers.Items.IndexOf(bucket);
                }
            }

            if (folderMappingSettings.ContainsKey(Constants.UseHttps))
            {
                chkUseHttps.Checked = bool.Parse(folderMappingSettings[Constants.UseHttps].ToString());
            }

            chkDirectLink.Checked = !folderMappingSettings.ContainsKey(Constants.DirectLink) || folderMappingSettings[Constants.DirectLink].ToString().ToLowerInvariant() == "true";

            if (folderMappingSettings.ContainsKey(Constants.CustomDomain))
            {
                tbCustomDomain.Text = folderProvider.GetEncryptedSetting(folderMappingSettings, Constants.CustomDomain);
            }

            if (folderMappingSettings.ContainsKey(Constants.SyncBatchSize) && folderMappingSettings[Constants.SyncBatchSize] != null)
            {
                tbSyncBatchSize.Text = folderMappingSettings[Constants.SyncBatchSize].ToString();
            }
            else
            {
                tbSyncBatchSize.Text = Constants.DefaultSyncBatchSize.ToString();
            }
        }

        /// <summary>
        /// Updates concrete settings for the specified folder mapping.
        /// </summary>
        /// <param name="folderMappingID">The folder mapping identifier.</param>
        public override void UpdateSettings(int folderMappingID)
        {
            Page.Validate();

            if (!Page.IsValid)
            {
                throw new Exception();
            }

            var folderMappingController = FolderMappingController.Instance;
            var folderMapping = folderMappingController.GetFolderMapping(folderMappingID);

            var accountName = GetAccountName();
            var accountKey = GetAccountKey();
            var container = GetContainer();
            var useHttps = GetUseHttps();
            var customDomain = GetCustomDomain();
            var synchBatchSize = GetSynchBatchSize();

            if (AreThereFolderMappingsWithSameSettings(folderMapping, accountName, container))
            {
                valContainerName.ErrorMessage = Localization.GetString("MultipleFolderMappingsWithSameSettings.ErrorMessage", LocalResourceFile);
                valContainerName.IsValid = false;

                throw new Exception();
            }
            
            folderMapping.FolderMappingSettings[Constants.AccountName] = accountName;
            folderMapping.FolderMappingSettings[Constants.AccountKey] = accountKey;
            folderMapping.FolderMappingSettings[Constants.Container] = container;
            folderMapping.FolderMappingSettings[Constants.UseHttps] = useHttps;
			folderMapping.FolderMappingSettings[Constants.DirectLink] = chkDirectLink.Checked;
            folderMapping.FolderMappingSettings[Constants.CustomDomain] = customDomain;
            folderMapping.FolderMappingSettings[Constants.SyncBatchSize] = synchBatchSize;

            folderMappingController.UpdateFolderMapping(folderMapping);
        }

        #endregion

        #region Private Methods

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
            return chkUseHttps.Checked.ToString();
        }

        private string GetContainer()
        {
            string container;

            if (SelectContainerPanel.Visible)
            {
                container = ddlContainers.SelectedValue;
            }
            else
            {
                container = tbContainerName.Text.Trim().ToLowerInvariant();
                if (!CreateContainer(container)) throw new Exception();
            }

            return container;
        }

        private bool CreateContainer(string containerName)
        {
            var accountName = tbAccountName.Text.Trim();
            var accountKey = tbAccountKey.Text.Trim();
            var useHttps = chkUseHttps.Checked;

            StorageCredentials sc;

            try
            {
                sc = new StorageCredentials(accountName, accountKey);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", LocalResourceFile);
                valContainerName.IsValid = false;

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
                            valContainerName.ErrorMessage = Localization.GetString("AccountNotFound.ErrorMessage",
                                LocalResourceFile);
                            break;
                        case "AuthenticationFailure":
                            valContainerName.ErrorMessage = Localization.GetString(
                                "AuthenticationFailure.ErrorMessage", LocalResourceFile);
                            break;
                        case "AccessDenied":
                            valContainerName.ErrorMessage = Localization.GetString("AccessDenied.ErrorMessage",
                                LocalResourceFile);
                            break;
                        case "ContainerAlreadyExists":
                            return true;
                        default:
                            Logger.Error(ex);
                            valContainerName.ErrorMessage = Localization.GetString("NewContainer.ErrorMessage",
                                LocalResourceFile);
                            break;
                    }
                }
                else
                {
                    valContainerName.ErrorMessage = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                valContainerName.ErrorMessage = Localization.GetString("NewContainer.ErrorMessage", LocalResourceFile);
            }

            valContainerName.IsValid = false;
            return false;
        }

        private string GetAccountKey()
        {
            return FolderProvider.Instance(Constants.FolderProviderType).EncryptValue(tbAccountKey.Text);            
        }

        private string GetAccountName()
        {
            return FolderProvider.Instance(Constants.FolderProviderType).EncryptValue(tbAccountName.Text);            
        }

        private string GetCustomDomain()
        {
            return FolderProvider.Instance(Constants.FolderProviderType).EncryptValue(tbCustomDomain.Text);
        }

        private string GetSynchBatchSize()
        {
            return tbSyncBatchSize.Text;
        }

        private void LoadContainers()
        {
            var accountName = tbAccountName.Text.Trim();
            var accountKey = tbAccountKey.Text.Trim();
            var useHttps = chkUseHttps.Checked;

            StorageCredentials sc;
            
            try
            {
                sc = new StorageCredentials(accountName, accountKey);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", LocalResourceFile);
                valContainerName.IsValid = false;
                
                return;
            }

            var csa = new CloudStorageAccount(sc, useHttps);
            var blobClient = csa.CreateCloudBlobClient();

            try
            {
                foreach (var container in blobClient.ListContainers())
                {
                    ddlContainers.Items.Add(container.Name);
                }
            }
            catch (StorageException ex)
            {
                if(ex.RequestInformation.ExtendedErrorInformation != null)
                { 
                    switch (ex.RequestInformation.ExtendedErrorInformation.ErrorCode)
                    {
                        case "AuthenticationFailure":
                            valContainerName.ErrorMessage = Localization.GetString("AuthenticationFailure.ErrorMessage", LocalResourceFile);
                            break;
                        default:
                            Logger.Error(ex);
                            valContainerName.ErrorMessage = Localization.GetString("ListContainers.ErrorMessage", LocalResourceFile);
                            break;
                    }
                }
                else
                {
                    valContainerName.ErrorMessage = ex.RequestInformation.HttpStatusMessage ?? ex.Message;
                }

                valContainerName.IsValid = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                valContainerName.ErrorMessage = Localization.GetString("ListContainers.ErrorMessage", LocalResourceFile);
                valContainerName.IsValid = false;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// </summary>
        protected void ddlContainers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlContainers.SelectedIndex != 1) return;

            if (tbAccountName.Text.Trim().Length > 0 && tbAccountKey.Text.Trim().Length > 0)
            {
                ddlContainers.Items.Clear();

                ddlContainers.Items.Add(Localization.GetString("SelectContainer", LocalResourceFile));
                ddlContainers.Items.Add(Localization.GetString("RefreshContainerList", LocalResourceFile));

                LoadContainers();

                if (ddlContainers.Items.Count == 3)
                {
                    // If there is only one container, then select it
                    ddlContainers.SelectedValue = ddlContainers.Items[2].Value;
                }
            }
            else
            {
                valContainerName.ErrorMessage = Localization.GetString("CredentialsRequired.ErrorMessage", LocalResourceFile);
                valContainerName.IsValid = false;
            }
        }

        /// <summary>
        /// </summary>
        protected void btnNewContainer_Click(object sender, EventArgs e)
        {
            SelectContainerPanel.Visible = false;
            CreateContainerPanel.Visible = true;
        }

        /// <summary>
        /// </summary>
        protected void btnSelectExistingContainer_Click(object sender, EventArgs e)
        {
            SelectContainerPanel.Visible = true;
            CreateContainerPanel.Visible = false;
        }

        /// <summary>
        /// </summary>
        protected void valContainerName_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (SelectContainerPanel.Visible)
            {
                if (ddlContainers.SelectedIndex > 1)
                {
                    args.IsValid = true;
                    return;
                }
            }
            else
            {
                if (tbContainerName.Text.Trim().Length > 0)
                {
                    args.IsValid = true;
                    return;
                }
            }

            valContainerName.ErrorMessage = Localization.GetString("valContainerName.ErrorMessage", LocalResourceFile);
            args.IsValid = false;
        }

        #endregion
    }
}