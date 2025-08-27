// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider.Browser;

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

    private PortalSettings portalSettings;

    /// <summary>Gets a value indicating whether another request can use the <see cref="IHttpHandler" /> instance.</summary>
    public bool IsReusable => false;

    /// <summary>Gets a value indicating whether to override files.</summary>
    private static bool OverrideFiles =>
        HttpContext.Current.Request["overrideFiles"].Equals("1")
        || HttpContext.Current.Request["overrideFiles"].Equals("true", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>Gets the storage folder.</summary>
    /// <value>The storage folder.</value>
    private static IFolderInfo StorageFolder =>
        FolderManager.Instance.GetFolder(Convert.ToInt32(HttpContext.Current.Request["storageFolderID"]));

    /// <summary>Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="IHttpHandler" /> interface.</summary>
    /// <param name="context">An <see cref="HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
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

    /// <summary>Gets the codec for the given <paramref name="format"/>.</summary>
    /// <param name="format">The image format.</param>
    /// <returns>The codec, or <see langword="null"/>.</returns>
    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        var codecs = ImageCodecInfo.GetImageDecoders();

        return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
    }

    private static (int NewWidth, int NewHeight) DetermineNewDimensions(Image uplImage, int maxWidth, int maxHeight)
    {
        // it's too big: we need to resize
        int newWidth, newHeight;

        // which determines the max: height or width?
        var ratioWidth = (double)maxWidth / (double)uplImage.Width;
        var ratioHeight = (double)maxHeight / (double)uplImage.Height;
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

        return (newWidth, newHeight);
    }

    private static Stream ResizeImage(Image uplImage, int newWidth, int newHeight)
    {
        // Add Compression to Jpeg Images
        if (uplImage.RawFormat.Equals(ImageFormat.Jpeg))
        {
            return ResizeJpegImage(uplImage, newWidth, newHeight);
        }

        using Image newImage = uplImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
        var imageFormat = uplImage.RawFormat;

        var fileContents = new MemoryStream();
        newImage.Save(fileContents, imageFormat);
        return fileContents;
    }

    private static Stream ResizeJpegImage(Image uplImage, int newWidth, int newHeight)
    {
        var jpgEncoder = GetEncoder(uplImage.RawFormat);

        var myEncoder = Encoder.Quality;
        var encodeParams = new EncoderParameters(1);
        var encodeParam = new EncoderParameter(myEncoder, 80L);
        encodeParams.Param[0] = encodeParam;

        using var destination = new Bitmap(newWidth, newHeight);
        using (var graphic = Graphics.FromImage(destination))
        {
            graphic.SmoothingMode = SmoothingMode.AntiAlias;
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.DrawImage(uplImage, 0, 0, destination.Width, destination.Height);
        }

        var fileContents = new MemoryStream();
        destination.Save(fileContents, jpgEncoder, encodeParams);
        return fileContents;
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
        var request = context.Request;
        int.TryParse(request.QueryString["PortalID"], out var portalId);
        this.portalSettings = new PortalSettings(portalId);

        if (!Enum.TryParse<SettingsMode>(request.QueryString["mode"], true, out var settingMode))
        {
            settingMode = SettingsMode.Default;
        }

        var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
        var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];
        var settingsDictionary = EditorController.GetEditorHostSettings();
        var portalRoles = RoleController.Instance.GetRoles(this.portalSettings.PortalId);

        return settingMode switch
        {
            SettingsMode.Default =>
                SettingsUtil.GetDefaultSettings(
                    this.portalSettings,
                    this.portalSettings.HomeDirectoryMapPath,
                    objProvider.Attributes["ck_configFolder"],
                    portalRoles),
            SettingsMode.Host =>
                SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    settingsDictionary,
                    SettingConstants.HostKey,
                    portalRoles),
            SettingsMode.Portal =>
                SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    settingsDictionary,
                    SettingConstants.PortalKey(portalId),
                    portalRoles),
            SettingsMode.Page =>
                SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    settingsDictionary,
                    $"DNNCKT#{request.QueryString["tabid"]}#",
                    portalRoles),
            SettingsMode.ModuleInstance =>
                SettingsUtil.LoadModuleSettings(
                    this.portalSettings,
                    new EditorProviderSettings(),
                    $"DNNCKMI#{request.QueryString["mid"]}#INS#{request.QueryString["ckId"]}#",
                    int.Parse(request.QueryString["mid"]),
                    portalRoles),
            _ => new EditorProviderSettings(),
        };
    }

    /// <summary>Uploads the whole file.</summary>
    /// <param name="context">The context.</param>
    /// <param name="statuses">The statuses.</param>
    private void UploadWholeFile(HttpContext context, List<FilesUploadStatus> statuses)
    {
        for (var i = 0; i < context.Request.Files.Count; i++)
        {
            var file = context.Request.Files[i];
            if (file is null)
            {
                continue;
            }

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
            if (!OverrideFiles)
            {
                var counter = 0;

                while (File.Exists(Path.Combine(StorageFolder.PhysicalPath, fileName)))
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

            var disposeStream = false;
            Stream fileContents = null;
            try
            {
                if (!contentType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                {
                    fileContents = file.InputStream;
                }
                else
                {
                    // it's an image, so we might need to resize
                    var currentSettings = this.GetCurrentSettings(context);

                    int maxWidth = currentSettings.ResizeWidthUpload;
                    int maxHeight = currentSettings.ResizeHeightUpload;
                    if (maxWidth <= 0 && maxHeight <= 0)
                    {
                        fileContents = file.InputStream;
                    }
                    else
                    {
                        // check if the size of the image is within boundaries
                        using var uplImage = Image.FromStream(file.InputStream);
                        if (uplImage.Width <= maxWidth && uplImage.Height <= maxHeight)
                        {
                            // fits within configured maximum dimensions
                            fileContents = file.InputStream;
                        }
                        else
                        {
                            var (newWidth, newHeight) = DetermineNewDimensions(uplImage, maxWidth, maxHeight);

                            disposeStream = true;
                            fileContents = ResizeImage(uplImage, newWidth, newHeight);
                        }
                    }
                }

                FileManager.Instance.AddFile(
                    StorageFolder,
                    fileName,
                    fileContents,
                    OverrideFiles,
                    true,
                    contentType,
                    userId);
            }
            finally
            {
                if (disposeStream)
                {
                    fileContents?.Dispose();
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
            context.Response.ContentType =
                context.Request["HTTP_ACCEPT"].Contains("application/json")
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
