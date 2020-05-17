﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
