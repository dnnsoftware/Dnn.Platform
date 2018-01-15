#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

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

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    ///<summary>
    /// Windows Azure Storage Folder Provider
    ///</summary>
    public class AzureFolderProvider : BaseRemoteStorageProvider
    {
        #region Constants

        private const string AccountName = "AccountName";
        private const string AccountKey = "AccountKey";
        private const string Container = "Container";
        private const string UseHttps = "UseHttps";
		private const string DirectLink = "DirectLink";
        private const string CustomDomain = "CustomDomain";
        private const string PlaceHolderFileName = "dotnetnuke.placeholder.donotdelete";

        #endregion

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

        #region Private Methods

        private static void CheckSettings(FolderMappingInfo folderMapping)
        {
            var settings = folderMapping.FolderMappingSettings;

            if (string.IsNullOrEmpty((string)settings[AccountName]) ||
                string.IsNullOrEmpty((string)settings[AccountKey]) ||
                string.IsNullOrEmpty((string)settings[Container]) ||
                string.IsNullOrEmpty((string)settings[UseHttps]))
            {
                throw new Exception("Settings cannot be found.");
            }
        }

        private CloudBlobContainer GetContainer(FolderMappingInfo folderMapping)
        {

            CheckSettings(folderMapping);

            var accountName = GetEncryptedSetting(folderMapping.FolderMappingSettings, AccountName);
            var accountKey = GetEncryptedSetting(folderMapping.FolderMappingSettings, AccountKey);
            var container = GetSetting(folderMapping, Container);
            var useHttps = GetBooleanSetting(folderMapping, UseHttps);

            var sc = new StorageCredentials(accountName, accountKey);
            var csa = new CloudStorageAccount(sc, useHttps);
            var blobClient = csa.CreateCloudBlobClient();
            return blobClient.GetContainerReference(container);
        }

        #endregion

        protected override void CopyFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
            var container = GetContainer(folderMapping);

            var sourceBlob = container.GetBlobReference(sourceUri);
            var newBlob = container.GetBlobReference(newUri);

            newBlob.StartCopy(sourceBlob.Uri);

            ClearCache(folderMapping.FolderMappingID);
        }

        protected override void DeleteFileInternal(FolderMappingInfo folderMapping, string uri)
        {
            var container = GetContainer(folderMapping);
            var blob = container.GetBlobReference(uri);

            blob.DeleteIfExists();

            ClearCache(folderMapping.FolderMappingID);
        }

        protected override void DeleteFolderInternal(FolderMappingInfo folderMapping, IFolderInfo folder)
        {
            DeleteFileInternal(folderMapping, folder.MappedPath + PlaceHolderFileName);
        }

        protected override Stream GetFileStreamInternal(FolderMappingInfo folderMapping, string uri)
        {
            var container = GetContainer(folderMapping);
            var blob = container.GetBlockBlobReference(uri);

            var memoryStream = new MemoryStream();
            blob.DownloadToStream(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        protected override IList<IRemoteStorageItem> GetObjectList(FolderMappingInfo folderMapping)
        {
            var cacheKey = string.Format(ListObjectsCacheKey, folderMapping.FolderMappingID);

            return CBO.GetCachedObject<IList<IRemoteStorageItem>>(new CacheItemArgs(cacheKey,
                                                                        ListObjectsCacheTimeout,
                                                                        CacheItemPriority.Default,
                                                                        folderMapping.FolderMappingID),
                                        c =>
                                        {
                                            var container = GetContainer(folderMapping);

                                            BlobContinuationToken continuationToken = null;
                                            BlobResultSegment resultSegment = null;

                                            var list = new List<IRemoteStorageItem>();
                                            do
                                            {
                                                //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                                                //or by calling a different overload.
                                                resultSegment = container.ListBlobsSegmented("", true, BlobListingDetails.All, 10, continuationToken, null, null);
                                                foreach (var blobItem in resultSegment.Results)
                                                {
                                                    list.Add(new AzureRemoteStorageItem {Blob = new AzureBlob(blobItem as CloudBlob)});
                                                }

                                                //Get the continuation token.
                                                continuationToken = resultSegment.ContinuationToken;
                                            }
                                            while (continuationToken != null);
                                            
                                            return list;
                                        });

        }

        protected override void MoveFileInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
            var container = GetContainer(folderMapping);

            var sourceBlob = container.GetBlobReference(sourceUri);
            var newBlob = container.GetBlobReference(newUri);

            newBlob.StartCopy(sourceBlob.Uri);
            sourceBlob.Delete();

            ClearCache(folderMapping.FolderMappingID);
        }

        protected override void MoveFolderInternal(FolderMappingInfo folderMapping, string sourceUri, string newUri)
        {
            var container = GetContainer(folderMapping);
            var directory = container.GetDirectoryReference(sourceUri);
            var blobs = directory.ListBlobs(true);

            foreach (var blobItem in blobs)
            {
                var blob = (CloudBlob)blobItem;
                var newBlob = container.GetBlobReference(newUri + blobItem.Uri.LocalPath.Substring(directory.Uri.LocalPath.Length));
                newBlob.StartCopy(blob.Uri);
                blob.Delete();
            }

            ClearCache(folderMapping.FolderMappingID);
        }

        protected override void UpdateFileInternal(Stream stream, FolderMappingInfo folderMapping, string uri)
        {
            var container = GetContainer(folderMapping);
            var blob = container.GetBlockBlobReference(uri);

            stream.Seek(0, SeekOrigin.Begin);
            blob.UploadFromStream(stream);

            // Set the content type
            blob.Properties.ContentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(uri));
			blob.SetProperties();

            ClearCache(folderMapping.FolderMappingID);
        }

        #region FolderProvider Methods

        /// <remarks>
        /// Azure Storage doesn't support folders, so we create a file in order for the folder to not be deleted during future synchronizations.
        /// The file has an extension not allowed by host. This way the file won't be added during synchronizations.
        /// </remarks>
        public override void AddFolder(string folderPath, FolderMappingInfo folderMapping, string mappedPath)
        {
            Requires.NotNull("folderPath", folderPath);
            Requires.NotNull("folderMapping", folderMapping);

            UpdateFileInternal(new MemoryStream(), folderMapping, mappedPath + PlaceHolderFileName);
        }

        /// <summary>
        /// Gets the direct Url to the file.
        /// </summary>
        public override string GetFileUrl(IFileInfo file)
        {
            Requires.NotNull("file", file);

			var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
	        var directLink = string.IsNullOrEmpty(GetSetting(folderMapping, DirectLink)) || GetSetting(folderMapping, DirectLink).ToLowerInvariant() == "true";

	        if (directLink)
	        {
	            var folder = FolderManager.Instance.GetFolder(file.FolderId);
                var uri = folder.MappedPath + file.FileName;

		        var container = GetContainer(folderMapping);
		        var blob = container.GetBlobReference(uri);
                var absuri = blob.Uri.AbsoluteUri;
                var customDomain = GetEncryptedSetting(folderMapping.FolderMappingSettings, CustomDomain);

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
			        policy = new SharedAccessBlobPolicy { Permissions = SharedAccessBlobPermissions.Read, SharedAccessExpiryTime = DateTime.UtcNow.AddYears(100)};

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
        public override string GetFolderProviderIconPath()
        {
            return Globals.ResolveUrl("~/Providers/FolderProviders/AzureFolderProvider/images/FolderAzure_32x32_Standard.png");
        }

        public List<string> GetAllContainers(FolderMappingInfo folderMapping)
        {
            List<string> containers = new List<string>();
            var accountName = GetEncryptedSetting(folderMapping.FolderMappingSettings, AccountName);
            var accountKey = GetEncryptedSetting(folderMapping.FolderMappingSettings, AccountKey);
            var useHttps = GetBooleanSetting(folderMapping, UseHttps);

            var sc = new StorageCredentials(accountName, accountKey);
            var csa = new CloudStorageAccount(sc, useHttps);
            var blobClient = csa.CreateCloudBlobClient();
            blobClient.ListContainers().ToList().ForEach(x => containers.Add(x.Name));
            return containers;
        }

        #endregion

    }
}