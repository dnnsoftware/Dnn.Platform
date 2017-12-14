#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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