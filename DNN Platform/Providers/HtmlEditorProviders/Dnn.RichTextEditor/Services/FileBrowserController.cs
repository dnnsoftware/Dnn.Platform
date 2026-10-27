// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.RichTextEditorProvider.Components;
    using Dnn.RichTextEditorProvider.Components.FileBrowser;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// The server side of the editor's file browser and uploader. It speaks the Jodit connector protocol
    /// and checks every request against the current user's DNN folder permissions.
    /// </summary>
    [DnnAuthorize]
    public class FileBrowserController : DnnApiController
    {
        private const int SuccessCode = 220;
        private const string ResourceFile = ProviderSettings.ProviderPath + "App_LocalResources/FileBrowser.resx";

        private readonly IFileManager fileManager;
        private readonly FileBrowserService fileBrowser;

        /// <summary>Initializes a new instance of the <see cref="FileBrowserController"/> class.</summary>
        /// <param name="folderManager">The folder manager.</param>
        /// <param name="fileManager">The file manager.</param>
        /// <param name="folderMappingController">The folder mapping controller.</param>
        /// <param name="fileContentTypeManager">The file content type manager.</param>
        public FileBrowserController(IFolderManager folderManager, IFileManager fileManager, IFolderMappingController folderMappingController, IFileContentTypeManager fileContentTypeManager)
        {
            this.fileManager = fileManager;
            this.fileBrowser = new FileBrowserService(folderManager, fileManager, folderMappingController, new FolderPermissionChecker(), fileContentTypeManager.GetContentType);
        }

        /// <summary>Handles the browser's folder and file actions, sent as a form post with an <c>action</c> field.</summary>
        /// <returns>A <see cref="ConnectorResponse"/>.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> Connector()
        {
            var form = await this.Request.Content.ReadAsFormDataAsync();
            var folder = this.fileBrowser.GetBrowsableFolder(this.PortalSettings.PortalId, form["path"]);
            if (folder == null)
            {
                return this.Failure(FileBrowserError.FolderNotFound);
            }

            switch (form["action"])
            {
                case "permissions":
                    return this.Success(new ConnectorData { Permissions = this.fileBrowser.GetPermissions(folder), });

                case "folders":
                    var folderListing = this.CreateSource(folder);
                    folderListing.Folders = this.fileBrowser.GetSubfolderNames(folder);
                    return this.Success(new ConnectorData { Sources = new List<ConnectorSource> { folderListing, }, });

                case "files":
                    var fileListing = this.CreateSource(folder);
                    fileListing.Files = this.fileBrowser.GetFiles(folder, IsTrue(form["mods[onlyImages]"]))
                        .Select(ToConnectorFile)
                        .ToList();
                    return this.Success(new ConnectorData { Sources = new List<ConnectorSource> { fileListing, }, });

                case "folderCreate":
                    var created = this.fileBrowser.CreateFolder(folder, form["name"]);
                    return created.Succeeded ? this.Success(new ConnectorData()) : this.Failure(created.Error);

                case "fileRemove":
                    var removed = this.fileBrowser.RemoveFile(folder, form["name"]);
                    return removed.Succeeded ? this.Success(new ConnectorData()) : this.Failure(removed.Error);

                default:
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, CreateResponse(false, new ConnectorData { Messages = { "Unsupported action." }, }));
            }
        }

        /// <summary>
        /// Saves uploaded files. The target is the browser's current folder, or for uploads made straight
        /// from the editor, the provider's <c>uploadFolder</c> (the portal root by default).
        /// </summary>
        /// <returns>A <see cref="ConnectorResponse"/> listing the uploaded files' URLs.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> Upload()
        {
            if (!this.Request.Content.IsMimeMultipartContent())
            {
                return this.Request.CreateResponse(HttpStatusCode.UnsupportedMediaType);
            }

            var multipart = await this.Request.Content.ReadAsMultipartAsync(new MultipartMemoryStreamProvider());
            var fields = new NameValueCollection();
            var uploads = new List<HttpContent>();
            foreach (var part in multipart.Contents)
            {
                var disposition = part.Headers.ContentDisposition;
                var fieldName = disposition?.Name?.Trim('"');
                if (disposition?.FileName != null)
                {
                    uploads.Add(part);
                }
                else if (fieldName != null)
                {
                    fields[fieldName] = await part.ReadAsStringAsync();
                }
            }

            var path = fields["path"];
            if (string.IsNullOrEmpty(path))
            {
                path = ProviderSettings.GetAttributes()["uploadFolder"];
            }

            var folder = this.fileBrowser.GetBrowsableFolder(this.PortalSettings.PortalId, path);
            if (folder == null)
            {
                return this.Failure(FileBrowserError.FolderNotFound);
            }

            var data = new ConnectorData { BaseUrl = string.Empty, Files = new List<string>(), IsImages = new List<bool>(), };
            foreach (var upload in uploads)
            {
                var fileName = upload.Headers.ContentDisposition.FileName.Trim('"');
                using (var content = await upload.ReadAsStreamAsync())
                {
                    var result = this.fileBrowser.AddFile(folder, fileName, content, this.UserInfo.UserID);
                    if (result.Succeeded)
                    {
                        data.Files.Add(this.fileManager.GetUrl(result.Value));
                        data.IsImages.Add(FileBrowserService.IsImage(result.Value.FileName));
                    }
                    else
                    {
                        data.Messages.Add(string.Format(CultureInfo.CurrentCulture, "{0}: {1}", FileBrowserService.CleanFileName(fileName), GetMessage(result.Error)));
                    }
                }
            }

            var success = data.Files.Count > 0;
            data.Code = success ? SuccessCode : 0;
            return this.Request.CreateResponse(HttpStatusCode.OK, CreateResponse(success, data));
        }

        /// <summary>
        /// Redirects to a file in a folder that is not served from the portal's home directory (secure, database or
        /// cloud folders). The browser links such files through this action; access is checked here and again by the target URL.
        /// </summary>
        /// <param name="path">The file's folder path and name.</param>
        /// <returns>A redirect to the file, or 404.</returns>
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage File(string path)
        {
            var file = this.fileBrowser.GetViewableFile(this.PortalSettings.PortalId, path);
            if (file == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = this.Request.CreateResponse(HttpStatusCode.Found);
            response.Headers.Location = new Uri(this.Request.RequestUri, this.fileManager.GetUrl(file));
            return response;
        }

        private static bool IsTrue(string value)
        {
            return bool.TryParse(value, out var result) && result;
        }

        private static ConnectorFile ToConnectorFile(IFileInfo file)
        {
            return new ConnectorFile
            {
                File = file.FileName,
                Changed = file.LastModificationTime.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                Size = FormatSize(file.Size),
                IsImage = FileBrowserService.IsImage(file.FileName),
            };
        }

        private static string FormatSize(long bytes)
        {
            // The browser parses the KB/MB/GB suffix back to bytes when sorting by size.
            string[] units = { "KB", "MB", "GB", };
            if (bytes < 1024)
            {
                return bytes.ToString(CultureInfo.InvariantCulture);
            }

            double size = bytes;
            var unit = -1;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return size.ToString("0.#", CultureInfo.InvariantCulture) + " " + units[unit];
        }

        private static string GetMessage(FileBrowserError error)
        {
            var message = Localization.GetString(error.ToString(), ResourceFile);
            return string.IsNullOrEmpty(message) ? error.ToString() : message;
        }

        private static ConnectorResponse CreateResponse(bool success, ConnectorData data)
        {
            return new ConnectorResponse
            {
                Success = success,
                Time = DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture),
                Data = data,
            };
        }

        private ConnectorSource CreateSource(IFolderInfo folder)
        {
            // The browser builds a file's URL as baseurl/path/file. Standard folders are served from the home directory;
            // other folder types go through the File action, which redirects to the URL the folder provider gives.
            var baseUrl = this.fileBrowser.HasDirectUrls(folder)
                ? this.PortalSettings.HomeDirectory
                : $"{ServicesFramework.GetServiceFrameworkRoot()}API/{ServiceRouteMapper.ModuleFolderName}/FileBrowser/File?path=";

            return new ConnectorSource { BaseUrl = baseUrl, Path = folder.FolderPath, };
        }

        private HttpResponseMessage Success(ConnectorData data)
        {
            data.Code = SuccessCode;
            return this.Request.CreateResponse(HttpStatusCode.OK, CreateResponse(true, data));
        }

        private HttpResponseMessage Failure(FileBrowserError error)
        {
            // The browser reads failures from the body, so they are returned with 200.
            var data = new ConnectorData { Messages = { GetMessage(error) }, };
            return this.Request.CreateResponse(HttpStatusCode.OK, CreateResponse(false, data));
        }
    }
}
