using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.GeneratedImage.FilterTransform;
using DotNetNuke.Services.GeneratedImage.StartTransform;
using DotNetNuke.Services.Localization.Internal;
using Assembly = System.Reflection.Assembly;

namespace DotNetNuke.Services.GeneratedImage
{
    public class DnnImageHandler : ImageHandler, System.Web.SessionState.IRequiresSessionState
    {
        /// <summary>
        /// While list of server folders where the system allow the dnn image handler to 
        /// read to serve image files from it and its subfolders
        /// </summary>
        private static readonly string[] WhiteListFolderPaths = {
            Globals.DesktopModulePath,
            Globals.ImagePath,
            Globals.ApplicationPath + "/Portals/"
        };

        private static bool IsAllowedFilePathImage(string filePath)
        {
            var normalizeFilePath = NormalizeFilePath(filePath.Trim());

            // Resources file cannot be served
            if (filePath.EndsWith(".resources"))
            {
                return false;
            }

            // File outside the white list cannot be served
            return WhiteListFolderPaths.Any(normalizeFilePath.StartsWith);
        }

        private static string NormalizeFilePath(string filePath)
        {
            var normalizeFilePath = filePath.Replace("\\", "/");
            if (!normalizeFilePath.StartsWith("/"))
            {
                normalizeFilePath = "/" + normalizeFilePath;
            }
            return normalizeFilePath;
        }

        private string _defaultImageFile = string.Empty;

        private Image EmptyImage
        {
            get
            {
                var emptyBmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed);
                emptyBmp.MakeTransparent();
                ContentType = ImageFormat.Png;

                if (string.IsNullOrEmpty(_defaultImageFile))
                {
                    return emptyBmp;
                }

                try
                {
                    var fullFilePath = HttpContext.Current.Server.MapPath(_defaultImageFile);

                    if (!File.Exists(fullFilePath) || !IsAllowedFilePathImage(_defaultImageFile))
                    {
                        return emptyBmp;
                    }

                    var fi = new System.IO.FileInfo(_defaultImageFile);
                    ContentType = GetImageFormat(fi.Extension);

                    using (var stream = new FileStream(fullFilePath, FileMode.Open))
                    {
                        return Image.FromStream(stream, true);
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Exceptions.LogException(ex);
                    return emptyBmp;
                }
            }
        }

        public DnnImageHandler()
        {
            // Set default settings here
            EnableClientCache = true;
            EnableServerCache = true;
            AllowStandalone = true;
            LogSecurity = false;
            EnableIPCount = false;
            ImageCompression = 95;
            DiskImageStore.PurgeInterval = new TimeSpan(0, 3, 0);
            IPCountPurgeInterval = new TimeSpan(0, 5, 0);
            IPCountMaxCount = 500;
            ClientCacheExpiration = new TimeSpan(0, 10, 0);
            AllowedDomains = new[] { string.Empty };

            // read settings from web.config
            ReadSettings();
        }

