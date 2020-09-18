// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Assets
{
    using System.Collections.Generic;

    using DotNetNuke.Services.FileSystem;

    public class FolderPathComparer : IComparer<int>
    {
        private readonly Dictionary<int, string> cache;

        public FolderPathComparer()
        {
            this.cache = new Dictionary<int, string>();
        }

        public int Compare(int folderIdA, int folderIdB)
        {
            if (folderIdA == folderIdB)
            {
                return 0;
            }

            return string.Compare(this.GetFolderPath(folderIdA), this.GetFolderPath(folderIdB));
        }

        private string GetFolderPath(int folderId)
        {
            if (!this.cache.ContainsKey(folderId))
            {
                var folder = FolderManager.Instance.GetFolder(folderId);
                this.cache.Add(folderId, folder.FolderPath);
            }

            return this.cache[folderId];
        }
    }
}
