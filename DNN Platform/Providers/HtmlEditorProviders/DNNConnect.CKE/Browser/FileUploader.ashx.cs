// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Browser
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Script.Serialization;

    using DNNConnect.CKEditorProvider.Constants;
    using DNNConnect.CKEditorProvider.Objects;
    using DNNConnect.CKEditorProvider.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    /// <summary>The File Upload Handler.</summary>
    public class FileUploader : IHttpHandler
    {
        /// <summary>The JavaScript Serializer.</summary>
        private readonly JavaScriptSerializer js = new JavaScriptSerializer();

        private PortalSettings portalSettings = null;

        /// <summary>Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.</summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>Gets a value indicating whether [override files].</summary>
        /// <value>
        ///   <see langword="true"/> if [override files]; otherwise, <see langword="false"/>.
        /// </value>
        private bool OverrideFiles
        {
            get
            {
                return HttpContext.Current.Request["overrideFiles"].Equals("1")
                       || HttpContext.Current.Request["overrideFiles"].Equals(
                           "true",
                           StringComparison.InvariantCultureIgnoreCase);
            }
        }

        /// <summary>Gets the storage folder.</summary>
        /// <value>
        /// The storage folder.
        /// </value>
        private IFolderInfo StorageFolder
        {
            get
            {
                return FolderManager.Instance.GetFolder(Convert.ToInt32(HttpContext.Current.Request["storageFolderID"]));
            }
        }

        /*
        /// <summary>Gets the storage folder.</summary>
        /// <value>
        /// The storage folder.
        /// </value>
        private PortalSettings PortalSettings
        {
            get
            {
                return new PortalSettings(Convert.ToInt32(HttpContext.Current.Request["portalID"]));
            }
        }*/

        /// <summary>Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.</summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Pragma", "no-cache");
            context.Response.AddHeader("Cache-Control", "private, no-cache");

            this.HandleMethod(context);
        }

        /// <summary>Returns the options.</summary>
        /// <param name="context">The context.</param>
        private static void ReturnOptions(HttpContext context)
        {
            context.Response.AddHeader("Allow", "DELETE,GET,HEAD,POST,PUT,OPTIONS");
            context.Response.StatusCode = 200;
        }

        /// <summary>The get encoder.</summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <returns>
        /// The Encoder.
        /// </returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        /// <summary>Handle request based on method.</summary>
        /// <param name="context">The context.</param>
        private void HandleMethod(HttpContext context)
        {
            switch (context.Request.HttpMethod)
            {
                case "HEAD":
                case "GET":
                    /*if (GivenFilename(context))
                    {
                        this.DeliverFile(context);
                    }
                    else
                    {
                        ListCurrentFiles(context);
                    }*/

                    break;

                case "POST":
                case "PUT":
                    this.UploadFile(context);
                    break;

                case "OPTIONS":
                    ReturnOptions(context);
                    break;

                default:
                    context.Response.ClearHeaders();
                    context.Response.StatusCode = 405;
                    break;
            }
        }

        /// <summary>Uploads the file.</summary>
        /// <param name="context">The context.</param>
        private void UploadFile(HttpContext context)
        {
            var statuses = new List<FilesUploadStatus>();

            this.UploadWholeFile(context, statuses);

            this.WriteJsonIframeSafe(context, statuses);
        }

        private EditorProviderSettings GetCurrentSettings(HttpContext context)
        {
            var currentSettings = new EditorProviderSettings();
            var request = context.Request;
            int portalId;
            int.TryParse(request.QueryString["PortalID"], out portalId);
            this.portalSettings = new PortalSettings(portalId);

            SettingsMode settingMode;
            if (!Enum.TryParse(request.QueryString["mode"], true, out settingMode))
            {
                settingMode = SettingsMode.Default;
            }

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];
            var settingsDictionary = EditorController.GetEditorHostSettings();
            var portalRoles = RoleController.Instance.GetRoles(this.portalSettings.PortalId);

            switch (settingMode)
            {
                case SettingsMode.Default:
                    // Load Default Settings
                    currentSettings = SettingsUtil.GetDefaultSettings(
                        this.portalSettings,
                        this.portalSettings.HomeDirectoryMapPath,
                        objProvider.Attributes["ck_configFolder"],
                        portalRoles);
                    break;
                case SettingsMode.Host:
                    currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        this.portalSettings,
                        currentSettings,
                        settingsDictionary,
                        SettingConstants.HostKey,
                        portalRoles);
                    break;
                case SettingsMode.Portal:
                    currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        this.portalSettings,
                        currentSettings,
                        settingsDictionary,
                        SettingConstants.PortalKey(portalId),
                        portalRoles);
                    break;
                case SettingsMode.Page:
                    currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        this.portalSettings,
                        currentSettings,
                        settingsDictionary,
                        $"DNNCKT#{request.QueryString["tabid"]}#",
                        portalRoles);
                    break;
                case SettingsMode.ModuleInstance:
                    currentSettings = SettingsUtil.LoadModuleSettings(
                        this.portalSettings,
                        currentSettings,
                        $"DNNCKMI#{request.QueryString["mid"]}#INS#{request.QueryString["ckId"]}#",
                        int.Parse(request.QueryString["mid"]),
                        portalRoles);
                    break;
            }

            return currentSettings;
        }

        /// <summary>Uploads the whole file.</summary>
        /// <param name="context">The context.</param>
        /// <param name="statuses">The statuses.</param>
        private void UploadWholeFile(HttpContext context, List<FilesUploadStatus> statuses)
        {
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                var file = context.Request.Files[i];

                var fileName = Path.GetFileName(file.FileName);

                if (!string.IsNullOrEmpty(fileName))
                {
                    // Convert Unicode Chars
                    fileName = Utility.ConvertUnicodeChars(fileName);

                    // Replace dots in the name with underscores (only one dot can be there... security issue).
                    fileName = Regex.Replace(fileName, @"\.(?![^.]*$)", "_", RegexOptions.None);

                    // Check for Illegal Chars
                    if (Utility.ValidateFileName(fileName))
                    {
                        fileName = Utility.CleanFileName(fileName);
                    }
                }
                else
                {
                    throw new HttpRequestValidationException("File does not have a name");
                }

                if (fileName.Length > 220)
                {
                    fileName = fileName.Substring(fileName.Length - 220);
                }

                // file names starting with '\\' may be used for manipulating the filepath and explore vulnerabilities
                fileName = Regex.Replace(fileName, @"^\\+", string.Empty);

                var fileNameNoExtenstion = Path.GetFileNameWithoutExtension(fileName);

                // Rename File if Exists
                if (!this.OverrideFiles)
                {
                    var counter = 0;

                    while (File.Exists(Path.Combine(this.StorageFolder.PhysicalPath, fileName)))
                    {
                        counter++;
                        fileName = string.Format(
                            "{0}_{1}{2}",
                            fileNameNoExtenstion,
                            counter,
                            Path.GetExtension(file.FileName));
                    }
                }

                var contentType = FileContentTypeManager.Instance.GetContentType(Path.GetExtension(fileName));
                var userId = UserController.Instance.GetCurrentUserInfo().UserID;

                if (!contentType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                {
                    FileManager.Instance.AddFile(this.StorageFolder, fileName, file.InputStream, this.OverrideFiles, true, contentType, userId);
                }
                else
                {
                    // it's an image, so we might need to resize
                    var currentSettings = this.GetCurrentSettings(context);

                    int maxWidth = currentSettings.ResizeWidthUpload;
                    int maxHeight = currentSettings.ResizeHeightUpload;
                    if (maxWidth <= 0 && maxHeight <= 0)
                    {
                        FileManager.Instance.AddFile(this.StorageFolder, fileName, file.InputStream);
                    }
                    else
                    {
                        // check if the size of the image is within boundaries
                        using (var uplImage = Image.FromStream(file.InputStream))
                        {
                            if (uplImage.Width > maxWidth || uplImage.Height > maxHeight)
                            {
                                // it's too big: we need to resize
                                int newWidth, newHeight;

                                // which determines the max: height or width?
                                double ratioWidth = (double)maxWidth / (double)uplImage.Width;
                                double ratioHeight = (double)maxHeight / (double)uplImage.Height;
                                if (ratioWidth < ratioHeight)
                                {
                                    // max width needs to be used
                                    newWidth = maxWidth;
                                    newHeight = (int)Math.Round(uplImage.Height * ratioWidth);
                                }
                                else
                                {
                                    // max height needs to be used
                                    newHeight = maxHeight;
                                    newWidth = (int)Math.Round(uplImage.Width * ratioHeight);
                                }

                                // Add Compression to Jpeg Images
                                if (uplImage.RawFormat.Equals(ImageFormat.Jpeg))
                                {
                                    ImageCodecInfo jpgEncoder = GetEncoder(uplImage.RawFormat);

                                    Encoder myEncoder = Encoder.Quality;
                                    EncoderParameters encodeParams = new EncoderParameters(1);
                                    EncoderParameter encodeParam = new EncoderParameter(myEncoder, 80L);
                                    encodeParams.Param[0] = encodeParam;

                                    using (Bitmap dst = new Bitmap(newWidth, newHeight))
                                    {
                                        using (Graphics g = Graphics.FromImage(dst))
                                        {
                                            g.SmoothingMode = SmoothingMode.AntiAlias;
                                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                            g.DrawImage(uplImage, 0, 0, dst.Width, dst.Height);
                                        }

                                        using (var stream = new MemoryStream())
                                        {
                                            dst.Save(stream, jpgEncoder, encodeParams);
                                            FileManager.Instance.AddFile(this.StorageFolder, fileName, stream);
                                        }
                                    }
                                }
                                else
                                {
                                    // Finally Create a new Resized Image
                                    using (Image newImage = uplImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
                                    {
                                        var imageFormat = uplImage.RawFormat;
                                        using (var stream = new MemoryStream())
                                        {
                                            newImage.Save(stream, imageFormat);
                                            FileManager.Instance.AddFile(this.StorageFolder, fileName, stream);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // fits within configured maximum dimensions
                                FileManager.Instance.AddFile(this.StorageFolder, fileName, file.InputStream);
                            }
                        }
                    }
                }

                var fullName = Path.GetFileName(fileName);
                statuses.Add(new FilesUploadStatus(fullName, file.ContentLength));
            }
        }

        /// <summary>Writes the JSON iFrame safe.</summary>
        /// <param name="context">The context.</param>
        /// <param name="statuses">The statuses.</param>
        private void WriteJsonIframeSafe(HttpContext context, List<FilesUploadStatus> statuses)
        {
            context.Response.AddHeader("Vary", "Accept");
            try
            {
                context.Response.ContentType = context.Request["HTTP_ACCEPT"].Contains("application/json")
                                                   ? "application/json"
                                                   : "text/plain";
            }
            catch
            {
                context.Response.ContentType = "text/plain";
            }

            var jsonObj = this.js.Serialize(statuses.ToArray());
            context.Response.Write(jsonObj);
        }
    }
}
