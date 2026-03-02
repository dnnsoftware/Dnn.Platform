// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    using Microsoft.Extensions.DependencyInjection;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class FileDeletionController(IFileLockingController fileLockingController, IFileVersionController fileVersionController, IFolderMappingController folderMappingController, IContentController contentController, DataProvider dataProvider)
        : ServiceLocator<IFileDeletionController, FileDeletionController>, IFileDeletionController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileDeletionController));
        private readonly IFileLockingController fileLockingController = fileLockingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileLockingController>();
        private readonly IFileVersionController fileVersionController = fileVersionController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileVersionController>();
        private readonly IFolderMappingController folderMappingController = folderMappingController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderMappingController>();
        private readonly IContentController contentController = contentController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IContentController>();
        private readonly DataProvider dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();

        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IFileLockingController. Scheduled removal in v12.0.0.")]
        public FileDeletionController()
            : this(null, null, null, null, null)
        {
        }

        /// <inheritdoc />
        public void DeleteFile(IFileInfo file)
        {
            if (this.fileLockingController.IsFileLocked(file, out var lockReason))
            {
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            this.fileVersionController.DeleteAllUnpublishedVersions(file, false);
            try
            {
                var folderMapping = this.folderMappingController.GetFolderMapping(file.PortalId, file.FolderMappingID);
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

        /// <inheritdoc />
        public void UnlinkFile(IFileInfo file)
        {
            if (this.fileLockingController.IsFileLocked(file, out var lockReason))
            {
                throw new FileLockedException(Localization.GetExceptionMessage(lockReason, "File locked. The file cannot be updated. Reason: " + lockReason));
            }

            this.fileVersionController.DeleteAllUnpublishedVersions(file, false);

            this.DeleteFileData(file);
        }

        /// <inheritdoc />
        public void DeleteFileData(IFileInfo file)
        {
            this.dataProvider.DeleteFile(file.PortalId, file.FileName, file.FolderId);
            this.DeleteContentItem(file.ContentItemID);
        }

        /// <inheritdoc />
        protected override Func<IFileDeletionController> GetFactory()
        {
            return () => Globals.DependencyProvider.GetRequiredService<IFileDeletionController>();
        }

        private void DeleteContentItem(int contentItemId)
        {
            if (contentItemId == Null.NullInteger)
            {
                return;
            }

            this.contentController.DeleteContentItem(contentItemId);
        }
    }
}
