#region Copyright
// DNN® and DotNetNuke® - http://www.DNNSoftware.com
// Copyright ©2002-2019
// by DNN Corp
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
