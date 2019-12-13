using System.Collections.Generic;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Services.Assets
{
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
            if (!cache.ContainsKey(folderId))
            {
                var folder = FolderManager.Instance.GetFolder(folderId);
                cache.Add(folderId, folder.FolderPath);
            }

            return cache[folderId];
        }
    }
}
