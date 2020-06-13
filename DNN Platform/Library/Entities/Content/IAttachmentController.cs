// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content
{
    using System.Collections.Generic;

    using DotNetNuke.Services.FileSystem;

    /// <summary>Interface of FileController.</summary>
    /// <seealso cref="AttachmentController"/>
    public interface IAttachmentController
    {
        /// <summary>
        /// Add a generic file to a <see cref="ContentItem"/>.
        /// </summary>
        /// <param name="contentItemId">The content item.</param>
        /// <param name="fileInfo">A file registered in the DotNetNuke. <seealso cref="FileManager"/></param>
        void AddFileToContent(int contentItemId, IFileInfo fileInfo);

        void AddFilesToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo);

        /// <summary>
        /// Add a video file to a <see cref="ContentItem"/>.
        /// </summary>
        /// <param name="contentItemId">The content item.</param>
        /// <param name="fileInfo">A file registered in the DotNetNuke. <seealso cref="FileManager"/></param>
        void AddVideoToContent(int contentItemId, IFileInfo fileInfo);

        void AddVideosToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo);

        /// <summary>
        /// Attach an image to a ContentItem.
        /// </summary>
        /// <param name="contentItemId">The content item.</param>
        /// <param name="fileInfo">A file registered in the DotNetNuke. <seealso cref="FileManager"/></param>
        void AddImageToContent(int contentItemId, IFileInfo fileInfo);

        void AddImagesToContent(int contentItemId, IEnumerable<IFileInfo> fileInfo);

        IList<IFileInfo> GetFilesByContent(int contentItemId);

        IList<IFileInfo> GetVideosByContent(int contentItemId);

        IList<IFileInfo> GetImagesByContent(int contentItemId);
    }
}
