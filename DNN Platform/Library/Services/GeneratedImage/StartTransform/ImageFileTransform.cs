using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    /// <summary>
    /// Image File ImageTransform class
    /// </summary>
    public class ImageFileTransform : ImageTransform
	{
		/// <summary>
		/// Path or Url of image file
		/// </summary>
		public string ImageFile { get; set; }

        /// <summary>
        /// Sets the Image to return if no image or error
        /// </summary>
        public Image EmptyImage { get; set; }

        /// <summary>
        /// Provides an Unique String for the image transformation
        /// </summary>
        public override string UniqueString => base.UniqueString + "-" +  ImageFile;

        public ImageFileTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

        /// <summary>
        /// Processes an input image applying a file image transformation.
        /// This will return an image after read the stream from the <see cref="ImageFile"/> Path or Url
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after file image transformation</returns>
        public override Image ProcessImage(Image image)
		{
		    if (ImageFile.StartsWith("http"))
		    {
		        var httpWebRequest = (HttpWebRequest) WebRequest.Create(ImageFile);

		        try
		        {
		            using (var httpWebReponse = (HttpWebResponse) httpWebRequest.GetResponse())
		            {
		                using (var stream = httpWebReponse.GetResponseStream())
		                {
		                    return Image.FromStream(stream);
		                }
		            }
		        }
		        catch (Exception)
		        {
		            return EmptyImage;
		        }
		    }
		    return new Bitmap(ImageFile);
		}
	}
}