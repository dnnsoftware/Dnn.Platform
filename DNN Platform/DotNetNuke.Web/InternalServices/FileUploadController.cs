#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.UI.WebControls;
using ClientDependency.Core;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class FileUploadController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUploadController));
        private static Regex _userFolderEx = new Regex("users/\\d+/\\d+/(\\d+)/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public class FolderItemDTO
        {
            public string FolderPath { get; set; }
            public string FileFilter { get; set; }
            public bool Required { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage LoadFiles(FolderItemDTO folderItem)
        {
            int effectivePortalId = PortalSettings.PortalId;
            if (string.IsNullOrEmpty(folderItem.FolderPath))
            {
                folderItem.FolderPath = "";
            }

            int userId;
            if (IsUserFolder(folderItem.FolderPath, out userId))
            {
                var user = UserController.GetUserById(effectivePortalId, userId);
                if (user != null && user.IsSuperUser)
                {
                    effectivePortalId = Null.NullInteger;
                }
                else
                {
                    effectivePortalId = PortalController.GetEffectivePortalId(effectivePortalId);
                }
            }

            var list = Globals.GetFileList(effectivePortalId, folderItem.FileFilter, !folderItem.Required, folderItem.FolderPath);
            var fileItems = list.OfType<FileItem>().ToList();

            return Request.CreateResponse(HttpStatusCode.OK, fileItems);
        }

        [HttpGet]
        public HttpResponseMessage LoadImage(string fileId)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                int file;
                if (int.TryParse(fileId, out file))
                {
                    var imageUrl = ShowImage(file);
                    return Request.CreateResponse(HttpStatusCode.OK, imageUrl);
                }
            }

            return Request.CreateResponse(HttpStatusCode.InternalServerError);
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public Task<HttpResponseMessage> PostFile()
        {
            HttpRequestMessage request = Request;

            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType); 
            }

            var provider = new MultipartMemoryStreamProvider();

            // local references for use in closure
            var portalSettings = PortalSettings;
            var currentSynchronizationContext = SynchronizationContext.Current;
            var userInfo = UserInfo;
            var task = request.Content.ReadAsMultipartAsync(provider)
                .ContinueWith(o =>
                    {
                        string folder = string.Empty;
                        string filter = string.Empty;
                        string fileName = string.Empty;
                        bool overwrite = false;
                        bool isHostMenu = false;
                        bool extract = false;
                        Stream stream = null;
                        string returnFilename = string.Empty;

                        foreach (var item in provider.Contents)
                        {
                            var name = item.Headers.ContentDisposition.Name;
                            switch (name.ToUpper())
                            {
                                case "\"FOLDER\"":
                                    folder = item.ReadAsStringAsync().Result ?? "";
                                    break;

                                case "\"FILTER\"":
                                    filter = item.ReadAsStringAsync().Result ?? "";
                                    break;

                                case "\"OVERWRITE\"":
                                    bool.TryParse(item.ReadAsStringAsync().Result, out overwrite);
                                    break;

                                case "\"ISHOSTMENU\"":
                                    bool.TryParse(item.ReadAsStringAsync().Result, out isHostMenu);
                                    break;

                                case "\"EXTRACT\"":
                                    bool.TryParse(item.ReadAsStringAsync().Result, out extract);
                                    break;

                                case "\"POSTFILE\"":
                                    fileName = item.Headers.ContentDisposition.FileName.Replace("\"", "");
                                    if (fileName.IndexOf("\\", StringComparison.Ordinal) != -1)
                                    {
                                        fileName = Path.GetFileName(fileName);
                                    }
                                    stream = item.ReadAsStreamAsync().Result;
                                    break;
                            }
                        }

                        var errorMessage = "";
                        var alreadyExists = false;
                        if (!string.IsNullOrEmpty(fileName) && stream != null)
                        {
                            // Everything ready
                            
                            // The SynchronizationContext keeps the main thread context. Send method is synchronous
                            currentSynchronizationContext.Send(
                                delegate
                                    {
                                        returnFilename = SaveFile(stream, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, out alreadyExists, out errorMessage);
                                    },null
                                );
                            
                        }

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            /* Response Content Type cannot be application/json 
                             * because IE9 with iframe-transport manages the response 
                             * as a file download 
                             */
                            var mediaTypeFormatter = new JsonMediaTypeFormatter();
                            mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                            return Request.CreateResponse(
                                HttpStatusCode.OK,
                                new
                                {
                                    AlreadyExists = alreadyExists,
                                    Message = string.Format(GetLocalizedString("ErrorMessage"), fileName, errorMessage)
                                }, mediaTypeFormatter, "text/plain");
                        }

                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        returnFilename = returnFilename.Replace(root, "~/");
                        returnFilename = VirtualPathUtility.ToAbsolute(returnFilename);

                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(returnFilename)
                        };

                    });

            return task; 
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public Task<HttpResponseMessage> UploadFromLocal()
        {
            HttpRequestMessage request = Request;

            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType); 
            }

            var provider = new MultipartMemoryStreamProvider();

            // local references for use in closure
            var portalSettings = PortalSettings;
            var currentSynchronizationContext = SynchronizationContext.Current;
            var userInfo = UserInfo;
            var task = request.Content.ReadAsMultipartAsync(provider)
                .ContinueWith(o =>
                    {
                        string folder = string.Empty;
                        string filter = string.Empty;
                        string fileName = string.Empty;
                        bool overwrite = false;
                        bool isHostMenu = false;
                        bool extract = false;
                        Stream stream = null;
                        string returnFilename = string.Empty;

                        foreach (var item in provider.Contents)
                        {
                            var name = item.Headers.ContentDisposition.Name;
                            switch (name.ToUpper())
                            {
                                case "\"FOLDER\"":
                                    folder = item.ReadAsStringAsync().Result ?? "";
                                    break;

                                case "\"FILTER\"":
                                    filter = item.ReadAsStringAsync().Result ?? "";
                                    break;

                                case "\"OVERWRITE\"":
                                    bool.TryParse(item.ReadAsStringAsync().Result, out overwrite);
                                    break;

                                case "\"ISHOSTMENU\"":
                                    bool.TryParse(item.ReadAsStringAsync().Result, out isHostMenu);
                                    break;

                                case "\"EXTRACT\"":
                                    bool.TryParse(item.ReadAsStringAsync().Result, out extract);
                                    break;

                                case "\"POSTFILE\"":
                                    fileName = item.Headers.ContentDisposition.FileName.Replace("\"", "");
                                    if (fileName.IndexOf("\\", StringComparison.Ordinal) != -1)
                                    {
                                        fileName = Path.GetFileName(fileName);
                                    }
                                    stream = item.ReadAsStreamAsync().Result;
                                    break;
                            }
                        }

                        var errorMessage = "";
                        var alreadyExists = false;
                        if (!string.IsNullOrEmpty(fileName) && stream != null)
                        {
                            // Everything ready
                            
                            // The SynchronizationContext keeps the main thread context. Send method is synchronous
                            currentSynchronizationContext.Send(
                                delegate
                                    {
                                        returnFilename = SaveFile(stream, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, out alreadyExists, out errorMessage);
                                    },null
                                );
                            
                        }

                        var mediaTypeFormatter = new JsonMediaTypeFormatter();
                        mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                        var root = AppDomain.CurrentDomain.BaseDirectory;
                        returnFilename = returnFilename.Replace(root, "~/");

                        var size = IsImage(returnFilename) ?
                            ImageHeader.GetDimensions(HostingEnvironment.MapPath(returnFilename)) :
                            Size.Empty;

                        /* Response Content Type cannot be application/json 
                         * because IE9 with iframe-transport manages the response 
                         * as a file download 
                         */
                        return Request.CreateResponse(
                            HttpStatusCode.OK,
                            new UploadDto { AlreadyExists = alreadyExists, Message = errorMessage, Orientation = size.Orientation(), Path = VirtualPathUtility.ToAbsolute(returnFilename) },
                            mediaTypeFormatter,
                            "text/plain");
                    });

            return task; 
        }

        private static string SaveFile(Stream stream, PortalSettings portalSettings, UserInfo userInfo, string folder, string filter, string fileName, bool overwrite, bool isHostMenu, bool extract, out bool alreadyExists, out string errorMessage)
        {
            alreadyExists = false;
            try
            {
                var extension = Path.GetExtension(fileName).Replace(".", "");
                if (!string.IsNullOrEmpty(filter) && !filter.ToLower().Contains(extension.ToLower()))
                {
                    errorMessage = GetLocalizedString("ExtensionNotAllowed");
                    return string.Empty;
                }

                if (!IsAllowedExtension(extension))
                {
                    errorMessage = GetLocalizedString("ExtensionNotAllowed");
                    return string.Empty;
                }

                var folderManager = FolderManager.Instance;

                // Check if this is a User Folder
                var effectivePortalId = isHostMenu ? Null.NullInteger : PortalController.GetEffectivePortalId(portalSettings.PortalId);
                int userId;
                var folderInfo = folderManager.GetFolder(effectivePortalId, folder);
                if (IsUserFolder(folder, out userId))
                {
                    var user = UserController.GetUserById(effectivePortalId, userId);
                    if (user != null)
                    {
                        folderInfo = folderManager.GetUserFolder(user);
                    }
                }

                if (!PortalSecurity.IsInRoles(userInfo, portalSettings, folderInfo.FolderPermissions.ToString("WRITE")) 
                    && !PortalSecurity.IsInRoles(userInfo, portalSettings, folderInfo.FolderPermissions.ToString("ADD")))
                {
                    errorMessage = GetLocalizedString("NoPermission");
                    return string.Empty;
                }

                if (!overwrite && FileManager.Instance.FileExists(folderInfo, fileName, true))
                {
                    errorMessage = GetLocalizedString("AlreadyExists");
                    alreadyExists = true;
                    return Path.Combine(folderInfo.PhysicalPath, fileName);
                }

                var file = FileManager.Instance.AddFile(folderInfo, fileName, stream, true, false, FileManager.Instance.GetContentType(Path.GetExtension(fileName)), userInfo.UserID);

                if (extract && extension.ToLower() == "zip")
                {
                    FileManager.Instance.UnzipFile(file);
                    FileManager.Instance.DeleteFile(file);
                }

                errorMessage = "";
                return Path.Combine(folderInfo.PhysicalPath, fileName);
            }
            catch (Exception exe)
            {
                Logger.Error(exe.Message);
                errorMessage = exe.Message;
                return string.Empty;
            }
        }

        private static string GetLocalizedString(string key)
        {
            const string resourceFile = "/App_GlobalResources/FileUpload.resx";
            return Localization.GetString(key, resourceFile);
        }

        private static bool IsUserFolder(string folderPath, out int userId)
        {
            var match = _userFolderEx.Match(folderPath);
            userId = match.Success ? int.Parse(match.Groups[1].Value) : Null.NullInteger;

            return match.Success;
        }

        private static string ShowImage(int fileId)
        {
            var image = (Services.FileSystem.FileInfo)FileManager.Instance.GetFile(fileId);

            if (image != null && IsAllowedExtension(image.Extension) && IsImageExtension(image.Extension))
            {
                var imageUrl = FileManager.Instance.GetUrl(image);
                return imageUrl;
            }

            return null;
        }

        private static readonly List<string> ImageExtensions = new List<string> { "JPG", "JPE", "BMP", "GIF", "PNG", "JPEG", "ICO" }; 

        private static bool IsImageExtension(string extension)
        {
            return ImageExtensions.Contains(extension.ToUpper());
        }

        private static bool IsImage(string fileName)
        {
            var name = fileName.ToUpper();
            return ImageExtensions.Any(extension => name.EndsWith("." + extension));
        }

        private static bool IsAllowedExtension(string extension)
        {
            return !string.IsNullOrEmpty(extension)
                   && Host.AllowedExtensionWhitelist.IsAllowedExtension(extension);
        }

        public class UploadByUrlDto
        {
            public string Url { get; set; }
            public string Folder { get; set; }
            public bool Overwrite { get; set; }
            public bool Unzip { get; set; }
            public string Filter { get; set; }
            public bool IsHostMenu { get; set; }
        }

        [DataContract]
        public class UploadDto
        {
            [DataMember(Name = "path")]
            public string Path { get; set; }

            [DataMember(Name = "orientation")]
            public Orientation Orientation { get; set; }

            [DataMember(Name = "alreadyExists")]
            public bool AlreadyExists { get; set; }

            [DataMember(Name = "message")]
            public string Message { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UploadFromUrl(UploadByUrlDto dto)
        {
            WebResponse response = null;
            Stream responseStream = null;
            var mediaTypeFormatter = new JsonMediaTypeFormatter();
            mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(dto.Url);
                request.Credentials = CredentialCache.DefaultCredentials;
                response = request.GetResponse();
                responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    throw new Exception("No server response");
                }
                var inMemoryStream = new MemoryStream();
                {
                    var count = 0;
                    do
                    {
                        var buffer = new byte[4096];
                        count = responseStream.Read(buffer, 0, 4096);
                        inMemoryStream.Write(buffer, 0, count);
                    } while (responseStream.CanRead && count > 0);

                    string errorMessage;
                    bool alreadyExists;

                    var segments = dto.Url.Split('/');
                    var fileName = segments[segments.Length - 1];
                    var returnFilename = SaveFile(inMemoryStream, PortalSettings, UserInfo, dto.Folder.TextOrEmpty(), dto.Filter.TextOrEmpty(),
                                                  fileName, dto.Overwrite, dto.IsHostMenu, dto.Unzip, out alreadyExists,
                                                  out errorMessage);

                    var root = AppDomain.CurrentDomain.BaseDirectory;
                    returnFilename = returnFilename.Replace(root, "~/");

                    var size = IsImage(returnFilename) ?
                        ImageHeader.GetDimensions(HostingEnvironment.MapPath(returnFilename)) :
                        Size.Empty;

                    /* Response Content Type cannot be application/json 
                     * because IE9 with iframe-transport manages the response 
                     * as a file download 
                     */
                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        new UploadDto { AlreadyExists = alreadyExists, Message = errorMessage, Orientation = size.Orientation(), Path = VirtualPathUtility.ToAbsolute(returnFilename) },
                        mediaTypeFormatter,
                        "text/plain");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    new UploadDto { AlreadyExists = false, Message = ex.Message },
                    mediaTypeFormatter,
                    "text/plain");
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }

    }

}