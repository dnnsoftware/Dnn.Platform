﻿#region Copyright
// DNN® and DotNetNuke® - http://www.DNNSoftware.com
// Copyright ©2002-2019
// by DNN Corp
// All Rights Reserved
#endregion

using Microsoft.WindowsAzure.Storage.Blob;

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    internal static class BlobItemExtensions
    {
        public static string RelativePath(this IListBlobItem item)
        {
            var filterUrl = item.Container.Uri.AbsoluteUri;
            if (!filterUrl.EndsWith("/"))
            {
                filterUrl = filterUrl + "/";
            }

            return item.Uri.AbsoluteUri.Replace(filterUrl, string.Empty);
        }
    }
}