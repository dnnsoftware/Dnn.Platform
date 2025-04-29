// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider;

using System;

using Microsoft.WindowsAzure.Storage.Blob;

[Serializable]
public class AzureBlob
{
    private readonly string relativePath = string.Empty;
    private readonly DateTime lastModifiedUtc = DateTime.MinValue;
    private readonly long length = -1;
    private readonly string etag = string.Empty;

    public AzureBlob(CloudBlob blob)
    {
        if (blob == null)
        {
            return;
        }

        this.relativePath = blob.RelativePath();
        this.lastModifiedUtc = blob.Properties.LastModified.GetValueOrDefault(DateTimeOffset.MinValue).UtcDateTime;
        this.length = blob.Properties.Length;
        this.etag = blob.Properties.ETag;
    }

    public string RelativePath => this.relativePath;

    public DateTime LastModifiedUtc => this.lastModifiedUtc;

    public long Length => this.length;

    public string ETag => this.etag;
}
