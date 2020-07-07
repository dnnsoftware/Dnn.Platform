// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class FileDeletionController : ServiceLocator<IFileDeletionController, FileDeletionController>, IFileDeletionController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileDeletionController));

        public void DeleteFile(IFileInfo file)
        {
            string lockReason;
            if (FileLockingController.Instance.IsFileLocked(file, out lockReason))
            {
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
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
                throw new FolderProviderException(Localization.GetExceptionMessage("DeleteFileUnderlyingSystemError", "The underlying system threw an exception. The file has not been deleted."), ex);
            }

            this.DeleteFileData(file);

            DataCache.RemoveCache("GetFileById" + file.FileId);
        }

        public void UnlinkFile(IFileInfo file)
        {
            string lockReason;
            if (FileLockingController.Instance.IsFileLocked(file, out lockReason))
            {
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            FileVersionController.Instance.DeleteAllUnpublishedVersions(file, false);

            this.DeleteFileData(file);
        }

        public void DeleteFileData(IFileInfo file)
        {
            DataProvider.Instance().DeleteFile(file.PortalId, file.FileName, file.FolderId);
            this.DeleteContentItem(file.ContentItemID);
        }

        protected override Func<IFileDeletionController> GetFactory()
        {
            return () => new FileDeletionController();
        }

        private void DeleteContentItem(int contentItemId)
        {
            if (contentItemId == Null.NullInteger)
            {
                return;
            }

            Util.GetContentController().DeleteContentItem(contentItemId);
        }
    }
}
