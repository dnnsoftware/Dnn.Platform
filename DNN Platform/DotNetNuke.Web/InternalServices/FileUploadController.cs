// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

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
using System.Web;
using System.Web.Http;
using System.Web.UI.WebControls;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Abstractions.Security;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
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

using Microsoft.Extensions.DependencyInjection;

using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

/// <summary>A web API for uploading files.</summary>
[DnnAuthorize]
public class FileUploadController : DnnApiController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FileUploadController));
    private static readonly Regex UserFolderEx = new Regex(@"users/\d+/\d+/(\d+)/", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly List<string> ImageExtensions = Globals.ImageFileTypes.Split(',').ToList();

    private readonly IHostSettings hostSettings;
    private readonly ICryptographyProvider cryptographyProvider;
    private readonly IPortalController portalController;
    private readonly IApplicationStatusInfo appStatus;
    private readonly IPortalGroupController portalGroupController;

    /// <summary>Initializes a new instance of the <see cref="FileUploadController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
    public FileUploadController()
        : this(null, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FileUploadController"/> class.</summary>
    /// <param name="hostSettings">The host settings.</param>
    [Obsolete("Deprecated in DotNetNuke 10.2.2. Use overload with ICryptographyProvider. Scheduled for removal in v12.0.0.")]
    public FileUploadController(IHostSettings hostSettings)
        : this(hostSettings, null, null, null, null)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="FileUploadController"/> class.</summary>
    /// <param name="hostSettings">The host settings.</param>
    /// <param name="cryptographyProvider">The cryptography provider.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="appStatus">The application status.</param>
    /// <param name="portalGroupController">The portal group controller.</param>
    public FileUploadController(IHostSettings hostSettings, ICryptographyProvider cryptographyProvider, IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
    {
        this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        this.cryptographyProvider = cryptographyProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<ICryptographyProvider>();
        this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
        this.portalGroupController = portalGroupController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>();
    }

    /// <summary>Gets the URL for a file.</summary>
    /// <param name="fileId">The ID of the file.</param>
    /// <returns>The URL.</returns>
    public static string GetUrl(int fileId)
    {
        var file = FileManager.Instance.GetFile(fileId, true);
        return FileManager.Instance.GetUrl(file);
    }

    /// <summary>Gets the files in the folder.</summary>
    /// <param name="folderItem">Information about the folder.</param>
    /// <returns>A response with a list of <see cref="FileItem"/> objects.</returns>
    [HttpPost]
    public HttpResponseMessage LoadFiles(FolderItemDTO folderItem)
    {
        int effectivePortalId = this.PortalSettings.PortalId;

        if (folderItem.FolderId <= 0)
        {
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        var folder = FolderManager.Instance.GetFolder(folderItem.FolderId);

        if (folder == null)
        {
            return this.Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        if (IsUserFolder(folder.FolderPath, out var userId))
        {
            var user = UserController.GetUserById(this.hostSettings, effectivePortalId, userId);
            if (user is { IsSuperUser: true, })
            {
                effectivePortalId = Null.NullInteger;
            }
            else
            {
                effectivePortalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, effectivePortalId);
            }
        }

        var list = Globals.GetFileList(effectivePortalId, folderItem.FileFilter, !folderItem.Required, folder.FolderPath);
        var fileItems = list.OfType<FileItem>().ToList();

        return this.Request.CreateResponse(HttpStatusCode.OK, fileItems);
    }

    /// <summary>Gets the URL of an image file.</summary>
    /// <param name="fileId">The file ID.</param>
    /// <returns>A response with either <c>null</c> or the image URL.</returns>
    [HttpGet]
    public HttpResponseMessage LoadImage(string fileId)
    {
        if (!string.IsNullOrEmpty(fileId))
        {
            int file;
            if (int.TryParse(fileId, out file))
            {
                var imageUrl = ShowImage(file);
                return this.Request.CreateResponse(HttpStatusCode.OK, imageUrl);
            }
        }

        return this.Request.CreateResponse(HttpStatusCode.InternalServerError);
    }

    /// <summary>Uploads an image.</summary>
    /// <returns>A response with an <see cref="SavedFileDTO"/> object.</returns>
    /// <exception cref="HttpResponseException">If the request is not using a multipart MIME content type.</exception>
    [HttpPost]
    [IFrameSupportedValidateAntiForgeryToken]
    public Task<HttpResponseMessage> PostFile()
    {
        HttpRequestMessage request = this.Request;

        if (!request.Content.IsMimeMultipartContent())
        {
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        var provider = new MultipartMemoryStreamProvider();

        // local references for use in closure
        IPortalSettings portalSettings = this.PortalSettings;
        var currentSynchronizationContext = SynchronizationContext.Current;
        var userInfo = this.UserInfo;
        var task = request.Content.ReadAsMultipartAsync(provider)
            .ContinueWith(_ =>
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
                    switch (name.ToUpperInvariant())
                    {
                        case "\"FOLDER\"":
                            folder = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"FILTER\"":
                            filter = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"OVERWRITE\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out overwrite))
                            {
                                overwrite = false;
                            }

                            break;

                        case "\"ISHOSTMENU\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out isHostMenu))
                            {
                                isHostMenu = false;
                            }

                            break;

                        case "\"EXTRACT\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out extract))
                            {
                                extract = false;
                            }

                            break;

                        case "\"POSTFILE\"":
                            fileName = item.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                            if (fileName.IndexOf(@"\", StringComparison.Ordinal) != -1)
                            {
                                fileName = Path.GetFileName(fileName);
                            }

                            stream = item.ReadAsStreamAsync().Result;
                            break;
                    }
                }

                var errorMessage = string.Empty;
                var alreadyExists = false;
                if (!string.IsNullOrEmpty(fileName) && stream != null)
                {
                    // Everything ready

                    // The SynchronizationContext keeps the main thread context. Send method is synchronous
                    currentSynchronizationContext.Send(
                        _ =>
                        {
                            returnFileDto = SaveFile(stream, this.portalController, this.appStatus, this.portalGroupController, this.hostSettings, portalSettings, userInfo, folder, filter, fileName, overwrite, isHostMenu, extract, out alreadyExists, out errorMessage);
                        },
                        null);
                }

                /* Response Content Type cannot be application/json
                 * because IE9 with iframe-transport manages the response
                 * as a file download
                 */
                var mediaTypeFormatter = new JsonMediaTypeFormatter();
                mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    return this.Request.CreateResponse(
                        HttpStatusCode.BadRequest,
                        new
                        {
                            AlreadyExists = alreadyExists,
                            Message = string.Format(
                                CultureInfo.CurrentCulture,
                                GetLocalizedString("ErrorMessage"),
                                fileName,
                                errorMessage),
                        },
                        mediaTypeFormatter,
                        "text/plain");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, returnFileDto, mediaTypeFormatter, "text/plain");
            });

        return task;
    }

    /// <summary>Uploads a file to the current portal.</summary>
    /// <returns>A response with a <see cref="FileUploadDto"/> object.</returns>
    [HttpPost]
    [IFrameSupportedValidateAntiForgeryToken]
    [AllowAnonymous]
    public Task<HttpResponseMessage> UploadFromLocal()
    {
        return this.UploadFromLocal(this.PortalSettings.PortalId);
    }

    /// <summary>Uploads a file.</summary>
    /// <param name="portalId">The ID of the portal to which to upload it.</param>
    /// <returns>A response with a <see cref="FileUploadDto"/> object.</returns>
    /// <exception cref="HttpResponseException">If the request is not using a multipart MIME content type.</exception>
    [HttpPost]
    [IFrameSupportedValidateAntiForgeryToken]
    [AllowAnonymous]
    public Task<HttpResponseMessage> UploadFromLocal(int portalId)
    {
        var request = this.Request;
        FileUploadDto result = null;
        if (!request.Content.IsMimeMultipartContent())
        {
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        if (portalId > -1)
        {
            if (!this.IsPortalIdValid(portalId))
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }
        }
        else
        {
            portalId = this.PortalSettings.PortalId;
        }

        var provider = new MultipartMemoryStreamProvider();

        // local references for use in closure
        var currentSynchronizationContext = SynchronizationContext.Current;
        var userInfo = this.UserInfo;
        var task = request.Content.ReadAsMultipartAsync(provider)
            .ContinueWith(_ =>
            {
                var folder = string.Empty;
                var filter = string.Empty;
                var fileName = string.Empty;
                var validationCode = string.Empty;
                var overwrite = false;
                var isHostPortal = false;
                var extract = false;
                Stream stream = null;

                foreach (var item in provider.Contents)
                {
                    var name = item.Headers.ContentDisposition.Name;
                    switch (name.ToUpperInvariant())
                    {
                        case "\"FOLDER\"":
                            folder = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"FILTER\"":
                            filter = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;

                        case "\"OVERWRITE\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out overwrite))
                            {
                                overwrite = false;
                            }

                            break;

                        case "\"ISHOSTPORTAL\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out isHostPortal))
                            {
                                isHostPortal = false;
                            }

                            break;

                        case "\"EXTRACT\"":
                            if (!bool.TryParse(item.ReadAsStringAsync().Result, out extract))
                            {
                                extract = false;
                            }

                            break;

                        case "\"PORTALID\"":
                            if (userInfo.IsSuperUser)
                            {
                                var originalPortalId = portalId;
                                if (!int.TryParse(item.ReadAsStringAsync().Result, out portalId))
                                {
                                    portalId = originalPortalId;
                                }
                            }

                            break;
                        case "\"VALIDATIONCODE\"":
                            validationCode = item.ReadAsStringAsync().Result ?? string.Empty;
                            break;
                        case "\"POSTFILE\"":
                            fileName = item.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                            if (fileName.IndexOf(@"\", StringComparison.Ordinal) != -1)
                            {
                                fileName = Path.GetFileName(fileName);
                            }

                            if (!Globals.FileEscapingRegex.IsMatch(fileName))
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
                        _ =>
                        {
                            result = UploadFile(this.cryptographyProvider, this.hostSettings, stream, portalId, userInfo, folder, filter, fileName, overwrite, isHostPortal, extract, validationCode);
                        },
                        null);
                }

                var mediaTypeFormatter = new JsonMediaTypeFormatter();
                mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

                /* Response Content Type cannot be application/json
                 * because IE9 with iframe-transport manages the response
                 * as a file download
                 */
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    result,
                    mediaTypeFormatter,
                    "text/plain");
            });

        return task;
    }

    private static SavedFileDTO SaveFile(
        Stream stream,
        IPortalController portalController,
        IApplicationStatusInfo appStatus,
        IPortalGroupController portalGroupController,
        IHostSettings hostSettings,
        IPortalSettings portalSettings,
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
            var extension = Path.GetExtension(fileName).ValueOrEmpty().Replace(".", string.Empty);
            if (!string.IsNullOrEmpty(filter) && !filter.ToLowerInvariant().Contains(extension.ToLowerInvariant()))
            {
                errorMessage = GetLocalizedString("ExtensionNotAllowed");
                return savedFileDto;
            }

            var folderManager = FolderManager.Instance;

            // Check if this is a User Folder
            var effectivePortalId = isHostMenu ? Null.NullInteger : PortalController.GetEffectivePortalId(portalController, appStatus, portalGroupController, portalSettings.PortalId);
            var folderInfo = folderManager.GetFolder(effectivePortalId, folder);
            if (IsUserFolder(folder, out var userId))
            {
                var user = UserController.GetUserById(hostSettings, effectivePortalId, userId);
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

            const bool AlreadyCheckedPermissions = true;
            if (!overwrite && FileManager.Instance.FileExists(folderInfo, fileName, true))
            {
                errorMessage = GetLocalizedString("AlreadyExists");
                alreadyExists = true;
                savedFileDto.FilePath = Path.Combine(folderInfo.PhysicalPath, fileName);
                return savedFileDto;
            }

            var contentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName));
            var file = FileManager.Instance.AddFile(folderInfo, fileName, stream, true, !AlreadyCheckedPermissions, contentType, userInfo.UserID);

            if (extract && extension.Equals("zip", StringComparison.OrdinalIgnoreCase))
            {
                FileManager.Instance.UnzipFile(file);
                FileManager.Instance.DeleteFile(file);
            }

            errorMessage = string.Empty;
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
        userId = match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : Null.NullInteger;

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

    private static bool IsImageExtension(string extension)
    {
        return ImageExtensions.Any(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsImage(string fileName)
    {
        return ImageExtensions.Any(extension => fileName.EndsWith("." + extension, StringComparison.OrdinalIgnoreCase));
    }

    private static FileUploadDto UploadFile(
        ICryptographyProvider cryptographyProvider,
        IHostSettings hostSettings,
        Stream stream,
        int portalId,
        UserInfo userInfo,
        string folder,
        string filter,
        string fileName,
        bool overwrite,
        bool isHostPortal,
        bool extract,
        string validationCode)
    {
        var result = new FileUploadDto();
        BinaryReader reader = null;
        Stream fileContent = null;
        try
        {
            var extensionList = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                extensionList = filter.Split(',').Select(i => i.Trim()).ToList();
            }

            var validateParams = new List<object> { extensionList, userInfo.UserID };
            if (!userInfo.IsSuperUser)
            {
                validateParams.Add(portalId);
            }

            if (!ValidationUtils.ValidationCodeMatched(cryptographyProvider, hostSettings, validateParams, validationCode))
            {
                throw new InvalidOperationException("Bad Request");
            }

            var extension = Path.GetExtension(fileName).ValueOrEmpty().Replace(".", string.Empty);
            result.FileIconUrl = IconController.GetFileIconUrl(extension);

            if (!string.IsNullOrEmpty(filter) && !filter.ToLowerInvariant().Contains(extension.ToLowerInvariant()))
            {
                result.Message = GetLocalizedString("ExtensionNotAllowed");
                return result;
            }

            var folderManager = FolderManager.Instance;
            var effectivePortalId = isHostPortal ? Null.NullInteger : portalId;
            var folderInfo = folderManager.GetFolder(effectivePortalId, folder);

            if (folderInfo == null && IsUserFolder(folder, out var userId))
            {
                var user = UserController.GetUserById(hostSettings, effectivePortalId, userId);
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

            const bool AlreadyCheckedPermissions = true;
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
                file = FileManager.Instance.AddFile(folderInfo, fileName, stream, true, !AlreadyCheckedPermissions, FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName)), userInfo.UserID);
                if (extract && extension.Equals("zip", StringComparison.OrdinalIgnoreCase))
                {
                    var destinationFolder = FolderManager.Instance.GetFolder(file.FolderId);
                    var invalidFiles = new List<string>();
                    var filesCount = FileManager.Instance.UnzipFile(file, destinationFolder, invalidFiles);

                    var invalidFilesJson = invalidFiles.Count > 0
                        ? string.Join(",", invalidFiles.Select(invalidFile => HttpUtility.JavaScriptStringEncode(invalidFile, addDoubleQuotes: true)))
                        : string.Empty;
                    result.Prompt = $"{{\"invalidFiles\":[{invalidFilesJson}], \"totalCount\": {filesCount}}}";
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

            if (extract && extension.Equals("zip", StringComparison.OrdinalIgnoreCase))
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

    private static IPortalInfo[] GetMyPortalGroup()
    {
        return (
                from @group in PortalGroupController.Instance.GetPortalGroups().ToArray()
                select PortalGroupController.Instance.GetPortalsByGroup(@group.PortalGroupId) into portals
                where portals.Any((IPortalInfo x) => x.PortalId == PortalSettings.Current.PortalId)
                select portals.Cast<IPortalInfo>().ToArray())
            .FirstOrDefault();
    }

    private bool IsPortalIdValid(int portalId)
    {
        if (this.UserInfo.IsSuperUser)
        {
            return true;
        }

        if (this.PortalSettings.PortalId == portalId)
        {
            return true;
        }

        var isAdminUser = PortalSecurity.IsInRole(this.PortalSettings.AdministratorRoleName);
        if (!isAdminUser)
        {
            return false;
        }

        var myGroup = GetMyPortalGroup();
        return myGroup != null && myGroup.Any(p => p.PortalId == portalId);
    }

    /// <summary>A data transfer object with information about a folder.</summary>
    public class FolderItemDTO
    {
        /// <summary>Gets or sets the folder ID.</summary>
        public int FolderId { get; set; }

        /// <summary>Gets or sets the file filter.</summary>
        public string FileFilter { get; set; }

        /// <summary>Gets or sets a value indicating whether to include an entry for an unspecified file.</summary>
        public bool Required { get; set; }
    }

    /// <summary>A data transfer object with information about a file that has been saved.</summary>
    public class SavedFileDTO
    {
        /// <summary>Gets or sets the ID of the file.</summary>
        public string FileId { get; set; }

        /// <summary>Gets or sets the path of the file.</summary>
        public string FilePath { get; set; }
    }

    /// <summary>A data transfer object with information about an upload by URL request.</summary>
    public class UploadByUrlDto
    {
        /// <summary>Gets or sets the URL.</summary>
        public string Url { get; set; }

        /// <summary>Gets or sets the destination folder.</summary>
        public string Folder { get; set; }

        /// <summary>Gets or sets a value indicating whether to overwrite an existing file.</summary>
        public bool Overwrite { get; set; }

        /// <summary>Gets or sets a value indicating whether to unzip the resulting file.</summary>
        public bool Unzip { get; set; }

        /// <summary>Gets or sets the filter.</summary>
        public string Filter { get; set; }

        /// <summary>Gets or sets a value indicating whether the request is from the host menu.</summary>
        public bool IsHostMenu { get; set; }

        /// <summary>Gets or sets the portal ID.</summary>
        public int PortalId { get; set; } = -1;

        /// <summary>Gets or sets the validation code.</summary>
        public string ValidationCode { get; set; }
    }

    /// <summary>A data transfer object with information about a file upload.</summary>
    [DataContract]
    public class FileUploadDto
    {
        /// <summary>Gets or sets the file path.</summary>
        [DataMember(Name = "path")]
        public string Path { get; set; }

        /// <summary>Gets or sets the image orientation.</summary>
        [DataMember(Name = "orientation")]
        public Orientation Orientation { get; set; }

        /// <summary>Gets or sets a value indicating whether the file already exists.</summary>
        [DataMember(Name = "alreadyExists")]
        public bool AlreadyExists { get; set; }

        /// <summary>Gets or sets an error message.</summary>
        [DataMember(Name = "message")]
        public string Message { get; set; }

        /// <summary>Gets or sets the URL of the file type icon.</summary>
        [DataMember(Name = "fileIconUrl")]
        public string FileIconUrl { get; set; }

        /// <summary>Gets or sets the ID of the file.</summary>
        [DataMember(Name = "fileId")]
        public int FileId { get; set; }

        /// <summary>Gets or sets the name of the file.</summary>
        [DataMember(Name = "fileName")]
        public string FileName { get; set; }

        /// <summary>Gets or sets a JSON string with <c>invalidFiles</c> and <c>totalCount</c> fields.</summary>
        [DataMember(Name = "prompt")]
        public string Prompt { get; set; }
    }
}
