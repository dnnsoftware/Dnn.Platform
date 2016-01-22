using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    /// <summary>
    /// Image File ImageTransform class
    /// </summary>
    public class ImageFileTransform : ImageTransform
	{
		/// <summary>
		/// File path of the image
		/// </summary>
		public string ImageFilePath { get; set; }

        /// <summary>
        /// Url of the image
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Sets the Image to return if no image or error
        /// </summary>
        public Image EmptyImage { get; set; }

        /// <summary>
        /// Provides an Unique String for the image transformation
        /// </summary>
        public override string UniqueString => base.UniqueString + "-" +  ImageFilePath + ImageUrl;

        public ImageFileTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

        /// <summary>
        /// Processes an input image applying a file image transformation.
        /// This will return an image after read the stream from the File Path  <see cref="ImageFilePath"/> or Url <see cref="ImageUrl"/>
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after file image transformation</returns>
        public override Image ProcessImage(Image image)
        {
            return !string.IsNullOrEmpty(ImageUrl) ? 
                ProcessImageFromUrl() : 
                ProcessImageFilePath();
        }

        private Image ProcessImageFilePath()
        {
            try
            {
                using (var stream = new FileStream(ImageFilePath, FileMode.Open))
                {
                    return CopyImage(stream);
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
                return EmptyImage;
            }
        }

        private Image ProcessImageFromUrl()
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(ImageUrl);

            try
            {
                using (var httpWebReponse = (HttpWebResponse) httpWebRequest.GetResponse())
                {
                    using (var stream = httpWebReponse.GetResponseStream())
                    {
                        return CopyImage(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
                return EmptyImage;
            }
        }
	}
}