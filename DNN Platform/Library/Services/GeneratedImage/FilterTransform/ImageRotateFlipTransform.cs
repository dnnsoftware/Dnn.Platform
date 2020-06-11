// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
        public override string UniqueString => base.UniqueString + "-" + this.RotateFlip;

	    public ImageRotateFlipTransform()
		{
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
			this.RotateFlip = RotateFlipType.RotateNoneFlipNone;
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
			bmap.RotateFlip(this.RotateFlip);
			return (Bitmap)bmap.Clone();
		}
	}
}
