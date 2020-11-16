// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Web.Caching;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Providers.FolderProviders.Components;
    using DotNetNuke.Services.FileSystem;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Auth;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Windows Azure Storage Folder Provider.
    /// </summary>
    public class AzureFolderProvider : BaseRemoteStorageProvider
    {
        public AzureFolderProvider()
        {
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
        }

        protected override string FileNotFoundMessage
        {
            get
            {
                return "Azure File Not Found";
            }
        }

        protected override string ObjectCacheKey
        {
            get { return "Azure_Object_{0}_{1}"; }
        }

        protected override string ListObjectsCacheKey
        {
            get { return "Azure_ListObjects_{0}"; }
        }

        /// <remarks>
        /// Azure Storage doesn't support folders, so we create a file in order for the folder to not be deleted during future synchronizations.
        /// The file has an extension not allowed by host. This way the file won't be added during synchronizations.
        /// </remarks>
        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping, string mappedPath)
        {
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            this.UpdateFileInternal(new MemoryStream(), folderMapping, mappedPath + Constants.PlaceHolderFileName);
        }

        /// <summary>
        /// Gets the direct Url to the file.
        /// </summary>
        /// <returns></returns>
        public override string GetFileUrl(IFileInfo file)
        {
            Requires.NotNull("file", file);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            var directLink = string.IsNullOrEmpty(GetSetting(folderMapping, Constants.DirectLink)) || GetSetting(folderMapping, Constants.DirectLink).ToLowerInvariant() == "true";

            if (directLink)
            {
                var folder = FolderManager.Instance.GetFolder(file.FolderId);
                var uri = folder.MappedPath + file.FileName;

                var container = this.GetContainer(folderMapping);
                var blob = container.GetBlobReference(uri);
                var absuri = blob.Uri.AbsoluteUri;
                var customDomain = this.GetEncryptedSetting(folderMapping.FolderMappingSettings, Constants.CustomDomain);

                if (!string.IsNullOrEmpty(customDomain))
                {
                    var customUri = new UriBuilder(customDomain).Uri;
                    absuri =
                        new UriBuilder(blob.Uri.AbsoluteUri) { Host = customUri.Host, Scheme = customUri.Scheme, Port = customUri.Port }
                            .Uri.AbsoluteUri;
                }

                const string groupPolicyIdentifier = "DNNFileManagerPolicy";

                var permissions = container.GetPermissions();

                SharedAccessBlobPolicy policy;

                permissions.SharedAccessPolicies.TryGetValue(groupPolicyIdentifier, out policy);

                if (policy == null)
                {
                    policy = new SharedAccessBlobPolicy { Permissions = SharedAccessBlobPermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddYears(100) };

                    permissions.SharedAccessPolicies.Add(groupPolicyIdentifier, policy);
                }
                else
                {
                    policy.Permissions = SharedAccessBlobPermissions.Read;
                    policy.SharedAccessExpiryTime = DateTime.UtcNow.AddYears(100);
                }

                /*
                 * Workaround for CONTENT-3662
                 * The Azure Client Storage api has issue when used with Italian Thread.Culture or eventually other cultures
                 * (see this article for further information https://connect.microsoft.com/VisualStudio/feedback/details/760974/windows-azure-sdk-cloudblobcontainer-setpermissions-permissions-as-microsoft-windowsazure-storageclient-blobcontainerpermissions-error).
                 * This code changes the thread culture to en-US
                 */
                var currentCulture = Thread.CurrentThread.CurrentCulture;
                if (currentCulture.Name != "en-US")
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
                }

                container.SetPermissions(permissions);

                var signature = blob.GetSharedAccessSignature(new SharedAccessBlobPolicy(), groupPolicyIdentifier);

                // Reset original Thread Culture
                if (currentCulture.Name != "en-US")
                {
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                }

                return absuri + signature;
            }

            return FileLinkClickController.Instance.GetFileLinkClick(file);
        }

        /// <summary>
        /// Gets the URL of the image to display in FileManager tree.
        /// </summary>
        /// <returns></returns>
        public override string GetFolderProviderIconPath()
        {
            return Globals.ResolveUrl("~/Providers/FolderProviders/AzureFolderProvider/images/FolderAzure_32x32_Standard.png");
        }

        public List<string> GetAllContainers(FolderMappingInfo folderMapping)
        {
            List<string> containers = new List<string>();
            var accountName = this.GetEncryptedSetting(folderMapping.FolderMappingSettings, Constants.AccountName);
            var accountKey = this.GetEncryptedSetting(folderMapping.FolderMappingSettings, Constants.AccountKey);
            var useHttps = GetBooleanSetting(folderMapping, Constants.UseHttps);

            var sc = new StorageCredentials(accountName, accountKey);
            var csa = new CloudStorageAccount(sc, useHttps);
            var blobClient = csa.CreateCloudBlobClient();
            blobClient.ListContainers().ToList().ForEach(x => containers.Add(x.Name));
            return containers;
        }

        protected override void CopyFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
            var container = this.GetContainer(folderMapping);

            var sourceBlob = container.GetBlobReference(sourceUri);
            var newBlob = container.GetBlobReference(newUri);

            newBlob.StartCopy(sourceBlob.Uri);

            this.ClearCache(folderMapping.FolderMappingID);
        }

        protected override void DeleteFileInternal(FolderMappingInfo folderMapping, string uri)
        {
            var container = this.GetContainer(folderMapping);
            var blob = container.GetBlobReference(uri);

            blob.DeleteIfExists();

            this.ClearCache(folderMapping.FolderMappingID);
        }

        protected override void DeleteFolderInternal(FolderMappingInfo folderMapping, IFolderInfo folder)
        {
            this.DeleteFileInternal(folderMapping, folder.MappedPath + Constants.PlaceHolderFileName);
        }

        protected override Stream GetFileStreamInternal(FolderMappingInfo folderMapping, string uri)
        {
            var container = this.GetContainer(folderMapping);
            var blob = container.GetBlockBlobReference(uri);

            var memoryStream = new MemoryStream();
            blob.DownloadToStream(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        protected override IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping)
        {
            var cacheKey = string.Format(this.ListObjectsCacheKey, folderMapping.FolderMappingID);

            return CBO.GetCachedObject<IList<IRemoteStorageItem>>(
                new CacheItemArgs(
                cacheKey,
                this.ListObjectsCacheTimeout,
                CacheItemPriority.Default,
                folderMapping.FolderMappingID),
                c =>
                                        {
                                            var container = this.GetContainer(folderMapping);
                                            var synchBatchSize = GetIntegerSetting(folderMapping, Constants.SyncBatchSize, Constants.DefaultSyncBatchSize);

                                            BlobContinuationToken continuationToken = null;
                                            BlobResultSegment resultSegment = null;

                                            var list = new List<IRemoteStorageItem>();
                                            do
                                            {
                                                // This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                                                // or by calling a different overload.
                                                resultSegment = container.ListBlobsSegmented(string.Empty, true, BlobListingDetails.All, synchBatchSize, continuationToken, null, null);
                                                foreach (var blobItem in resultSegment.Results)
                                                {
                                                    list.Add(new AzureRemoteStorageItem { Blob = new AzureBlob(blobItem as CloudBlob) });
                                                }

                                                // Get the continuation token.
                                                continuationToken = resultSegment.ContinuationToken;
                                            }
                                            while (continuationToken != null);

                                            return list;
                                        });
        }

        protected override void MoveFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
            var container = this.GetContainer(folderMapping);

            var sourceBlob = container.GetBlobReference(sourceUri);
            var newBlob = container.GetBlobReference(newUri);

            newBlob.StartCopy(sourceBlob.Uri);
            sourceBlob.Delete();

            this.ClearCache(folderMapping.FolderMappingID);
        }

        protected override void MoveFolderInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
            var container = this.GetContainer(folderMapping);
            var directory = container.GetDirectoryReference(sourceUri);
            var blobs = directory.ListBlobs(true);

            foreach (var blobItem in blobs)
            {
                var blob = (CloudBlob)blobItem;
                var newBlob = container.GetBlobReference(newUri + blobItem.Uri.LocalPath.Substring(directory.Uri.LocalPath.Length));
                newBlob.StartCopy(blob.Uri);
                blob.Delete();
            }

            this.ClearCache(folderMapping.FolderMappingID);
        }

        protected override void UpdateFileInternal(Stream stream, FolderMappingInfo folderMapping, string uri)
        {
            var container = this.GetContainer(folderMapping);
            var blob = container.GetBlockBlobReference(uri);

            stream.Seek(0, SeekOrigin.Begin);
            blob.UploadFromStream(stream);

            // Set the content type
            blob.Properties.ContentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(uri));
            blob.SetProperties();

            this.ClearCache(folderMapping.FolderMappingID);
        }

        private static void CheckSettings(FolderMappingInfo folderMapping)
        {
            var settings = folderMapping.FolderMappingSettings;

            if (string.IsNullOrEmpty((string)settings[Constants.AccountName]) ||
                string.IsNullOrEmpty((string)settings[Constants.AccountKey]) ||
                string.IsNullOrEmpty((string)settings[Constants.Container]) ||
                string.IsNullOrEmpty((string)settings[Constants.UseHttps]))
            {
                throw new Exception("Settings cannot be found.");
            }
        }

        private CloudBlobContainer GetContainer(FolderMappingInfo folderMapping)
        {
            CheckSettings(folderMapping);

            var accountName = this.GetEncryptedSetting(folderMapping.FolderMappingSettings, Constants.AccountName);
            var accountKey = this.GetEncryptedSetting(folderMapping.FolderMappingSettings, Constants.AccountKey);
            var container = GetSetting(folderMapping, Constants.Container);
            var useHttps = GetBooleanSetting(folderMapping, Constants.UseHttps);

            var sc = new StorageCredentials(accountName, accountKey);
            var csa = new CloudStorageAccount(sc, useHttps);
            var blobClient = csa.CreateCloudBlobClient();
            return blobClient.GetContainerReference(container);
        }
    }
}
