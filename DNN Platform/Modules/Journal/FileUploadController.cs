#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

namespace DotNetNuke.Modules.Journal
{
    public class FileUploadController : DnnApiController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (FileUploadController));

        [DnnAuthorize]
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage UploadFile()
        {
            var statuses = new List<FilesStatus>();
            try
            {
                //todo can we eliminate the HttpContext here
                UploadWholeFile(HttpContextSource.Current, statuses);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return IframeSafeJson(statuses);
        }

        private HttpResponseMessage IframeSafeJson(List<FilesStatus> statuses)
        {
            //return json but label it as plain text
            return new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(statuses))
            };
        }

        private static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG", ".JPEG", ".ICO", ".SVG" };

        private static bool IsImageExtension(string extension)
        {
            return ImageExtensions.Contains(extension.ToUpper());
        }

        // Upload entire file
        private void UploadWholeFile(HttpContextBase context, ICollection<FilesStatus> statuses)
        {
            for (var i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];
                if (file == null) continue;

                var fileName = Path.GetFileName(file.FileName);
                //fix any filename issues that would cause double escaping exceptions
                if (IsImageExtension(Path.GetExtension(fileName)))
                {
                    fileName = fileName.Replace("+", ""); 
                }
                
                try
                {
                    var fileInfo = JournalController.Instance.SaveJourmalFile(ActiveModule, UserInfo, fileName, file.InputStream);
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
                        message = "File type not allowed."
                    });
                }
            }
        }
    }
}
