#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Web;
using DotNetNuke.Providers.FolderProviders.Components;
using Microsoft.WindowsAzure.StorageClient;

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    [Serializable]
    public class AzureRemoteStorageItem : IRemoteStorageItem
    {
        public CloudBlob Blob { get; set; }

        public string Key
        {
            get
            {
                var path = Blob.RelativePath().Replace("+", "%2b");
                return HttpUtility.UrlDecode(path);
            }
        }

        public DateTime LastModified
        {
            get { return (Blob != null) ? Blob.Properties.LastModifiedUtc : DateTime.MinValue; } 
        }
        
        public long Size
        {
            get { return (Blob != null) ? Blob.Properties.Length : -1; }
        }

        public string HashCode
        {
            get { return (Blob != null) ? Blob.Properties.ETag : String.Empty; }
        }
    }
}
