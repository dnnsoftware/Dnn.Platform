// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
                var path = this.Blob.RelativePath.Replace("+", "%2b");
                return HttpUtility.UrlDecode(path);
            }
        }

        public DateTime LastModified => this.Blob.LastModifiedUtc;

        public long Size => this.Blob.Length;

        public string HashCode => this.Blob.ETag;
    }
}
