using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
	public class ImageInvertTransform : ImageTransform
	{
		public ImageInvertTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
		}

		public override Image ProcessImage(Image image)
		{
			Bitmap temp = (Bitmap)image;
			Bitmap bmap = (Bitmap)temp.Clone();
			Color c;
			for (int i = 0; i < bmap.Width; i++)
			{
				for (int j = 0; j < bmap.Height; j++)
				{
					c = bmap.GetPixel(i, j);
					bmap.SetPixel(i, j, Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B));
				}
			}
			return (Bitmap)bmap.Clone();
		}
	}
}