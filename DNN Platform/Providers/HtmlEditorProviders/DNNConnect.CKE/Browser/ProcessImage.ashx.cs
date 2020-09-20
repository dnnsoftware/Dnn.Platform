using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;

namespace DNNConnect.CKEditorProvider.Browser
{

    /// <summary>
    /// The process image.
    /// </summary>
    public class ProcessImage : IHttpHandler
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether IsReusable.
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The get thumb abort.
        /// </summary>
        /// <returns>
        /// Returns abort.
        /// </returns>
        public bool GetThumbAbort()
        {
            return false;
        }

        #endregion

        #region Implemented Interfaces

        #region IHttpHandler

        /// <summary>
        /// The process request.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        public void ProcessRequest(HttpContext context)
        {
            float imageH = float.Parse(context.Request["imageH"]);
            float imageW = float.Parse(context.Request["imageW"]);
            float angle = float.Parse(context.Request["imageRotate"]);
            int sourceImageId = int.Parse(context.Request["fileId"]);
            float imageX = float.Parse(context.Request["imageX"]);
            float imageY = float.Parse(context.Request["imageY"]);
            float selectorH = float.Parse(context.Request["selectorH"]);
            float selectorW = float.Parse(context.Request["selectorW"]);
            float selectorX = float.Parse(context.Request["selectorX"]);
            float selectorY = float.Parse(context.Request["selectorY"]);
            float viewPortH = float.Parse(context.Request["viewPortH"]);
            float viewPortW = float.Parse(context.Request["viewPortW"]);

            bool bSaveFile;

            try
            {
                bSaveFile = bool.Parse(context.Request["saveFile"]);
            }
            catch (Exception)
            {
                bSaveFile = false;
            }

            string sNewFileName = null;

            if (!string.IsNullOrEmpty(context.Request["newFileName"]))
            {
                sNewFileName = context.Request["newFileName"];
            }

            float pWidth = imageW;
            float pHeight = imageH;

            var file = FileManager.Instance.GetFile(sourceImageId);
            if (file == null)
            {
                return;
                
            }

            Bitmap img = (Bitmap)Image.FromStream(FileManager.Instance.GetFileContent(file));

            // Resize
            Bitmap imageP = ResizeImage(img, Convert.ToInt32(pWidth), Convert.ToInt32(pHeight));

            // Rotate if angle is not 0.00 or 360
            if (angle > 0.0F && angle < 360.00F)
            {
                imageP = (Bitmap)RotateImage(imageP, angle);
                pWidth = imageP.Width;
                pHeight = imageP.Height;
            }

            // Calculate Coords of the Image into the ViewPort
            float srcX;
            float dstX;
            float srcY;
            float dstY;

            if (pWidth > viewPortW)
            {
                srcX = Math.Abs(imageX - Math.Abs((imageW - pWidth) / 2));
                dstX = 0;
            }
            else
            {
                srcX = 0;
                dstX = imageX + ((imageW - pWidth) / 2);
            }

            if (pHeight > viewPortH)
            {
                srcY = Math.Abs(imageY - Math.Abs((imageH - pHeight) / 2));
                dstY = 0;
            }
            else
            {
                srcY = 0;
                dstY = imageY + ((imageH - pHeight) / 2);
            }

            // Get Image viewed into the ViewPort
            imageP = ImageCopy(imageP, dstX, dstY, srcX, srcY, viewPortW, viewPortH);

            // Get Selector Portion
            imageP = ImageCopy(imageP, 0, 0, selectorX, selectorY, selectorW, selectorH);

            if (bSaveFile)
            {
                context.Response.ContentType = "text/plain";

                var sourceFolder = FolderManager.Instance.GetFolder(file.FolderId);
                if (PortalSettings.Current != null 
                        && !HasWritePermission(sourceFolder.FolderPath))
                {
                    throw new SecurityException("You don't have write permission to save files under this folder.");
                }

                using (var stream = new MemoryStream())
                {
                    var fileName = GenerateName(file, sNewFileName);
                    imageP.Save(stream, img.RawFormat);
                    FileManager.Instance.AddFile(sourceFolder, fileName, stream);
                }
            }
            else
            {
                context.Response.ContentType = "image/jpeg";
                imageP.Save(context.Response.OutputStream, ImageFormat.Jpeg);
            }

            imageP.Dispose();
            img.Dispose();
        }

