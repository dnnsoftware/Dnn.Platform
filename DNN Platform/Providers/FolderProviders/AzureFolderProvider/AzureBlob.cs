// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using System;

    using Microsoft.WindowsAzure.Storage.Blob;

    [Serializable]
    public class AzureBlob
    {
        private readonly string _relativePath = string.Empty;
        private readonly DateTime _lastModifiedUtc = DateTime.MinValue;
        private readonly long _length = -1;
        private readonly string _etag = string.Empty;

        public AzureBlob(CloudBlob blob)
        {
            if (blob == null)
            {
                return;
            }

            this._relativePath = blob.RelativePath();
            this._lastModifiedUtc = blob.Properties.LastModified.GetValueOrDefault(DateTimeOffset.MinValue).UtcDateTime;
            this._length = blob.Properties.Length;
            this._etag = blob.Properties.ETag;
        }

        public string RelativePath => this._relativePath;

        public DateTime LastModifiedUtc => this._lastModifiedUtc;

        public long Length => this._length;

        public string ETag => this._etag;
    }
}
