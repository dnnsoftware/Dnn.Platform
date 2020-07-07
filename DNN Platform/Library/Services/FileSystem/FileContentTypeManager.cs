// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common.Lists;
    using DotNetNuke.ComponentModel;

    public class FileContentTypeManager : ComponentBase<IFileContentTypeManager, FileContentTypeManager>, IFileContentTypeManager
    {
        private static readonly object _threadLocker = new object();
        private IDictionary<string, string> _contentTypes;

        public virtual IDictionary<string, string> ContentTypes
        {
            get
            {
                if (this._contentTypes == null)
                {
                    lock (_threadLocker)
                    {
                        if (this._contentTypes == null)
                        {
                            var listController = new ListController();
                            var listEntries = listController.GetListEntryInfoItems("ContentTypes");
                            if (listEntries == null || !listEntries.Any())
                            {
                                this._contentTypes = this.GetDefaultContentTypes();
                            }
                            else
                            {
                                this._contentTypes = new Dictionary<string, string>();
                                if (listEntries != null)
                                {
                                    foreach (var contentTypeEntry in listEntries)
                                    {
                                        this._contentTypes.Add(contentTypeEntry.Value, contentTypeEntry.Text);
                                    }
                                }
                            }
                        }
                    }
                }

                return this._contentTypes;
            }
        }

        public virtual string GetContentType(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return "application/octet-stream";
            }

            var key = extension.TrimStart('.').ToLowerInvariant();
            return this.ContentTypes.ContainsKey(key) ? this.ContentTypes[key] : "application/octet-stream";
        }

        private Dictionary<string, string> GetDefaultContentTypes()
        {
            var contentTypes = new Dictionary<string, string>();
            contentTypes.Add("txt", "text/plain");
            contentTypes.Add("htm", "text/html");
            contentTypes.Add("html", "text/html");
            contentTypes.Add("rtf", "text/richtext");
            contentTypes.Add("jpg", "image/jpeg");
            contentTypes.Add("jpeg", "image/jpeg");
            contentTypes.Add("gif", "image/gif");
            contentTypes.Add("bmp", "image/bmp");
            contentTypes.Add("png", "image/png");
            contentTypes.Add("ico", "image/x-icon");
            contentTypes.Add("svg", "image/svg+xml");
            contentTypes.Add("ttf", "font/ttf");
            contentTypes.Add("eot", "application/vnd.ms-fontobject");
            contentTypes.Add("woff", "application/font-woff");
            contentTypes.Add("mp3", "audio/mpeg");
            contentTypes.Add("wma", "audio/x-ms-wma");
            contentTypes.Add("mpg", "video/mpeg");
            contentTypes.Add("mpeg", "video/mpeg");
            contentTypes.Add("avi", "video/avi");
            contentTypes.Add("mp4", "video/mp4");
            contentTypes.Add("wmv", "video/x-ms-wmv");
            contentTypes.Add("pdf", "application/pdf");
            contentTypes.Add("doc", "application/msword");
            contentTypes.Add("dot", "application/msword");
            contentTypes.Add("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
            contentTypes.Add("dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template");
            contentTypes.Add("csv", "text/csv");
            contentTypes.Add("xls", "application/x-msexcel");
            contentTypes.Add("xlt", "application/x-msexcel");
            contentTypes.Add("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            contentTypes.Add("xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template");
            contentTypes.Add("ppt", "application/vnd.ms-powerpoint");
            contentTypes.Add("pps", "application/vnd.ms-powerpoint");
            contentTypes.Add("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
            contentTypes.Add("ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow");

            return contentTypes;
        }
    }
}