        private bool HasWritePermission(string relativePath)
        {
            var portalId = PortalSettings.Current.PortalId;
            return FolderPermissionController.HasFolderPermission(portalId, relativePath, "WRITE");
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Generats the New File Path
        /// </summary>
        /// <param name="sNewFileName">
        /// New File Name for the Image
        /// </param>
        /// <param name="sSourceFullPath">
        /// The Full Path of the Original Image
        /// </param>
        /// <returns>
        /// The generate name.
        /// </returns>
        private static string GenerateName(IFileInfo file, string sNewFileName)
        {
            string sNewFilePath = !string.IsNullOrEmpty(sNewFileName)
                                      ? $"{CleanName(sNewFileName)}.{file.Extension}"
                                      : $"{Path.GetFileNameWithoutExtension(sNewFileName)}_crop.{file.Extension}";

            int iCounter = 0;

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            while (FileManager.Instance.FileExists(folder, sNewFilePath))
            {
                iCounter++;

                string sFileNameNoExt = Path.GetFileNameWithoutExtension(sNewFilePath);
                sNewFilePath = $"{sFileNameNoExt}_{iCounter}.{file.Extension}";
            }

            return sNewFilePath;
        }

        /// <summary>
        /// The image copy.
        /// </summary>
        /// <param name="srcBitmap">
        /// The src bitmap.
        /// </param>
        /// <param name="dstX">
        /// The dst x.
        /// </param>
        /// <param name="dstY">
        /// The dst y.
        /// </param>
        /// <param name="srcX">
        /// The src x.
        /// </param>
        /// <param name="srcY">
        /// The src y.
        /// </param>
        /// <param name="dstWidth">
        /// The dst width.
        /// </param>
        /// <param name="dstHeight">
        /// The dst height.
        /// </param>
        /// <returns>
        /// Returns the copied Bitmap
        /// </returns>
        private static Bitmap ImageCopy(
            Image srcBitmap, float dstX, float dstY, float srcX, float srcY, float dstWidth, float dstHeight)
        {
            // Create the new bitmap and associated graphics object
            RectangleF sourceRec = new RectangleF(srcX, srcY, dstWidth, dstHeight);
            RectangleF destRec = new RectangleF(dstX, dstY, dstWidth, dstHeight);
            Bitmap bmp = new Bitmap(Convert.ToInt32(dstWidth), Convert.ToInt32(dstHeight));
            Graphics g = Graphics.FromImage(bmp);

            g.DrawImage(srcBitmap, destRec, sourceRec, GraphicsUnit.Pixel);
            g.Dispose();

            return bmp;
        }

        /// <summary>
        /// Method to rotate an image either clockwise or counter-clockwise
        /// </summary>
        /// <param name="img">
        /// the image to be rotated
        /// </param>
        /// <param name="rotationAngle">
        /// the angle (in degrees). 
        ///   Positive values will rotate clockwise
        ///   negative values will rotate counter-clockwise
        /// </param>
        /// <returns>
        /// Returns the Rotated Image
        /// </returns>
        private static Image RotateImage(Image img, double rotationAngle)
        {
            Bitmap returnBitmap = new Bitmap(img.Width, img.Height + 1);

            Graphics g = Graphics.FromImage(returnBitmap);

            g.TranslateTransform((float)img.Width / 2, (float)img.Height / 2);
            g.RotateTransform((float)rotationAngle);
            g.TranslateTransform(-(float)img.Width / 2, -(float)img.Height / 2);

            g.DrawImage(img, img.Width / 2 - img.Height / 2, img.Height / 2 - img.Width / 2, img.Height, img.Width);

            return returnBitmap;
        }

        /// <summary>
        /// The resize image.
        /// </summary>
        /// <param name="img">
        /// The img.
        /// </param>
        /// <param name="width">
        /// The width.
        /// </param>
        /// <param name="height">
        /// The height.
        /// </param>
        /// <returns>
        /// Returns the Resized Bitmap
        /// </returns>
        private Bitmap ResizeImage(Image img, int width, int height)
        {
            Image.GetThumbnailImageAbort callback = GetThumbAbort;
            return (Bitmap)img.GetThumbnailImage(width, height, callback, IntPtr.Zero);
        }

        private static string CleanName(string name)
        {
            name = name.Replace("\\", "/");
            if (name.Contains("/"))
            {
                name = name.Substring(name.LastIndexOf('/') + 1);
            }

            return name;
        }

        #endregion
    }
}