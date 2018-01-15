#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Web;
using DotNetNuke.Providers.FolderProviders.Components;

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    [Serializable]
    public class AzureRemoteStorageItem : IRemoteStorageItem
    {
        public AzureBlob Blob { get; set; }

        public string Key
        {
            get
            {
                var path = Blob.RelativePath.Replace("+", "%2b");
                return HttpUtility.UrlDecode(path);
            }
        }

        public DateTime LastModified => Blob.LastModifiedUtc;

        public long Size => Blob.Length;

        public string HashCode => Blob.ETag;
    }
}
