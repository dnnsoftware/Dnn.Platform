// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;

    /// <summary>Implementation of <see cref="IAttachmentController"/>.</summary>
    public class AttachmentController : IAttachmentController
    {
        internal const string FilesKey = "Files";
        internal const string ImageKey = "Images";
        internal const string VideoKey = "Videos";
        internal const string TitleKey = "Title";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AttachmentController));

        private readonly IContentController _contentController;

        public AttachmentController()
            : this(Util.GetContentController())
        {
        }

        public AttachmentController(IContentController contentController)
        {
            this._contentController = contentController;
        }

        public void AddFileToContent(int contentItemId, IFileInfo fileInfo)
        {
            this.AddFilesToContent(contentItemId, new[] { fileInfo });
        }

        public void AddFilesToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo)
        {
            this.AddToContent(contentItemId, contentItem => contentItem.Files.AddRange(fileInfo));
        }

        public void AddVideoToContent(int contentItemId, IFileInfo fileInfo)
        {
            this.AddVideosToContent(contentItemId, new[] { fileInfo });
        }

        public void AddVideosToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo)
        {
            this.AddToContent(contentItemId, contentItem => contentItem.Videos.AddRange(fileInfo));
        }

        public void AddImageToContent(int contentItemId, IFileInfo fileInfo)
        {
            this.AddImagesToContent(contentItemId, new[] { fileInfo });
        }

        public void AddImagesToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo)
        {
            this.AddToContent(contentItemId, contentItem => contentItem.Images.AddRange(fileInfo));
        }

        public IList<IFileInfo> GetVideosByContent(int contentItemId)
        {
            var files = this.GetFilesByContent(contentItemId, VideoKey);

            return files.Select(fileId => FileManager.Instance.GetFile(fileId)).ToList();
        }

        public IList<IFileInfo> GetImagesByContent(int contentItemId)
        {
            var files = this.GetFilesByContent(contentItemId, ImageKey);

            return files.Select(fileId => FileManager.Instance.GetFile(fileId)).ToList();
        }

        public IList<IFileInfo> GetFilesByContent(int contentItemId)
        {
            var files = this.GetFilesByContent(contentItemId, FilesKey);

            return files.Select(fileId => FileManager.Instance.GetFile(fileId)).ToList();
        }

        internal static void SerializeAttachmentMetadata(ContentItem contentItem)
        {
            SerializeToMetadata(contentItem.Files, contentItem.Metadata, FilesKey);
            SerializeToMetadata(contentItem.Videos, contentItem.Metadata, VideoKey);
            SerializeToMetadata(contentItem.Images, contentItem.Metadata, ImageKey);
        }

        internal static IEnumerable<IFileInfo> DeserializeFileInfo(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                yield break;
            }

            foreach (var file in content.FromJson<int[]>().ToArray())
            {
                IFileInfo fileInfo = null;
                try
                {
                    fileInfo = FileManager.Instance.GetFile(file);
                }
                catch
                {
                    // throw new ApplicationException(string.Format("Error loading file properties for FileID '{0}'", file), ex);

                    // On second thought, I don't know how much sense it makes to be throwing an exception here.  If the file
                    // has been deleted or is otherwise unavailable, there's really no reason we can't continue on handling the
                    // ContentItem without its attachment.  Better than the yellow screen of death? --cbond
                    Logger.WarnFormat("Unable to load file properties for File ID {0}", file);
                }

                if (fileInfo != null)
                {
                    yield return fileInfo;
                }
            }
        }

        internal static string SerializeFileInfo(IEnumerable<IFileInfo> files)
        {
            var fileList = files.Select(x => x.FileId).ToArray();
            if (fileList.Length == 0)
            {
                return null;
            }

            return fileList.ToJson();
        }

        private static void SerializeToMetadata(IList<IFileInfo> files, NameValueCollection nvc, string key)
        {
            var remove = !files.Any();
            if (remove == false)
            {
                var serialized = SerializeFileInfo(files);

                if (string.IsNullOrEmpty(serialized))
                {
                    remove = true;
                }
                else
                {
                    nvc[key] = serialized;
                }
            }

            if (remove)
            {
                nvc.Remove(key);
            }
        }

        private void AddToContent(int contentItemId, Action<ContentItem> action)
        {
            var contentItem = this._contentController.GetContentItem(contentItemId);

            action(contentItem);

            this._contentController.UpdateContentItem(contentItem);
        }

        private IEnumerable<int> GetFilesByContent(int contentItemId, string type)
        {
            var contentItem = this._contentController.GetContentItem(contentItemId);
            if (contentItem == null)
            {
                throw new ApplicationException(string.Format("Cannot find ContentItem ID {0}", contentItemId));
            }

            var serialized = contentItem.Metadata[type];

            if (string.IsNullOrEmpty(serialized))
            {
                return new int[0];
            }

            try
            {
                return serialized.FromJson<int[]>().ToArray();
            }
            catch (FormatException ex)
            {
                throw new ApplicationException(
                    string.Format("ContentItem metadata has become corrupt (ID {0}): invalid file ID", contentItemId), ex);
            }
        }
    }
}
