// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
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
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;
    using Newtonsoft.Json;

    public class FileUploadController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUploadController));

        private static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO", ".SVG" };

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

        private static bool IsImageExtension(string extension)
        {
            return ImageExtensions.Contains(extension.ToUpper());
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

                // fix any filename issues that would cause double escaping exceptions
                if (IsImageExtension(Path.GetExtension(fileName)))
                {
                    fileName = fileName.Replace("+", string.Empty);
                }

                try
                {
                    var fileInfo = JournalController.Instance.SaveJourmalFile(this.ActiveModule, this.UserInfo, fileName, file.InputStream);
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
                        file_id = fileInfo.FileId,
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
