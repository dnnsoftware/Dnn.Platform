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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common.Utils;
using DotNetNuke.Entities.Icons;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using ContentDisposition = System.Net.Mime.ContentDisposition;
using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;
using System.Web;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]    
    public class FileUploadController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUploadController));
        private static readonly Regex UserFolderEx = new Regex(@"users/\d+/\d+/(\d+)/", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public class FolderItemDTO
        {
            public int FolderId { get; set; }
            public string FileFilter { get; set; }
            public bool Required { get; set; }
        }

        public class SavedFileDTO
        {
            public string FileId { get; set; }
            public string FilePath { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage LoadFiles(FolderItemDTO folderItem)
        {
            int effectivePortalId = PortalSettings.PortalId;
            

            if (folderItem.FolderId <= 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            var folder = FolderManager.Instance.GetFolder(folderItem.FolderId);

            if (folder == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            int userId;
            if (IsUserFolder(folder.FolderPath, out userId))
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

            var list = Globals.GetFileList(effectivePortalId, folderItem.FileFilter, !folderItem.Required, folder.FolderPath);
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
                        var returnFileDto = new SavedFileDTO();

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
                                        returnFileDto = SaveFile(stream, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, out alreadyExists, out errorMessage);
                                    },null
                                );
                            
                        }

                        /* Response Content Type cannot be application/json 
                         * because IE9 with iframe-transport manages the response 
                         * as a file download 
                         */
                        var mediaTypeFormatter = new JsonMediaTypeFormatter();
                        mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            return Request.CreateResponse(
                                HttpStatusCode.BadRequest,
                                new
                                {
                                    AlreadyExists = alreadyExists,
                                    Message = string.Format(GetLocalizedString("ErrorMessage"), fileName, errorMessage)
                                }, mediaTypeFormatter, "text/plain");
                        }
                        
                        return Request.CreateResponse(HttpStatusCode.OK, returnFileDto, mediaTypeFormatter, "text/plain");
                    });

            return task; 
        }

        private static SavedFileDTO SaveFile(
                Stream stream,
                PortalSettings portalSettings,
                UserInfo userInfo,
                string folder,
                string filter,
                string fileName,
                bool overwrite,
                bool isHostMenu,
                bool extract,
                out bool alreadyExists,
                out string errorMessage)
        {
            alreadyExists = false;
            var savedFileDto = new SavedFileDTO();
            try
            {
                var extension = Path.GetExtension(fileName).ValueOrEmpty().Replace(".", "");
                if (!string.IsNullOrEmpty(filter) && !filter.ToLower().Contains(extension.ToLower()))
                {
                    errorMessage = GetLocalizedString("ExtensionNotAllowed");
                    return savedFileDto;
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
                    return savedFileDto;
                }

                if (!overwrite && FileManager.Instance.FileExists(folderInfo, fileName, true))
                {
                    errorMessage = GetLocalizedString("AlreadyExists");
                    alreadyExists = true;
                    savedFileDto.FilePath = Path.Combine(folderInfo.PhysicalPath, fileName);
                    return savedFileDto;
                }

	            var contentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName));
				var file = FileManager.Instance.AddFile(folderInfo, fileName, stream, true, false, contentType, userInfo.UserID);

                if (extract && extension.ToLower() == "zip")
                {
                    FileManager.Instance.UnzipFile(file);
                    FileManager.Instance.DeleteFile(file);
                }

                errorMessage = "";
                savedFileDto.FileId = file.FileId.ToString(CultureInfo.InvariantCulture);
                savedFileDto.FilePath = FileManager.Instance.GetUrl(file);
                return savedFileDto;
            }
            catch (InvalidFileExtensionException)
            {
                errorMessage = GetLocalizedString("ExtensionNotAllowed");
                return savedFileDto;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                errorMessage = ex.Message;
                return savedFileDto;
            }
        }

        private static string GetLocalizedString(string key)
        {
            const string resourceFile = "/App_GlobalResources/FileUpload.resx";
            return Localization.GetString(key, resourceFile);
        }

        private static bool IsUserFolder(string folderPath, out int userId)
        {
            var match = UserFolderEx.Match(folderPath);
            userId = match.Success ? int.Parse(match.Groups[1].Value) : Null.NullInteger;

            return match.Success;
        }

        private static string ShowImage(int fileId)
        {
            var image = (FileInfo)FileManager.Instance.GetFile(fileId);

            if (image != null && IsImageExtension(image.Extension))
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
        public class FileUploadDto
        {
            [DataMember(Name = "path")]
            public string Path { get; set; }

            [DataMember(Name = "orientation")]
            public Orientation Orientation { get; set; }

            [DataMember(Name = "alreadyExists")]
            public bool AlreadyExists { get; set; }

            [DataMember(Name = "message")]
            public string Message { get; set; }

            [DataMember(Name = "fileIconUrl")]
            public string FileIconUrl { get; set; }

            [DataMember(Name = "fileId")]
            public int FileId { get; set; }

            [DataMember(Name = "fileName")]
            public string FileName { get; set; }

            [DataMember(Name = "prompt")]
            public string Prompt { get; set; }
        }

        public static string GetUrl(int fileId)
        {
            var file = FileManager.Instance.GetFile(fileId, true);
            return FileManager.Instance.GetUrl(file);
        }

        private static FileUploadDto UploadFile(
                Stream stream,
                int portalId,
                UserInfo userInfo,
                string folder,
                string filter,
                string fileName,
                bool overwrite,
                bool isHostPortal,
                bool extract)
        {
            var result = new FileUploadDto();
            BinaryReader reader = null;
            Stream fileContent = null;
            try
            {
                var extension = Path.GetExtension(fileName).ValueOrEmpty().Replace(".", "");
                result.FileIconUrl = IconController.GetFileIconUrl(extension);

                if (!string.IsNullOrEmpty(filter) && !filter.ToLower().Contains(extension.ToLower()))
                {
                    result.Message = GetLocalizedString("ExtensionNotAllowed");
                    return result;
                }

                var folderManager = FolderManager.Instance;

                var effectivePortalId = isHostPortal ? Null.NullInteger : portalId;

                // Check if this is a User Folder                
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

                if (!FolderPermissionController.HasFolderPermission(portalId, folder, "WRITE")
                    && !FolderPermissionController.HasFolderPermission(portalId, folder, "ADD"))
                {
                    result.Message = GetLocalizedString("NoPermission");
                    return result;
                }

                IFileInfo file;

                if (!overwrite && FileManager.Instance.FileExists(folderInfo, fileName, true))
                {
                    result.Message = GetLocalizedString("AlreadyExists");
                    result.AlreadyExists = true;
                    file = FileManager.Instance.GetFile(folderInfo, fileName, true);
                    result.FileId = file.FileId;
                }
                else
                {
                    file = FileManager.Instance.AddFile(folderInfo, fileName, stream, true, false,
                                                        FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)),
                                                        userInfo.UserID);
                    if (extract && extension.ToLower() == "zip")
                    {
                        var destinationFolder = FolderManager.Instance.GetFolder(file.FolderId);
                        var invalidFiles = new List<string>();
                        var filesCount = FileManager.Instance.UnzipFile(file, destinationFolder, invalidFiles);

                        var invalidFilesJson = invalidFiles.Count > 0
                            ? string.Format("\"{0}\"", string.Join("\",\"", invalidFiles))
                            : string.Empty;
                        result.Prompt = string.Format("{{\"invalidFiles\":[{0}], \"totalCount\": {1}}}", invalidFilesJson, filesCount);
                    }
                    result.FileId = file.FileId;
                }

                fileContent = FileManager.Instance.GetFileContent(file);

                var path = GetUrl(result.FileId);
                using (reader = new BinaryReader(fileContent))
                {
                    Size size;
                    if (IsImage(fileName))
                    {
                        try
                        {
                            size = ImageHeader.GetDimensions(reader);
                        }
                        catch (ArgumentException exc)
                        {
                            Logger.Warn("Unable to get image dimensions for image file", exc);
                            size = new Size(32, 32);
                        }
                    }
                    else
                    {
                        size = new Size(32, 32);
                    }

                    result.Orientation = size.Orientation();
                }

                result.Path = result.FileId > 0 ? path : string.Empty;
                result.FileName = fileName;

                if (extract && extension.ToLower() == "zip")
                {
                    FileManager.Instance.DeleteFile(file);
                }

                return result;
            }
            catch (InvalidFileExtensionException)
            { 
                result.Message = GetLocalizedString("ExtensionNotAllowed");
                return result;
            }
            catch (Exception exe)
            {
                Logger.Error(exe);
                result.Message = exe.Message;
                return result;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
                if (fileContent != null)
                {
                    fileContent.Close();
                    fileContent.Dispose();
                }
            }
        }

        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        [AllowAnonymous]
        public Task<HttpResponseMessage> UploadFromLocal()
        {
            var request = Request;
            FileUploadDto result = null;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            // local references for use in closure
            var portalId = PortalSettings.PortalId;
            var currentSynchronizationContext = SynchronizationContext.Current;
            var userInfo = UserInfo;
            var task = request.Content.ReadAsMultipartAsync(provider)
                .ContinueWith(o =>
                {
                    var folder = string.Empty;
                    var filter = string.Empty;
                    var fileName = string.Empty;
                    var overwrite = false;
                    var isHostPortal = false;
                    var extract = false;
                    Stream stream = null;

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

                            case "\"ISHOSTPORTAL\"":
                                bool.TryParse(item.ReadAsStringAsync().Result, out isHostPortal);
                                break;

                            case "\"EXTRACT\"":
                                bool.TryParse(item.ReadAsStringAsync().Result, out extract);
                                break;

                            case "\"PORTALID\"":
                                if (userInfo.IsSuperUser)
                                {
                                    int.TryParse(item.ReadAsStringAsync().Result, out portalId);
                                }
                                break;

                            case "\"POSTFILE\"":
                                fileName = item.Headers.ContentDisposition.FileName.Replace("\"", "");
                                if (fileName.IndexOf("\\", StringComparison.Ordinal) != -1)
                                {
                                    fileName = Path.GetFileName(fileName);
                                }
                                if (Globals.FileEscapingRegex.Match(fileName).Success==false )
                                    {
                                        stream = item.ReadAsStreamAsync().Result;
                                    }
                                break;
                        }
                    }

                    if (!string.IsNullOrEmpty(fileName) && stream != null)
                    {
                        // The SynchronizationContext keeps the main thread context. Send method is synchronous
                        currentSynchronizationContext.Send(
                            delegate
                            {
                                result = UploadFile(stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract);
                            },
                            null
                        );
                    }

                    var mediaTypeFormatter = new JsonMediaTypeFormatter();
                    mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                    /* Response Content Type cannot be application/json 
                     * because IE9 with iframe-transport manages the response 
                     * as a file download 
                     */
                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        result,
                        mediaTypeFormatter,
                        "text/plain");
                });

            return task;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public HttpResponseMessage UploadFromUrl(UploadByUrlDto dto)
        {
            FileUploadDto result;
            WebResponse response = null;
            Stream responseStream = null;
            var mediaTypeFormatter = new JsonMediaTypeFormatter();
            mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

            if (VerifySafeUrl(dto.Url) == false)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }


            try
            {
                var request = (HttpWebRequest)WebRequest.Create(dto.Url);
                request.Credentials = CredentialCache.DefaultCredentials;
                response = request.GetResponse();
                responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    throw new Exception("No server response");
                }

	            var fileName = GetFileName(response);
	            if (string.IsNullOrEmpty(fileName))
	            {
		            fileName = HttpUtility.UrlDecode(new Uri(dto.Url).Segments.Last());
	            }

                var portalId = PortalSettings.PortalId;

	            result = UploadFile(responseStream, portalId, UserInfo, dto.Folder.ValueOrEmpty(), dto.Filter.ValueOrEmpty(),
                    fileName, dto.Overwrite, dto.IsHostMenu, dto.Unzip);

                /* Response Content Type cannot be application/json 
                    * because IE9 with iframe-transport manages the response 
                    * as a file download 
                    */
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    result,
                    mediaTypeFormatter,
                    "text/plain");
            }
            catch (Exception ex)
            {
                result = new FileUploadDto
                {
                    Message = ex.Message
                };
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    result,
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

		private string GetFileName(WebResponse response)
		{
			if (!response.Headers.AllKeys.Contains("Content-Disposition"))
			{
				return string.Empty;
			}

			var contentDisposition = response.Headers["Content-Disposition"];
			return new ContentDisposition(contentDisposition).FileName;
		}

        private bool VerifySafeUrl(string url)
        {
            Uri uri = new Uri(url);
            if (uri.Scheme == "http" || uri.Scheme == "https")
            {

                if (!uri.Host.Contains("."))
                {
                    return false;
                }
                if (uri.IsLoopback)
                {
                    return false;
                }
                if (uri.PathAndQuery.Contains("#") || uri.PathAndQuery.Contains(":"))
                {
                    return false;
                }

                if (uri.Host.StartsWith("10") || uri.Host.StartsWith("172") || uri.Host.StartsWith("192"))
                {
                    //check nonroutable IP addresses
                    if (NetworkUtils.IsIPInRange(uri.Host, "10.0.0.0", "8") ||
                        NetworkUtils.IsIPInRange(uri.Host, "172.16.0.0", "12") ||
                        NetworkUtils.IsIPInRange(uri.Host, "192.168.0.0", "16"))
                    {
                        return false;
                    }
                }

                return true;
            }
            return false;
        }
    }

}