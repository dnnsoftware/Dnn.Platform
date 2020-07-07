// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// Grey Scale ImageTransform class.
    /// </summary>
    public class ImageGreyScaleTransform : ImageTransform
    {
        public ImageGreyScaleTransform()
        {
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
        }

        /// <summary>
        /// Processes an input image applying a grey scale image transformation.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
            var temp = (Bitmap)image;
            var bmap = (Bitmap)temp.Clone();
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)((.299 * c.R) + (.587 * c.G) + (.114 * c.B));

                    bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }

            return (Bitmap)bmap.Clone();
        }
    }
}
