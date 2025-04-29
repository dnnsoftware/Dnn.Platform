// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider;

using System;
using System.Web;

using DotNetNuke.Providers.FolderProviders.Components;

[Serializable]
public class AzureRemoteStorageItem : IRemoteStorageItem
{
    /// <inheritdoc/>
    public string Key
    {
        get
        {
            var path = this.Blob.RelativePath.Replace("+", "%2b");
            return HttpUtility.UrlDecode(path);
        }
    }

    /// <inheritdoc/>
    public DateTime LastModified => this.Blob.LastModifiedUtc;

    /// <inheritdoc/>
    public long Size => this.Blob.Length;

    /// <inheritdoc/>
    public string HashCode => this.Blob.ETag;

    public AzureBlob Blob { get; set; }
}
