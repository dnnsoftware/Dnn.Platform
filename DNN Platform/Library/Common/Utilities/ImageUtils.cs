// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    using DotNetNuke.Services.FileSystem.Internal;

    public class ImageUtils
    {
        public static Size GetSize(string sPath)
        {
            Image g = Image.FromFile(sPath);
            Size s = g.Size;
            g.Dispose();
            return s;
        }

        /// <summary>
        /// return height of image.
        /// </summary>
        /// <param name="sPath">file path of image.</param>
        /// <returns></returns>
        public static int GetHeight(string sPath)
        {
            Image g = Image.FromFile(sPath);
            int h = g.Height;
            g.Dispose();
            return h;
        }

        /// <summary>
        /// return width of image.
        /// </summary>
        /// <param name="sPath">file path of image.</param>
        /// <returns></returns>
        public static int GetWidth(string sPath)
        {
            Image g = Image.FromFile(sPath);
            int w = g.Width;
            g.Dispose();
            return w;
        }

        /// <summary>
        /// return height of image.
        /// </summary>
        /// <param name="sFile">Stream of image.</param>
        /// <returns></returns>
        public static int GetHeightFromStream(Stream sFile)
        {
            Image g = Image.FromStream(sFile, true);
            int h = g.Height;
            g.Dispose();
            return h;
        }

        /// <summary>
        /// width of image.
        /// </summary>
        /// <param name="sFile">Steam of image.</param>
        /// <returns></returns>
        public static int GetWidthFromStream(Stream sFile)
        {
            Image g = Image.FromStream(sFile, true);
            int w = g.Width;
            g.Dispose();
            return w;
        }

        /// <summary>
        /// create an image.
        /// </summary>
        /// <param name="sFile">path of load image file - will be resized according to height and width set.</param>
        /// <returns></returns>
        public static string CreateImage(string sFile)
        {
            Image g = Image.FromFile(sFile);
            int h = g.Height;
            int w = g.Width;
            g.Dispose();
            return CreateImage(sFile, h, w);
        }

        /// <summary>
        /// create an image.
        /// </summary>
        /// <param name="sFile">path of image file.</param>
        /// <param name="intHeight">height.</param>
        /// <param name="intWidth">width.</param>
        /// <returns></returns>
        public static string CreateImage(string sFile, int intHeight, int intWidth)
        {
            var fi = new FileInfo(sFile);
            string tmp = fi.FullName.Replace(fi.Extension, "_TEMP" + fi.Extension);
            if (FileWrapper.Instance.Exists(tmp))
            {
                FileWrapper.Instance.SetAttributes(tmp, FileAttributes.Normal);
                FileWrapper.Instance.Delete(tmp);
            }

            File.Copy(sFile, tmp);

            using (var fileContent = File.OpenRead(tmp))
            using (var content = CreateImage(fileContent, intHeight, intWidth, fi.Extension))
            {
                string sFileExt = fi.Extension;
                string sFileNoExtension = Path.GetFileNameWithoutExtension(sFile);

                sFile += sFileNoExtension + sFileExt;
                if (FileWrapper.Instance.Exists(sFile))
                {
                    FileWrapper.Instance.SetAttributes(sFile, FileAttributes.Normal);
                    FileWrapper.Instance.Delete(sFile);
                }

                var arrData = new byte[2048];
                using (Stream outStream = FileWrapper.Instance.Create(sFile))
                {
                    long originalPosition = content.Position;
                    content.Position = 0;

                    try
                    {
                        int intLength = content.Read(arrData, 0, arrData.Length);

                        while (intLength > 0)
                        {
                            outStream.Write(arrData, 0, intLength);
                            intLength = content.Read(arrData, 0, arrData.Length);
                        }
                    }
                    finally
                    {
                        content.Position = originalPosition;
                    }
                }
            }

            if (FileWrapper.Instance.Exists(tmp))
            {
                FileWrapper.Instance.SetAttributes(tmp, FileAttributes.Normal);
                FileWrapper.Instance.Delete(tmp);
            }

            return sFile;
        }

        public static Stream CreateImage(Stream stream, int intHeight, int intWidth, string extension)
        {
            using (var original = new Bitmap(stream))
            {
                int imgHeight, imgWidth;
                PixelFormat format = original.PixelFormat;
                if (format.ToString().Contains("Indexed"))
                {
                    format = PixelFormat.Format24bppRgb;
                }

                int newHeight = intHeight;
                int newWidth = intWidth;
                Size imgSize;
                if (original.Width > newWidth || original.Height > newHeight)
                {
                    imgSize = NewImageSize(original.Width, original.Height, newWidth, newHeight);
                    imgHeight = imgSize.Height;
                    imgWidth = imgSize.Width;
                }
                else
                {
                    imgHeight = original.Height;
                    imgWidth = original.Width;
                }

                if (imgWidth < 1)
                {
                    imgWidth = 1;
                }

                if (imgHeight < 1)
                {
                    imgHeight = 1;
                }

                imgSize = new Size(imgWidth, imgHeight);

                using (var newImg = new Bitmap(imgWidth, imgHeight, format))
                {
                    newImg.SetResolution(original.HorizontalResolution, original.VerticalResolution);

                    using (Graphics canvas = Graphics.FromImage(newImg))
                    {
                        canvas.SmoothingMode = SmoothingMode.None;
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        if (!extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            canvas.Clear(Color.White);
                            canvas.FillRectangle(Brushes.White, 0, 0, imgSize.Width, imgSize.Height);
                        }

                        canvas.DrawImage(original, 0, 0, imgSize.Width, imgSize.Height);

                        // newImg.Save
                        ImageFormat imgFormat = ImageFormat.Bmp;
                        if (extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
                        {
                            imgFormat = ImageFormat.Png;
                        }
                        else if (extension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase))
                        {
                            imgFormat = ImageFormat.Gif;
                        }
                        else if (extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase))
                        {
                            imgFormat = ImageFormat.Jpeg;
                        }

                        var content = new MemoryStream();
                        newImg.Save(content, imgFormat);
                        return content;
                    }
                }
            }
        }

        /// <summary>
        /// create a JPG image.
        /// </summary>
        /// <param name="sFile">name of image.</param>
        /// <param name="img">bitmap of image.</param>
        /// <param name="compressionLevel">image quality.</param>
        /// <returns></returns>
        public static string CreateJPG(string sFile, Bitmap img, int compressionLevel)
        {
            Graphics bmpOutput = Graphics.FromImage(img);
            bmpOutput.InterpolationMode = InterpolationMode.HighQualityBicubic;
            bmpOutput.SmoothingMode = SmoothingMode.HighQuality;
            var compressionRectange = new Rectangle(0, 0, img.Width, img.Height);

            bmpOutput.DrawImage(img, compressionRectange);

            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
            Encoder myEncoder = Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);
            var myEncoderParameter = new EncoderParameter(myEncoder, compressionLevel);
            myEncoderParameters.Param[0] = myEncoderParameter;
            if (File.Exists(sFile))
            {
                File.Delete(sFile);
            }

            try
            {
                img.Save(sFile, myImageCodecInfo, myEncoderParameters);
            }
            catch (Exception)
            {
                // suppress unexpected exceptions
            }

            img.Dispose();
            bmpOutput.Dispose();
            return sFile;
        }

        /// <summary>
        /// create an image based on a stream (read from a database).
        /// </summary>
        /// <param name="sFile">image name.</param>
        /// <param name="intHeight">height.</param>
        /// <param name="intWidth">width.</param>
        /// <returns>steam.</returns>
        public static MemoryStream CreateImageForDB(Stream sFile, int intHeight, int intWidth)
        {
            var newStream = new MemoryStream();
            Image g = Image.FromStream(sFile);
            int imgHeight, imgWidth;

            if (intHeight > 0 & intWidth > 0)
            {
                int newHeight = intHeight;
                int newWidth = intWidth;
                if (g.Width > newWidth | g.Height > newHeight)
                {
                    Size imgSize = NewImageSize(g.Width, g.Height, newWidth, newHeight);
                    imgHeight = imgSize.Height;
                    imgWidth = imgSize.Width;
                }
                else
                {
                    imgHeight = g.Height;
                    imgWidth = g.Width;
                }
            }
            else
            {
                imgWidth = g.Width;
                imgHeight = g.Height;
            }

            var imgOutput1 = new Bitmap(g, imgWidth, imgHeight);
            Graphics bmpOutput = Graphics.FromImage(imgOutput1);
            bmpOutput.InterpolationMode = InterpolationMode.HighQualityBicubic;
            bmpOutput.SmoothingMode = SmoothingMode.HighQuality;
            var compressionRectange = new Rectangle(0, 0, imgWidth, imgHeight);
            bmpOutput.DrawImage(g, compressionRectange);
            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");
            Encoder myEncoder = Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);
            var myEncoderParameter = new EncoderParameter(myEncoder, 90);
            myEncoderParameters.Param[0] = myEncoderParameter;
            imgOutput1.Save(newStream, myImageCodecInfo, myEncoderParameters);
            g.Dispose();
            imgOutput1.Dispose();
            bmpOutput.Dispose();
            return newStream;
        }

        /// <summary>
        /// return the approriate encoded for the mime-type of the image being created.
        /// </summary>
        /// <param name="myMimeType">mime type (e.g jpg/png).</param>
        /// <returns></returns>
        public static ImageCodecInfo GetEncoderInfo(string myMimeType)
        {
            try
            {
                int i;
                ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
                for (i = 0; i <= (encoders.Length - 1); i++)
                {
                    if (encoders[i].MimeType == myMimeType)
                    {
                        return encoders[i];
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// scale an image based on existing dimensions and updated requirement.
        /// </summary>
        /// <param name="currentWidth">current width.</param>
        /// <param name="currentHeight">current height.</param>
        /// <param name="newWidth">new width.</param>
        /// <param name="newHeight">new height.</param>
        /// <returns>updated calculated height/width minesions.</returns>
        public static Size NewImageSize(int currentWidth, int currentHeight, int newWidth, int newHeight)
        {
            decimal decScale = ((decimal)currentWidth / (decimal)newWidth) > ((decimal)currentHeight / (decimal)newHeight) ? Convert.ToDecimal((decimal)currentWidth / (decimal)newWidth) : Convert.ToDecimal((decimal)currentHeight / (decimal)newHeight);
            newWidth = Convert.ToInt32(Math.Floor((decimal)currentWidth / decScale));
            newHeight = Convert.ToInt32(Math.Floor((decimal)currentHeight / decScale));

            var newSize = new Size(newWidth, newHeight);

            return newSize;
        }
    }
}
