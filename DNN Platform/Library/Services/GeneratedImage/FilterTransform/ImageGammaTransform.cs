using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
	public class ImageGammaTransform : ImageTransform
	{
		/// <summary>
		/// Sets the gamma value. Defaultvalue is 0. Range is 0.2 .. 5
		/// </summary>
		public double Gamma { get; set; }


		public override string UniqueString
		{
			get { return base.UniqueString + "-" + this.Gamma.ToString(); }
		}

		public ImageGammaTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
			this.Gamma = 1;
		}

		public override Image ProcessImage(Image image)
		{
			Bitmap temp = (Bitmap)image;
			Bitmap bmap = (Bitmap)temp.Clone();
			Color c;
			byte[] gammaArray = new byte[256];
			for (int i = 0; i < 256; ++i)
			{
				gammaArray[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / Gamma)) + 0.5));
			}
			
			for (int i = 0; i < bmap.Width; i++)
			{
				for (int j = 0; j < bmap.Height; j++)
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