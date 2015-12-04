using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    /// <summary>
    /// Constrast ImageTransform class
    /// </summary>
	public class ImageContrastTransform : ImageTransform
	{
		/// <summary>
		/// Sets the contrast value. Defaultvalue is 0. Range is -100 .. 100
		/// </summary>
		public double Contrast { get; set; }
        
        /// <summary>
        /// Provides an Unique String for this class
        /// </summary>
        public override string UniqueString => base.UniqueString + "-" + Contrast;

	    public ImageContrastTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
			Contrast = 0;
		}

        /// <summary>
        /// Processes an input image applying a contrast image transformation
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after image transformation</returns>
        public override Image ProcessImage(Image image)
		{
			var temp = (Bitmap)image;
			var bmap = (Bitmap)temp.Clone();
			if (Contrast < -100) Contrast = -100;
			if (Contrast > 100) Contrast = 100;
			Contrast = (100.0 + Contrast) / 100.0;
			Contrast *= Contrast;
			Color c;
			for (int i = 0; i < bmap.Width; i++)
			{
				for (int j = 0; j < bmap.Height; j++)
				{
					c = bmap.GetPixel(i, j);
					double pR = c.R / 255.0;
					pR -= 0.5;
					pR *= Contrast;
					pR += 0.5;
					pR *= 255;
					if (pR < 0) pR = 0;
					if (pR > 255) pR = 255;

					double pG = c.G / 255.0;
					pG -= 0.5;
					pG *= Contrast;
					pG += 0.5;
					pG *= 255;
					if (pG < 0) pG = 0;
					if (pG > 255) pG = 255;

					double pB = c.B / 255.0;
					pB -= 0.5;
					pB *= Contrast;
					pB += 0.5;
					pB *= 255;
					if (pB < 0) pB = 0;
					if (pB > 255) pB = 255;

					bmap.SetPixel(i, j, Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
				}
			}
			return (Bitmap)bmap.Clone();
		}
	}
}