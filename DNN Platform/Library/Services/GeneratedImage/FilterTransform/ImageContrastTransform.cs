// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// Constrast ImageTransform class.
    /// </summary>
    public class ImageContrastTransform : ImageTransform
    {
        public ImageContrastTransform()
        {
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.Contrast = 0;
        }

        /// <summary>
        /// Gets provides an Unique String for this class.
        /// </summary>
        public override string UniqueString => base.UniqueString + "-" + this.Contrast;

        /// <summary>
        /// Gets or sets the contrast value. Defaultvalue is 0. Range is -100 .. 100.
        /// </summary>
        public double Contrast { get; set; }

        /// <summary>
        /// Processes an input image applying a contrast image transformation.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
            var temp = (Bitmap)image;
            var bmap = (Bitmap)temp.Clone();
            if (this.Contrast < -100)
            {
                this.Contrast = -100;
            }

            if (this.Contrast > 100)
            {
                this.Contrast = 100;
            }

            this.Contrast = (100.0 + this.Contrast) / 100.0;
            this.Contrast *= this.Contrast;
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    double pR = c.R / 255.0;
                    pR -= 0.5;
                    pR *= this.Contrast;
                    pR += 0.5;
                    pR *= 255;
                    if (pR < 0)
                    {
                        pR = 0;
                    }

                    if (pR > 255)
                    {
                        pR = 255;
                    }

                    double pG = c.G / 255.0;
                    pG -= 0.5;
                    pG *= this.Contrast;
                    pG += 0.5;
                    pG *= 255;
                    if (pG < 0)
                    {
                        pG = 0;
                    }

                    if (pG > 255)
                    {
                        pG = 255;
                    }

                    double pB = c.B / 255.0;
                    pB -= 0.5;
                    pB *= this.Contrast;
                    pB += 0.5;
                    pB *= 255;
                    if (pB < 0)
                    {
                        pB = 0;
                    }

                    if (pB > 255)
                    {
                        pB = 255;
                    }

                    bmap.SetPixel(i, j, Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
                }
            }

            return (Bitmap)bmap.Clone();
        }
    }
}
