using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
	public class ImageRotateFlipTransform : ImageTransform
	{
		/// <summary>
        /// Sets the type of rotation / flip . Defaultvalue is RotateNoneFlipNone
		/// </summary>
		public RotateFlipType RotateFlip { get; set; }

		public override string UniqueString
		{
			get { return base.UniqueString + "-" + this.RotateFlip; }
		}

		public ImageRotateFlipTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
			this.RotateFlip = RotateFlipType.RotateNoneFlipNone;
		}

		public override Image ProcessImage(Image image)
		{
			Bitmap temp = (Bitmap)image;
			Bitmap bmap = (Bitmap)temp.Clone();
			bmap.RotateFlip(RotateFlip);
			return (Bitmap)bmap.Clone();
		}
	}
}