        // Add image generation logic here and return an instance of ImageInfo
        public override ImageInfo GenerateImage(NameValueCollection parameters)
        {
            SetupCulture();

            //which type of image should be generated ?
            string mode = string.IsNullOrEmpty(parameters["mode"]) ? "profilepic" : parameters["mode"].ToLower();

            // We need to determine the output format		
            string format = string.IsNullOrEmpty(parameters["format"]) ? "jpg" : parameters["format"].ToLower();

            // Lets retrieve the color
            Color color = string.IsNullOrEmpty(parameters["color"]) ? Color.White : (parameters["color"].StartsWith("#") ? ColorTranslator.FromHtml(parameters["color"]) : Color.FromName(parameters["color"]));
            Color backColor = string.IsNullOrEmpty(parameters["backcolor"]) ? Color.White : (parameters["backcolor"].StartsWith("#") ? ColorTranslator.FromHtml(parameters["backcolor"]) : Color.FromName(parameters["backcolor"]));

            // Do we have a border ?
            int border = string.IsNullOrEmpty(parameters["border"]) ? 0 : Convert.ToInt32(parameters["border"]);

            // Do we have a resizemode defined ?
            var resizeMode = string.IsNullOrEmpty(parameters["resizemode"]) ? ImageResizeMode.Fit : (ImageResizeMode)Enum.Parse(typeof(ImageResizeMode), parameters["ResizeMode"], true);

            // Maximum sizes 
            int maxWidth = string.IsNullOrEmpty(parameters["MaxWidth"]) ? 0 : Convert.ToInt32(parameters["MaxWidth"]);
            int maxHeight = string.IsNullOrEmpty(parameters["MaxHeight"]) ? 0 : Convert.ToInt32(parameters["MaxHeight"]);

            // Any text ?
            string text = string.IsNullOrEmpty(parameters["text"]) ? "" : parameters["text"];

            // Default Image
            _defaultImageFile = string.IsNullOrEmpty(parameters["NoImage"]) ? string.Empty : parameters["NoImage"];

            // Do we override caching for this image ?
            if (!string.IsNullOrEmpty(parameters["NoCache"]))
            {
                EnableClientCache = false;
                EnableServerCache = false;
            }

            try
            {
                ContentType = GetImageFormat(format);

                switch (mode)
                {
                    case "profilepic":
                        int uid;
                        if (!int.TryParse(parameters["userid"], out uid) || uid <= 0)
                        {
                            uid = -1;
                        }
                        var uppTrans = new UserProfilePicTransform
                        {
                            UserID = uid
                        };

                        IFileInfo photoFile;
                        ContentType = !uppTrans.TryGetPhotoFile(out photoFile)
                            ? ImageFormat.Gif
                            : GetImageFormat(photoFile?.Extension ?? "jpg");

                        ImageTransforms.Add(uppTrans);
                        break;

                    case "placeholder":
                        var placeHolderTrans = new PlaceholderTransform();
                        int width, height;
                        if (TryParseDimension(parameters["w"], out width))
                            placeHolderTrans.Width = width;
                        if (TryParseDimension(parameters["h"], out height))
                            placeHolderTrans.Height = height;
                        if (!string.IsNullOrEmpty(parameters["Color"]))
                            placeHolderTrans.Color = color;
                        if (!string.IsNullOrEmpty(parameters["Text"]))
                            placeHolderTrans.Text = text;
                        if (!string.IsNullOrEmpty(parameters["BackColor"]))
                            placeHolderTrans.BackColor = backColor;

                        ImageTransforms.Add(placeHolderTrans);
                        break;

                    case "securefile":
                        var secureFileTrans = new SecureFileTransform();
                        if (!string.IsNullOrEmpty(parameters["FileId"]))
                        {
                            var fileId = Convert.ToInt32(parameters["FileId"]);
                            var file = FileManager.Instance.GetFile(fileId);
                            if (file == null)
                            {
                                return GetEmptyImageInfo();
                            }
                            var folder = FolderManager.Instance.GetFolder(file.FolderId);
                            if (!secureFileTrans.DoesHaveReadFolderPermission(folder))
                            {
                                return GetEmptyImageInfo();
                            }
                            ContentType = GetImageFormat(file.Extension);
                            secureFileTrans.SecureFile = file;
                            secureFileTrans.EmptyImage = EmptyImage;
                            ImageTransforms.Add(secureFileTrans);
                        }
                        break;

                    case "file":
                        var imgFile = string.Empty;
                        var imgUrl = string.Empty;

                        // Lets determine the 2 types of Image Source: Single file, file url  
                        var filePath = parameters["File"];
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            filePath = filePath.Trim();
                            var fullFilePath = HttpContext.Current.Server.MapPath(filePath);
                            if (!File.Exists(fullFilePath) || !IsAllowedFilePathImage(filePath))
                            {
                                return GetEmptyImageInfo();
                            }
                            imgFile = fullFilePath;
                        }
                        else if (!string.IsNullOrEmpty(parameters["Url"]))
                        {
                            if (!parameters["Url"].StartsWith("http"))
                            {
                                return GetEmptyImageInfo();
                            }
                            imgUrl = parameters["Url"];
                        }

                        if (string.IsNullOrEmpty(parameters["format"]))
                        {
                            string extension;
                            if (string.IsNullOrEmpty(parameters["Url"]))
                            {
                                var fi = new System.IO.FileInfo(imgFile);
                                extension = fi.Extension.ToLower();
                            }
                            else
                            {
                                string[] parts = parameters["Url"].Split('.');
                                extension = parts[parts.Length - 1].ToLower();
                            }
                            ContentType = GetImageFormat(extension);
                        }
                        var imageFileTrans = new ImageFileTransform { ImageFilePath = imgFile, ImageUrl = imgUrl};
                        ImageTransforms.Add(imageFileTrans);
                        break;

                    default:

                        string imageTransformClass = ConfigurationManager.AppSettings["DnnImageHandler." + mode];
                        string[] imageTransformClassParts = imageTransformClass.Split(',');
                        var asm = Assembly.LoadFrom(Globals.ApplicationMapPath + @"\bin\" +
                                                         imageTransformClassParts[1].Trim() + ".dll");
                        var t = asm.GetType(imageTransformClassParts[0].Trim());
                        var imageTransform = (ImageTransform)Activator.CreateInstance(t);

                        foreach (var key in parameters.AllKeys)
                        {
                            var pi = t.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                            if (pi != null && key != "mode")
                            {
                                switch (key.ToLower())
                                {
                                    case "color":
                                        pi.SetValue(imageTransform, color, null);
                                        break;
                                    case "backcolor":
                                        pi.SetValue(imageTransform, backColor, null);
                                        break;
                                    case "border":
                                        pi.SetValue(imageTransform, border, null);
                                        break;
                                    default:
                                        switch (pi.PropertyType.Name)
                                        {
                                            case "Int32":
                                                pi.SetValue(imageTransform, Convert.ToInt32(parameters[key]), null);
                                                break;
                                            case "String":
                                                pi.SetValue(imageTransform, parameters[key], null);
                                                break;
                                        }
                                        break;
                                }
                            }
                        }
                        ImageTransforms.Add(imageTransform);
                        break;
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
                return GetEmptyImageInfo();
            }

            // Resize-Transformation
            if (mode != "placeholder")
            {
                int width, height;

                TryParseDimension(parameters["w"], out width);
                TryParseDimension(parameters["h"], out height);

                var size = string.IsNullOrEmpty(parameters["size"]) ? "" : parameters["size"];

                switch (size)
                {
                    case "xxs":
                        width = 16;
                        height = 16;
                        break;
                    case "xs":
                        width = 32;
                        height = 32;
                        break;
                    case "s":
                        width = 50;
                        height = 50;
                        break;
                    case "l":
                        width = 64;
                        height = 64;
                        break;
                    case "xl":
                        width = 128;
                        height = 128;
                        break;
                    case "xxl":
                        width = 256;
                        height = 256;
                        break;
                }

                if (mode == "profilepic")
                {
                    resizeMode = ImageResizeMode.FitSquare;
                    if (width>0 && height>0 && width != height)
                    {
                        resizeMode = ImageResizeMode.Fill;
                    }
                }

                if (width > 0 || height > 0)
                {
                    var resizeTrans = new ImageResizeTransform
                    {
                        Mode = resizeMode,
                        BackColor = backColor,
                        Width = width,
                        Height = height,
                        MaxWidth = maxWidth,
                        MaxHeight = maxHeight,
                        Border = border
                    };
                    ImageTransforms.Add(resizeTrans);
                }
            }

            // Gamma adjustment
            if (!string.IsNullOrEmpty(parameters["Gamma"]))
            {
                var gammaTrans = new ImageGammaTransform();
                double gamma;
                if (double.TryParse(parameters["Gamma"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out gamma) && gamma >= 0.2 && gamma <= 5)
                {
                    gammaTrans.Gamma = gamma;
                    ImageTransforms.Add(gammaTrans);
                }
            }

            // Brightness adjustment
            if (!string.IsNullOrEmpty(parameters["Brightness"]))
            {
                var brightnessTrans = new ImageBrightnessTransform();
                int brightness;
                if (int.TryParse(parameters["Brightness"], out brightness))
                {
                    brightnessTrans.Brightness = brightness;
                    ImageTransforms.Add(brightnessTrans);
                }
            }

            // Contrast adjustment
            if (!string.IsNullOrEmpty(parameters["Contrast"]))
            {
                var contrastTrans = new ImageContrastTransform();
                double contrast;
                if (double.TryParse(parameters["Contrast"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out contrast) && (contrast >= -100 && contrast <= 100))
                {
                    contrastTrans.Contrast = contrast;
                    ImageTransforms.Add(contrastTrans);
                }
            }

            // Greyscale
            if (!string.IsNullOrEmpty(parameters["Greyscale"]))
            {
                var greyscaleTrans = new ImageGreyScaleTransform();
                ImageTransforms.Add(greyscaleTrans);
            }

            // Invert
            if (!string.IsNullOrEmpty(parameters["Invert"]))
            {
                var invertTrans = new ImageInvertTransform();
                ImageTransforms.Add(invertTrans);
            }

            // Rotate / Flip 
            if (!string.IsNullOrEmpty(parameters["RotateFlip"]))
            {
                var rotateFlipTrans = new ImageRotateFlipTransform();
                var rotateFlipType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), parameters["RotateFlip"]);
                rotateFlipTrans.RotateFlip = rotateFlipType;
                ImageTransforms.Add(rotateFlipTrans);

            }

            // We start the chain with an empty image
            var dummy = new Bitmap(1, 1);
            using (var ms = new MemoryStream())
            {
                dummy.Save(ms, ContentType);
                return new ImageInfo(ms.ToArray());
            }
        }

