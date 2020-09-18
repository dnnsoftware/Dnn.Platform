// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;
    using Newtonsoft.Json;

    public class FileUploadController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUploadController));
        private readonly IFileManager _fileManager = FileManager.Instance;
        private readonly IFolderManager _folderManager = FolderManager.Instance;

        [DnnAuthorize]
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage UploadFile()
        {
            var statuses = new List<FilesStatus>();
            try
            {
                // todo can we eliminate the HttpContext here
                this.UploadWholeFile(HttpContextSource.Current, statuses);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return this.IframeSafeJson(statuses);
        }

        private HttpResponseMessage IframeSafeJson(List<FilesStatus> statuses)
        {
            // return json but label it as plain text
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(statuses)),
            };
        }

        // Upload entire file
        private void UploadWholeFile(HttpContextBase context, ICollection<FilesStatus> statuses)
        {
            for (var i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                if (file == null)
                {
                    continue;
                }

                var fileName = Path.GetFileName(file.FileName);

                try
                {
                    var userFolder = this._folderManager.GetUserFolder(this.UserInfo);

                    // todo: deal with the case where the exact file name already exists.
                    var fileInfo = this._fileManager.AddFile(userFolder, fileName, file.InputStream, true);
                    var fileIcon = Entities.Icons.IconController.IconURL("Ext" + fileInfo.Extension, "32x32");
                    if (!File.Exists(context.Server.MapPath(fileIcon)))
                    {
                        fileIcon = Entities.Icons.IconController.IconURL("File", "32x32");
                    }

                    statuses.Add(new FilesStatus
                    {
                        success = true,
                        name = fileName,
                        extension = fileInfo.Extension,
                        type = fileInfo.ContentType,
                        size = file.ContentLength,
                        progress = "1.0",
                        url = FileManager.Instance.GetUrl(fileInfo),
                        thumbnail_url = fileIcon,
                        message = "success",
                        id = fileInfo.FileId,
                    });
                }
                catch (InvalidFileExtensionException)
                {
                    statuses.Add(new FilesStatus
                    {
                        success = false,
                        name = fileName,
                        message = "File type not allowed.",
                    });
                }
            }
        }
    }
}
