// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.IO;

    /// <summary>
    /// An abstract ImageTransform class.
    /// </summary>
    public abstract class ImageTransform
    {
        // REVIEW: should this property be abstract?

        /// <summary>
        /// Gets provides an Unique String for the image transformation.
        /// </summary>
        public virtual string UniqueString => this.GetType().FullName;

        /// <summary>
        /// Gets or sets the interpolation mode used for resizing images. The default is HighQualityBicubic.
        /// </summary>
        public InterpolationMode InterpolationMode { get; set; }

        /// <summary>
        /// Gets or sets the smoothing mode used for resizing images. The default is HighQuality.
        /// </summary>
        public SmoothingMode SmoothingMode { get; set; }

        /// <summary>
        /// Gets or sets the pixel offset mode used for resizing images. The default is HighQuality.
        /// </summary>
        public PixelOffsetMode PixelOffsetMode { get; set; }

        /// <summary>
        /// Gets or sets the compositing quality used for resizing images. The default is HighQuality.
        /// </summary>
        public CompositingQuality CompositingQuality { get; set; }

        /// <summary>
        /// Process an input image applying the image transformation.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image processed.</returns>
        public abstract Image ProcessImage(Image image);

        /// <summary>
        /// Creates a new image from stream. The created image is independent of the stream.
        /// </summary>
        /// <param name="imgStream"></param>
        /// <returns>Image object.</returns>
        public virtual Bitmap CopyImage(Stream imgStream)
        {
            using (var srcImage = new Bitmap(imgStream))
            {
                var destImage = new Bitmap(srcImage.Width, srcImage.Height);
                using (var graph = Graphics.FromImage(destImage))
                {
                    graph.CompositingMode = CompositingMode.SourceCopy;
                    graph.CompositingQuality = this.CompositingQuality;
                    graph.InterpolationMode = this.InterpolationMode;
                    graph.SmoothingMode = this.SmoothingMode;
                    graph.DrawImage(srcImage, new Rectangle(0, 0, srcImage.Width, srcImage.Height));
                }

                return destImage;
            }
        }
    }
}
