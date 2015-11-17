using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
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
	public class DnnImageHandler : ImageHandler
	{
	    private string _defaultImageFile = "";

	    private Image EmptyImage
		{
			get
			{
                Bitmap emptyBmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed);
                emptyBmp.MakeTransparent();
                ContentType = ImageFormat.Png;

			    if (!String.IsNullOrEmpty(_defaultImageFile))
			    {
                    System.IO.FileInfo fi = new System.IO.FileInfo(_defaultImageFile);
			        string format = fi.Extension;
			        switch (format)
			        {
			            case "jpg":
			            case "jpeg":
			                ContentType = ImageFormat.Jpeg;
			                break;
			            case "bmp":
			                ContentType = ImageFormat.Bmp;
			                break;
			            case "gif":
			                ContentType = ImageFormat.Gif;
			                break;
			            case "png":
			                ContentType = ImageFormat.Png;
			                break;
			        }

			        if (File.Exists(_defaultImageFile))
			        {
			            emptyBmp = new Bitmap(Image.FromFile(_defaultImageFile, true));
			        }
			        else
			        {
			            _defaultImageFile = Path.GetFullPath(HttpContext.Current.Request.PhysicalApplicationPath + _defaultImageFile);
			            if (File.Exists(_defaultImageFile))
			            {
			                emptyBmp = new Bitmap(Image.FromFile(_defaultImageFile, true));
			            }
			        }
			    }
				return emptyBmp;
			}
		}

		public DnnImageHandler()
		{
            // Set default settings here
			EnableClientCache = true;
			EnableServerCache = true;
			AllowStandalone = false;
			LogSecurity = false;
		    EnableIPCount = true;
			ImageCompression = 95;
            DiskImageStore.PurgeInterval = new TimeSpan(0,3,0);
            IPCountPurgeInterval = new TimeSpan(0,5,0);
		    IPCountMaxCount = 2;
            ClientCacheExpiration = new TimeSpan(0,10,0);
		    AllowedDomains = new string[]{""};

            // read settings from web.config
            ReadSettings();
		}

        // Add image generation logic here and return an instance of ImageInfo
		public override ImageInfo GenerateImage(NameValueCollection parameters)
		{
            SetupCulture();

            //which type of image should be generated ?
            string mode = String.IsNullOrEmpty(parameters["mode"]) ? "profilepic" : parameters["mode"].ToLower();

            // We need to determine the output format		
            string format = String.IsNullOrEmpty(parameters["format"]) ? "jpg" : parameters["format"].ToLower();

            // Lets retrieve the color
            Color color = String.IsNullOrEmpty(parameters["color"]) ? Color.White : (parameters["color"].StartsWith("#") ? ColorTranslator.FromHtml(parameters["color"]) : Color.FromName(parameters["color"]));
            Color backColor = String.IsNullOrEmpty(parameters["backcolor"]) ? Color.White : (parameters["backcolor"].StartsWith("#") ? ColorTranslator.FromHtml(parameters["backcolor"]) : Color.FromName(parameters["backcolor"]));

            // Do we have a border ?
		    int border = String.IsNullOrEmpty(parameters["border"]) ? 0 : Convert.ToInt32(parameters["border"]);

            // Do we have a resizemode defined ?
		    ImageResizeMode resizeMode = String.IsNullOrEmpty(parameters["resizemode"]) ? ImageResizeMode.Fit : (ImageResizeMode) Enum.Parse(typeof (ImageResizeMode), parameters["ResizeMode"], true);

            // Maximum sizes 
            int maxWidth = String.IsNullOrEmpty(parameters["MaxWidth"]) ? 0 : Convert.ToInt32(parameters["MaxWidth"]);
            int maxHeight = String.IsNullOrEmpty(parameters["MaxHeight"]) ? 0 : Convert.ToInt32(parameters["MaxHeight"]);

            // Any text ?
            string text = String.IsNullOrEmpty(parameters["text"]) ? "" : parameters["text"];

            // Default Image
            _defaultImageFile = String.IsNullOrEmpty(parameters["NoImage"]) ? "" : parameters["NoImage"];

            // Do we override caching for this image ?
            if (!String.IsNullOrEmpty(parameters["NoCache"]))
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
		                UserProfilePicTransform uppTrans = new UserProfilePicTransform();
		                uppTrans.UserID = String.IsNullOrEmpty(parameters["userid"])
		                    ? -1
		                    : Convert.ToInt32(parameters["userid"]);
		                ImageTransforms.Add(uppTrans);
		                break;

		            case "modinfo":
		                ModInfoTransform modInfoTrans = new ModInfoTransform();
		                modInfoTrans.TabID = Convert.ToInt32(parameters["tabid"]);
		                modInfoTrans.ModuleID = Convert.ToInt32(parameters["moduleid"]);
		                ImageTransforms.Add(modInfoTrans);
		                break;

		            case "placeholder":
		                PlaceholderTransform placeHolderTrans = new PlaceholderTransform();
		                int width, height;
		                if (Int32.TryParse(parameters["w"], out width))
		                    placeHolderTrans.Width = width;
		                if (Int32.TryParse(parameters["h"], out height))
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
		                SecureFileTransform secureFileTrans = new SecureFileTransform();
		                if (!string.IsNullOrEmpty(parameters["FileId"]))
		                {
		                    int fileId = Convert.ToInt32(parameters["FileId"]);
		                    IFileInfo file = FileManager.Instance.GetFile(fileId);

		                    if (file != null)
		                    {
		                        ContentType = GetImageFormat(file.Extension);
		                        secureFileTrans.SecureFile = file;
		                        secureFileTrans.EmptyImage = EmptyImage;
		                        ImageTransforms.Add(secureFileTrans);
		                    }
		                }
		                break;

		            case "file":
		                string imgFile = "";

		                // Lets determine the 3 types of Image Source: Single file, file_in_directory[index], file url  
		                if (!String.IsNullOrEmpty(parameters["File"]))
		                {
		                    imgFile = parameters["File"].Trim();

		                    if (!File.Exists(imgFile))
		                    {
		                        imgFile = Path.GetFullPath(HttpContext.Current.Request.PhysicalApplicationPath + imgFile);
		                        if (!File.Exists(imgFile))
		                            return new ImageInfo(EmptyImage);
		                    }
		                }
		                else if (!String.IsNullOrEmpty(parameters["Path"]))
		                {
		                    int imgIndex = Convert.ToInt32(parameters["Index"]);
		                    string imgPath = parameters["Path"];

		                    if (!Directory.Exists(imgPath))
		                    {
		                        imgPath = Path.GetFullPath(HttpContext.Current.Request.PhysicalApplicationPath + imgPath);
		                        if (!Directory.Exists(imgPath))
		                            return new ImageInfo(EmptyImage);
		                    }

		                    string[] files = Directory.GetFiles(imgPath, "*.*");
		                    if (files.Length > 0 && files.Length - 1 >= imgIndex)
		                    {
		                        Array.Sort(files);
		                        imgFile = files[imgIndex];
		                        if (!File.Exists(imgFile))
		                            return new ImageInfo(EmptyImage);
		                    }
		                }
		                else if (!String.IsNullOrEmpty(parameters["Url"]))
		                {
		                    imgFile = parameters["Url"];
		                }

		                if (String.IsNullOrEmpty(parameters["format"]))
		                {
		                    string extension;
		                    if (String.IsNullOrEmpty(parameters["Url"]))
		                    {
		                        System.IO.FileInfo fi = new System.IO.FileInfo(imgFile);
		                        extension = fi.Extension.ToLower();
		                    }
		                    else
		                    {
		                        string[] parts = parameters["Url"].Split('.');
		                        extension = parts[parts.Length - 1].ToLower();
		                    }
		                    ContentType = GetImageFormat(extension);
		                }
		                ImageFileTransform imageFileTrans = new ImageFileTransform {ImageFile = imgFile};
		                ImageTransforms.Add(imageFileTrans);

		                break;

		            default:

		                string imageTransformClass = ConfigurationManager.AppSettings["DnnImageHandler." + mode];
		                string[] imageTransformClassParts = imageTransformClass.Split(',');
		                Assembly asm = Assembly.LoadFrom(Globals.ApplicationMapPath + @"\bin\" +
		                                                 imageTransformClassParts[1].Trim() + ".dll");
		                Type t = asm.GetType(imageTransformClassParts[0].Trim());
		                ImageTransform imageTransform = (ImageTransform) Activator.CreateInstance(t);

		                foreach (var key in parameters.AllKeys)
		                {
		                    PropertyInfo pi = t.GetProperty(key,
		                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
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
		    catch (Exception)
		    {
		        return new ImageInfo(EmptyImage);
		    }

		    // Resize-Transformation
		    if  (mode != "placeholder")
		    {
		        int width = String.IsNullOrEmpty(parameters["w"]) ? 0 : Convert.ToInt32(parameters["w"]);
		        int height = String.IsNullOrEmpty(parameters["h"]) ? 0 : Convert.ToInt32(parameters["h"]);
		        string size = String.IsNullOrEmpty(parameters["size"]) ? "" : parameters["size"];

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
                    if (!String.IsNullOrEmpty(parameters["w"]) && !String.IsNullOrEmpty(parameters["h"]) && width != height)
		                resizeMode = ImageResizeMode.Fill;
		        }
		        
                if (width > 0 || height > 0)
                {
                    ImageResizeTransform resizeTrans = new ImageResizeTransform
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
				ImageGammaTransform gammaTrans = new ImageGammaTransform();
				double gamma;
				if (Double.TryParse(parameters["Gamma"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out gamma) && (gamma >= 0.2 && gamma <= 5))
				{
					gammaTrans.Gamma = gamma;
					ImageTransforms.Add(gammaTrans);
				}
			}

			// Brightness adjustment
			if (!string.IsNullOrEmpty(parameters["Brightness"]))
			{
				ImageBrightnessTransform brightnessTrans = new ImageBrightnessTransform();
				int brightness;
				if (Int32.TryParse(parameters["Brightness"], out brightness))
				{
					brightnessTrans.Brightness = brightness;
					ImageTransforms.Add(brightnessTrans);
				}
			}

			// Contrast adjustment
			if (!string.IsNullOrEmpty(parameters["Contrast"]))
			{
				ImageContrastTransform contrastTrans = new ImageContrastTransform();
				double contrast;
				if (Double.TryParse(parameters["Contrast"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out contrast) && (contrast >= -100 && contrast <= 100))
				{
					contrastTrans.Contrast = contrast;
					ImageTransforms.Add(contrastTrans);
				}
			}

			// Greyscale
			if (!string.IsNullOrEmpty(parameters["Greyscale"]))
			{
				ImageGreyScaleTransform greyscaleTrans = new ImageGreyScaleTransform();
				ImageTransforms.Add(greyscaleTrans);
			}

			// Invert
			if (!string.IsNullOrEmpty(parameters["Invert"]))
			{
				ImageInvertTransform invertTrans = new ImageInvertTransform();
				ImageTransforms.Add(invertTrans);
			}

			// Rotate / Flip 
			if (!string.IsNullOrEmpty(parameters["RotateFlip"]))
			{
				ImageRotateFlipTransform rotateFlipTrans = new ImageRotateFlipTransform();
				RotateFlipType rotateFlipType = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), parameters["RotateFlip"]);
				rotateFlipTrans.RotateFlip = rotateFlipType;
				ImageTransforms.Add(rotateFlipTrans);
				
			}

            // We start the chain with an empty image
            Bitmap dummy = new Bitmap(1, 1, PixelFormat.Format24bppRgb);
            MemoryStream ms = new MemoryStream();
            dummy.Save(ms, ImageFormat.Jpeg);
            return new ImageInfo(ms.ToArray());

		}

	    private void ReadSettings()
	    {
	        string settings = ConfigurationManager.AppSettings["DnnImageHandler"];
	        if (!String.IsNullOrEmpty(settings))
	        {
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
                        default:
	                        break;
	                }
	            }
	        }
	    }

	    private void SetupCulture()
        {
            PortalSettings settings = PortalController.Instance.GetCurrentPortalSettings();
            if (settings == null) return;

            CultureInfo pageLocale = TestableLocalization.Instance.GetPageLocale(settings);
            if (pageLocale != null)
            {
                TestableLocalization.Instance.SetThreadCultures(pageLocale, settings);
            }
        }

        private ImageFormat GetImageFormat(string extension)
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