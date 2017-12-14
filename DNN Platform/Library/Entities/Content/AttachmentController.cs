#region Copyright
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

namespace DotNetNuke.Entities.Content
{
    /// <summary>Implementation of <see cref="IAttachmentController"/>.</summary>
    public class AttachmentController : IAttachmentController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (AttachmentController));
        public AttachmentController()
            : this(Util.GetContentController())
        {
        }

        public AttachmentController(IContentController contentController)
        {
            _contentController = contentController;
        }

        private readonly IContentController _contentController;

        #region Implementation of IFileController

        private void AddToContent(int contentItemId, Action<ContentItem> action)
        {
            var contentItem = _contentController.GetContentItem(contentItemId);

            action(contentItem);
            
            _contentController.UpdateContentItem(contentItem);
        }

        public void AddFileToContent(int contentItemId, IFileInfo fileInfo)
        {
            AddFilesToContent(contentItemId, new[] { fileInfo });
        }

        public void AddFilesToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo)
        {
            AddToContent(contentItemId, contentItem => contentItem.Files.AddRange(fileInfo));
        }
        
        public void AddVideoToContent(int contentItemId, IFileInfo fileInfo)
        {
            AddVideosToContent(contentItemId, new[] { fileInfo });
        }

        public void AddVideosToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo)
        {
            AddToContent(contentItemId, contentItem => contentItem.Videos.AddRange(fileInfo));
        }

        public void AddImageToContent(int contentItemId, IFileInfo fileInfo)
        {
            AddImagesToContent(contentItemId, new[] { fileInfo });
        }

        public void AddImagesToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo)
        {
            AddToContent(contentItemId, contentItem => contentItem.Images.AddRange(fileInfo));
        }

        public IList<IFileInfo> GetVideosByContent(int contentItemId)
        {
            var files = GetFilesByContent(contentItemId, VideoKey);

            return files.Select(fileId => FileManager.Instance.GetFile(fileId)).ToList();
        }

        public IList<IFileInfo> GetImagesByContent(int contentItemId)
        {
            var files = GetFilesByContent(contentItemId, ImageKey);

            return files.Select(fileId => FileManager.Instance.GetFile(fileId)).ToList();
        }

        public IList<IFileInfo> GetFilesByContent(int contentItemId)
        {
            var files = GetFilesByContent(contentItemId, FilesKey);

            return files.Select(fileId => FileManager.Instance.GetFile(fileId)).ToList();
        }

        #endregion

        #region Internal utility methods

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

        internal static void SerializeAttachmentMetadata(ContentItem contentItem)
        {
            SerializeToMetadata(contentItem.Files, contentItem.Metadata, FilesKey);
            SerializeToMetadata(contentItem.Videos, contentItem.Metadata, VideoKey);
            SerializeToMetadata(contentItem.Images, contentItem.Metadata, ImageKey);
        }

        private IEnumerable<int> GetFilesByContent(int contentItemId, string type)
        {
            var contentItem = _contentController.GetContentItem(contentItemId);
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

        #endregion

        #region Private

        internal const string FilesKey = "Files";
        internal const string ImageKey = "Images";
        internal const string VideoKey = "Videos";
        internal const string TitleKey = "Title";

        #endregion
    }
}