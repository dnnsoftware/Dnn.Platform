// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.FolderProviders.AzureFolderProvider
{
    using Microsoft.WindowsAzure.Storage.Blob;

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
