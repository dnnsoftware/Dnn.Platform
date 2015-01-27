using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage
{
    /// <summary>
    /// An abstract ImageTransform class
    /// </summary>
    public abstract class ImageTransform {

		/// <summary>
		/// Sets the interpolation mode used for resizing images. The default is HighQualityBicubic.
		/// </summary>
		public InterpolationMode InterpolationMode { get; set; }

		/// <summary>
		/// Sets the smoothing mode used for resizing images. The default is HighQuality.
		/// </summary>
		public SmoothingMode SmoothingMode { get; set; }

		/// <summary>
        /// Sets the pixel offset mode used for resizing images. The default is HighQuality.
		/// </summary>
		public PixelOffsetMode PixelOffsetMode { get; set; }

		/// <summary>
        /// Sets the compositing quality used for resizing images. The default is HighQuality.
		/// </summary>
		public CompositingQuality CompositingQuality { get; set; }

		public abstract Image ProcessImage(Image image);
        
        // REVIEW: should this property be abstract?
        public virtual string UniqueString {
            get {
                return GetType().FullName;
            }
        }
    }
}
