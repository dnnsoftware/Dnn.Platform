// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// Brightness ImageTransform class.
    /// </summary>
    public class ImageBrightnessTransform : ImageTransform
    {
        public ImageBrightnessTransform()
        {
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.Brightness = 0;
        }

        /// <summary>
        /// Gets provides an Unique String for this class.
        /// </summary>
        public override string UniqueString => base.UniqueString + "-" + this.Brightness;

        /// <summary>
        /// Gets or sets the brightness value. Defaultvalue is 0. Range is -255 .. 255.
        /// </summary>
        public int Brightness { get; set; }

        /// <summary>
        /// Processes an input image applying a brightness image transformation.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
            var temp = (Bitmap)image;
            var bmap = (Bitmap)temp.Clone();
            if (this.Brightness < -255)
            {
                this.Brightness = -255;
            }

            if (this.Brightness > 255)
            {
                this.Brightness = 255;
            }

            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    int cR = c.R + this.Brightness;
                    int cG = c.G + this.Brightness;
                    int cB = c.B + this.Brightness;

                    if (cR < 0)
                    {
                        cR = 1;
                    }

                    if (cR > 255)
                    {
                        cR = 255;
                    }

                    if (cG < 0)
                    {
                        cG = 1;
                    }

                    if (cG > 255)
                    {
                        cG = 255;
                    }

                    if (cB < 0)
                    {
                        cB = 1;
                    }

                    if (cB > 255)
                    {
                        cB = 255;
                    }

                    bmap.SetPixel(i, j, Color.FromArgb((byte)cR, (byte)cG, (byte)cB));
                }
            }

            return (Bitmap)bmap.Clone();
        }
    }
}
