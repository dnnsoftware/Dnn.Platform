#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

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
            _lastModifiedUtc = blob.Properties.LastModifiedUtc;
            _length = blob.Properties.Length;
            _etag = blob.Properties.ETag;
        }

        public string RelativePath => _relativePath;

        public DateTime LastModifiedUtc => _lastModifiedUtc;

        public long Length => _length;

        public string ETag => _etag;
    }
}
