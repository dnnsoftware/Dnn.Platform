﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Services.FileSystem.Internal
{
    public class FileDeletionController : ServiceLocator< IFileDeletionController, FileDeletionController>, IFileDeletionController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileDeletionController));
        public void DeleteFile(IFileInfo file)
        {
            string lockReason;
            if (FileLockingController.Instance.IsFileLocked(file, out lockReason))
            {
                throw new FileLockedException(Localization.Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            FileVersionController.Instance.DeleteAllUnpublishedVersions(file, false);
            try
            {
                var folderMapping = FolderMappingController.Instance.GetFolderMapping(file.PortalId, file.FolderMappingID);
                FolderProvider.Instance(folderMapping.FolderProviderType).DeleteFile(file);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw new FolderProviderException(Localization.Localization.GetExceptionMessage("DeleteFileUnderlyingSystemError", "The underlying system threw an exception. The file has not been deleted."), ex);
            }

            DeleteFileData(file);

            DataCache.RemoveCache("GetFileById" + file.FileId);
        }

        public void UnlinkFile(IFileInfo file)
        {
            string lockReason;
            if (FileLockingController.Instance.IsFileLocked(file, out lockReason))
            {
                throw new FileLockedException(Localization.Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            FileVersionController.Instance.DeleteAllUnpublishedVersions(file, false);
            
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
