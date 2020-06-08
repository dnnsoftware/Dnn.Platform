// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
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

            _relativePath = blob.RelativePath();
            _lastModifiedUtc = blob.Properties.LastModified.GetValueOrDefault(DateTimeOffset.MinValue).UtcDateTime;
            _length = blob.Properties.Length;
            _etag = blob.Properties.ETag;
        }

        public string RelativePath => _relativePath;

        public DateTime LastModifiedUtc => _lastModifiedUtc;

        public long Length => _length;

        public string ETag => _etag;
    }
}