        private ImageInfo GetEmptyImageInfo()
        {
            return new ImageInfo(EmptyImage)
            {
                IsEmptyImage = true
            };
        }

        private static bool TryParseDimension(string value, out int dimension)
        {
            dimension = 0;
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (!int.TryParse(value, out dimension))
            {
                return false;
            }

            // The system won't allow a resize for an image bigger than 4K pixels
            const int maxDimension = 4000;

            if (dimension > maxDimension)
            {
                dimension = 0;
                return false;
            }
            return true;
        }

        private void ReadSettings()
        {
            var settings = ConfigurationManager.AppSettings["DnnImageHandler"];
            if (string.IsNullOrEmpty(settings))
            {
                return;
            }

            string[] values = settings.Split(';');
            foreach (string value in values)
            {
                string[] setting = value.Split('=');
                string name = setting[0].ToLower();
                switch (name)
                {
                    case "enableclientcache":
                        EnableClientCache = Convert.ToBoolean(setting[1]);
                        break;
                    case "clientcacheexpiration":
                        ClientCacheExpiration = TimeSpan.FromSeconds(Convert.ToInt32(setting[1]));
                        break;
                    case "enableservercache":
                        EnableServerCache = Convert.ToBoolean(setting[1]);
                        break;
                    case "servercacheexpiration":
                        DiskImageStore.PurgeInterval = TimeSpan.FromSeconds(Convert.ToInt32(setting[1]));
                        break;
                    case "allowstandalone":
                        AllowStandalone = Convert.ToBoolean(setting[1]);
                        break;
                    case "logsecurity":
                        LogSecurity = Convert.ToBoolean(setting[1]);
                        break;
                    case "imagecompression":
                        ImageCompression = Convert.ToInt32(setting[1]);
                        break;
                    case "alloweddomains":
                        AllowedDomains = setting[1].Split(',');
                        break;
                    case "enableipcount":
                        EnableIPCount = Convert.ToBoolean(setting[1]);
                        break;
                    case "ipcountmax":
                        IPCountMaxCount = Convert.ToInt32(setting[1]);
                        break;
                    case "ipcountpurgeinterval":
                        IPCountPurgeInterval = TimeSpan.FromSeconds(Convert.ToInt32(setting[1]));
                        break;
                }
            }
        }

        private void SetupCulture()
        {
            var settings = PortalController.Instance.GetCurrentPortalSettings();
            if (settings == null) return;

            var pageLocale = TestableLocalization.Instance.GetPageLocale(settings);
            if (pageLocale != null)
            {
                TestableLocalization.Instance.SetThreadCultures(pageLocale, settings);
            }
        }

        private static ImageFormat GetImageFormat(string extension)
        {
            switch (extension.ToLower())
            {
                case "jpg":
                case "jpeg":
                    return ImageFormat.Jpeg;
                case "bmp":
                    return ImageFormat.Bmp;
                case "gif":
                    return ImageFormat.Gif;
                case "png":
                    return ImageFormat.Png;
                case "ico":
                    return ImageFormat.Icon;
                default:
                    return ImageFormat.Png;
            }
        }
    }
}