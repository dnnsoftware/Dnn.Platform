#region Copyright

// 
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
using System.Globalization;
using System.Net;
using System.Web.Http;
using System.Net.Http;
using DotNetNuke.Common;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
    public class UserFileController : DnnApiController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (UserFileController));
        private readonly IFolderManager _folderManager = FolderManager.Instance;

        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetItems()
        {
            return GetItems(null);
        }
        
        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetItems(string fileExtensions)
        {
            try
            {
                var userFolder = _folderManager.GetUserFolder(UserInfo);
                var extensions = new List<string>();

                if (!string.IsNullOrEmpty(fileExtensions))
                {
                    fileExtensions = fileExtensions.ToLowerInvariant();
                    extensions.AddRange(fileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }

                var folderStructure = new Item
                {
                    children = GetChildren(userFolder, extensions),
                    folder = true,
                    id = userFolder.FolderID,
                    name = Localization.GetString("UserFolderTitle.Text", Localization.SharedResourceFile)
                };

                return Request.CreateResponse(HttpStatusCode.OK, new List<Item> { folderStructure });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        // ReSharper disable LoopCanBeConvertedToQuery
        private List<Item> GetChildren(IFolderInfo folder, ICollection<string> extensions)
        {
            var everything = new List<Item>();

            var folders = _folderManager.GetFolders(folder);

            foreach (var currentFolder in folders)
            {
                everything.Add(new Item
                {
                    id = currentFolder.FolderID,
                    name = currentFolder.DisplayName ?? currentFolder.FolderName,
                    folder = true,
                    parentId = folder.FolderID,
                    children = GetChildren(currentFolder, extensions)
                });
            }

            var files = _folderManager.GetFiles(folder);

            foreach (var file in files)
            {
                // list is empty or contains the file extension in question
                if (extensions.Count == 0 || extensions.Contains(file.Extension.ToLowerInvariant()))
                {
                    everything.Add(new Item
                    {
                        id = file.FileId,
                        name = file.FileName,
                        folder = false,
                        parentId = file.FolderId,
                        thumb_url = GetThumbUrl(file),
                        type = GetTypeName(file),
                        size = GetFileSize(file.Size),
                        modified = GetModifiedTime(file.LastModificationTime)
                    });
                }
            }

            return everything;
        }

        private static string GetModifiedTime(DateTime dateTime)
        {
            return string.Format("{0:MMM} {0:dd}, {0:yyyy} at {0:t}", dateTime);
        }

        // ReSharper restore LoopCanBeConvertedToQuery

        private string GetThumbUrl(IFileInfo file)
        {
            if (IsImageFile(file.RelativePath))
            {
                return FileManager.Instance.GetUrl(file);
            }

            var fileIcon = IconController.IconURL("Ext" + file.Extension, "32x32");
            if (!System.IO.File.Exists(Request.GetHttpContext().Server.MapPath(fileIcon)))
            {
                fileIcon = IconController.IconURL("File", "32x32");
            }
            return fileIcon;
        }

        private static string GetTypeName(IFileInfo file)
        {
            return file.ContentType == null
                       ? string.Empty
                       : (file.ContentType.StartsWith("image/") 
                            ? file.ContentType.Replace("image/", string.Empty) 
                            : (file.Extension != null ? file.Extension.ToLowerInvariant() : string.Empty));
        }

        private static bool IsImageFile(string relativePath)
        {
            var acceptedExtensions = new List<string> { "jpg", "png", "gif", "jpe", "jpeg", "tiff" };
            var extension = relativePath.Substring(relativePath.LastIndexOf(".", StringComparison.Ordinal) + 1).ToLower();
            return acceptedExtensions.Contains(extension);
        }

        private static string GetFileSize(int sizeInBytes)
        {
            var size = sizeInBytes / 1024;
            var biggerThanAMegabyte = size > 1024;
            if (biggerThanAMegabyte)
            {
                size = (size / 1024);
            }
            return size.ToString(CultureInfo.InvariantCulture) + (biggerThanAMegabyte ? "Mb" : "k");
        }

        class Item
        {
            // ReSharper disable InconsistentNaming
            // ReSharper disable UnusedAutoPropertyAccessor.Local
            public int id { get; set; }
            public string name { get; set; }
            public bool folder { get; set; }
            public int parentId { get; set; }
            public string thumb_url { get; set; }
            public string type { get; set; }
            public string size { get; set; }
            public string modified { get; set; }
            public List<Item> children { get; set; }
            // ReSharper restore UnusedAutoPropertyAccessor.Local
            // ReSharper restore InconsistentNaming
        }
    }
}