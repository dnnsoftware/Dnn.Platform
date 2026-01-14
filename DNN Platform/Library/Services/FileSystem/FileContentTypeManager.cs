// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.ComponentModel;

    public class FileContentTypeManager : ComponentBase<IFileContentTypeManager, FileContentTypeManager>, IFileContentTypeManager
    {
        private static readonly object ThreadLocker = new object();
        private Dictionary<string, string> contentTypes;

        /// <inheritdoc/>
        public virtual IDictionary<string, string> ContentTypes
        {
            get
            {
                if (this.contentTypes == null)
                {
                    lock (ThreadLocker)
                    {
                        if (this.contentTypes == null)
                        {
                            var listController = new ListController();
                            var listEntries = listController.GetListEntryInfoItems("ContentTypes");
                            if (listEntries == null || !listEntries.Any())
                            {
                                this.contentTypes = GetDefaultContentTypes();
                            }
                            else
                            {
                                this.contentTypes = new Dictionary<string, string>();
                                if (listEntries != null)
                                {
                                    foreach (var contentTypeEntry in listEntries)
                                    {
                                        this.contentTypes.Add(contentTypeEntry.Value, contentTypeEntry.Text);
                                    }
                                }
                            }
                        }
                    }
                }

                return this.contentTypes;
            }
        }

        /// <inheritdoc/>
        public virtual string GetContentType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream";
            }

            var key = extension.TrimStart('.').ToLowerInvariant();
            return this.ContentTypes.TryGetValue(key, out var contentType) ? contentType : "application/octet-stream";
        }

        private static Dictionary<string, string> GetDefaultContentTypes()
        {
            return new Dictionary<string, string>
            {
                { "txt", "text/plain" },
                { "htm", "text/html" },
                { "html", "text/html" },
                { "rtf", "text/richtext" },
                { "jpg", "image/jpeg" },
                { "jpeg", "image/jpeg" },
                { "gif", "image/gif" },
                { "bmp", "image/bmp" },
                { "png", "image/png" },
                { "ico", "image/x-icon" },
                { "svg", "image/svg+xml" },
                { "ttf", "font/ttf" },
                { "eot", "application/vnd.ms-fontobject" },
                { "woff", "application/font-woff" },
                { "mp3", "audio/mpeg" },
                { "wma", "audio/x-ms-wma" },
                { "mpg", "video/mpeg" },
                { "mpeg", "video/mpeg" },
                { "avi", "video/avi" },
                { "mp4", "video/mp4" },
                { "wmv", "video/x-ms-wmv" },
                { "pdf", "application/pdf" },
                { "doc", "application/msword" },
                { "dot", "application/msword" },
                { "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { "dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
                { "csv", "text/csv" },
                { "xls", "application/x-msexcel" },
                { "xlt", "application/x-msexcel" },
                { "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { "xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
                { "ppt", "application/vnd.ms-powerpoint" },
                { "pps", "application/vnd.ms-powerpoint" },
                { "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { "ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow" },
                { "css", "text/css" },
            };
        }
    }
}
