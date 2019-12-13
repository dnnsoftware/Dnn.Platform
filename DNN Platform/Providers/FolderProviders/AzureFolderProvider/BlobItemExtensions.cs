// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
﻿using Microsoft.WindowsAzure.Storage.Blob;

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
