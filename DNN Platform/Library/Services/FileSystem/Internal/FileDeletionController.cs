#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.FileSystem.Internal
{
    public class FileDeletionController : ServiceLocator< IFileDeletionController, FileDeletionController>, IFileDeletionController
    {
        public void DeleteFile(IFileInfo file)
        {
            FileVersionController.Instance.DeleteAllUnpublishedVersions(file, false);

            var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
            FolderProvider.Instance(folderMapping.FolderProviderType).DeleteFile(file);
            
            DeleteFileData(file);
        }

        public void DeleteFileData(IFileInfo file)
        {
            DataProvider.Instance().DeleteFile(file.PortalId, file.FileName, file.FolderId);
            DeleteContentItem(file.ContentItemID);
        }

        private void DeleteContentItem(int contentItemId)
        {
            if (contentItemId == Null.NullInteger) return;

            Util.GetContentController().DeleteContentItem(contentItemId);
        }

        protected override Func<IFileDeletionController> GetFactory()
        {
            return () => new FileDeletionController();
        }
    }
}
