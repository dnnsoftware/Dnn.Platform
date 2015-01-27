using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
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

		public override string UniqueString
		{
			get
			{
				return base.UniqueString + "-" +  this.ImageFile;
			}
		}

        public ImageFileTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

		public override Image ProcessImage(Image image)
		{
		    if (ImageFile.StartsWith("http"))
		    {
		        HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(ImageFile);

		        try
		        {
		            using (HttpWebResponse httpWebReponse = (HttpWebResponse) httpWebRequest.GetResponse())
		            {
		                using (Stream stream = httpWebReponse.GetResponseStream())
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
		    else
		    {
                return new Bitmap(ImageFile);
		    }
		}
	}
}