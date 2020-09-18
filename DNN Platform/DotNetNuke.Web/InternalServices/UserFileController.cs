// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    public class UserFileController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserFileController));
        private readonly IFolderManager _folderManager = FolderManager.Instance;

        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetItems()
        {
            return this.GetItems(null);
        }

        [DnnAuthorize]
        [HttpGet]
        public HttpResponseMessage GetItems(string fileExtensions)
        {
            try
            {
                var userFolder = this._folderManager.GetUserFolder(this.UserInfo);
                var extensions = new List<string>();

                if (!string.IsNullOrEmpty(fileExtensions))
                {
                    fileExtensions = fileExtensions.ToLowerInvariant();
                    extensions.AddRange(fileExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
                }

                var folderStructure = new Item
                {
                    children = this.GetChildren(userFolder, extensions),
                    folder = true,
                    id = userFolder.FolderID,
                    name = Localization.GetString("UserFolderTitle.Text", Localization.SharedResourceFile),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, new List<Item> { folderStructure });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static string GetModifiedTime(DateTime dateTime)
        {
            return string.Format("{0:MMM} {0:dd}, {0:yyyy} at {0:t}", dateTime);
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
            var extension = relativePath.Substring(relativePath.LastIndexOf(".", StringComparison.Ordinal) + 1).ToLowerInvariant();
            return acceptedExtensions.Contains(extension);
        }

        private static string GetFileSize(int sizeInBytes)
        {
            var size = sizeInBytes / 1024;
            var biggerThanAMegabyte = size > 1024;
            if (biggerThanAMegabyte)
            {
                size = size / 1024;
            }

            return size.ToString(CultureInfo.InvariantCulture) + (biggerThanAMegabyte ? "Mb" : "k");
        }

        private string GetThumbUrl(IFileInfo file)
        {
            if (IsImageFile(file.RelativePath))
            {
                return FileManager.Instance.GetUrl(file);
            }

            var fileIcon = IconController.IconURL("Ext" + file.Extension, "32x32");
            if (!System.IO.File.Exists(this.Request.GetHttpContext().Server.MapPath(fileIcon)))
            {
                fileIcon = IconController.IconURL("File", "32x32");
            }

            return fileIcon;
        }

        private List<Item> GetChildren(IFolderInfo folder, ICollection<string> extensions)
        {
            var everything = new List<Item>();

            var folders = this._folderManager.GetFolders(folder);

            foreach (var currentFolder in folders)
            {
                everything.Add(new Item
                {
                    id = currentFolder.FolderID,
                    name = currentFolder.DisplayName ?? currentFolder.FolderName,
                    folder = true,
                    parentId = folder.FolderID,
                    children = this.GetChildren(currentFolder, extensions),
                });
            }

            var files = this._folderManager.GetFiles(folder);

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
                        thumb_url = this.GetThumbUrl(file),
                        type = GetTypeName(file),
                        size = GetFileSize(file.Size),
                        modified = GetModifiedTime(file.LastModificationTime),
                    });
                }
            }

            return everything;
        }

        private class Item
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

        // ReSharper disable LoopCanBeConvertedToQuery

        // ReSharper restore LoopCanBeConvertedToQuery
    }
}
