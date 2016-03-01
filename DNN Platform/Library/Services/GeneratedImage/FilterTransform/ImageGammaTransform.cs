using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    /// <summary>
    /// Gamma ImageTransform class
    /// </summary>
    public class ImageGammaTransform : ImageTransform
	{
		/// <summary>
		/// Sets the gamma value. Defaultvalue is 0. Range is 0.2 .. 5
		/// </summary>
		public double Gamma { get; set; }

        /// <summary>
        /// Provides an Unique String for this class
        /// </summary>
        public override string UniqueString => base.UniqueString + "-" + Gamma;

	    public ImageGammaTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
			Gamma = 1;
		}

        /// <summary>
        /// Processes an input image applying a gamma image transformation
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after image transformation</returns>
        public override Image ProcessImage(Image image)
		{
			var temp = (Bitmap)image;
			var bmap = (Bitmap)temp.Clone();
			Color c;
			byte[] gammaArray = new byte[256];
			for (var i = 0; i < 256; ++i)
			{
				gammaArray[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / Gamma)) + 0.5));
			}
			
			for (var i = 0; i < bmap.Width; i++)
			{
				for (var j = 0; j < bmap.Height; j++)
				{
					c = bmap.GetPixel(i, j);
					bmap.SetPixel(i, j, Color.FromArgb(gammaArray[c.R],
					   gammaArray[c.G], gammaArray[c.B]));
				}
			}
			return (Image)bmap.Clone();
		}
	}
}