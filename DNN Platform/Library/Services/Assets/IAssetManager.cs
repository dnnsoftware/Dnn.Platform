#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
    public interface IAssetManager
    {
        ContentPage GetFolderContent(int folderId, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure);

        ContentPage SearchFolderContent(int folderId, string pattern, int startIndex, int numItems, string sortExpression = null, SubfolderFilter subfolderFilter = SubfolderFilter.IncludeSubfoldersFolderStructure);

        IEnumerable<IFolderInfo> GetFolders(IFolderInfo parentFolder, string orderingField, bool asc);

        IFileInfo RenameFile(int fileId, string newFileName);

        IFolderInfo RenameFolder(int folderId, string folderName);

        IFolderInfo CreateFolder(string folderName, int folderParentId, int folderMappingId, string mappedPath);

        bool DeleteFolder(int folderId, bool onlyUnlink, ICollection<IFolderInfo> nonDeletedSubfolders);

        bool DeleteFile(int fileId);
    }
}