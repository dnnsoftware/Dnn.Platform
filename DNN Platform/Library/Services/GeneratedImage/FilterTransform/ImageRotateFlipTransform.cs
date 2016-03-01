using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    /// <summary>
    /// Rotation ImageTransform class
    /// </summary>
	public class ImageRotateFlipTransform : ImageTransform
	{
		/// <summary>
        /// Sets the type of rotation / flip . Defaultvalue is RotateNoneFlipNone
		/// </summary>
		public RotateFlipType RotateFlip { get; set; }

        /// <summary>
        /// Provides an Unique String for this transformation
        /// </summary>
        public override string UniqueString => base.UniqueString + "-" + RotateFlip;

	    public ImageRotateFlipTransform()
		{
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
			RotateFlip = RotateFlipType.RotateNoneFlipNone;
		}
        
        /// <summary>
        /// Processes an input image applying a rotation image transformation
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after image transformation</returns>
        public override Image ProcessImage(Image image)
		{
			var temp = (Bitmap)image;
			var bmap = (Bitmap)temp.Clone();
			bmap.RotateFlip(RotateFlip);
			return (Bitmap)bmap.Clone();
		}
	}
}