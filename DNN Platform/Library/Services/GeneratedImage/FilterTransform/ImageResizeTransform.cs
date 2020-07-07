// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    /// <summary>
    /// Resize ImageTransform class.
    /// </summary>
    public class ImageResizeTransform : ImageTransform
    {
        private int _width;
        private int _height;
        private int _border;
        private int _maxWidth;
        private int _maxHeight;

        public ImageResizeTransform()
        {
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.Mode = ImageResizeMode.Fit;
        }

        /// <summary>
        /// Gets provides an Unique String for this transformation.
        /// </summary>
        [Browsable(false)]
        public override string UniqueString => base.UniqueString + this.Width + this.InterpolationMode + this.Height + this.Mode;

        /// <summary>
        /// Gets or sets the resize mode. The default value is Fit.
        /// </summary>
        public ImageResizeMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the width of the resulting image.
        /// </summary>
        public int Width
        {
            get
            {
                return this._width;
            }

            set
            {
                CheckValue(value);
                this._width = value;
            }
        }

        /// <summary>
        /// Gets or sets the Max width of the resulting image.
        /// </summary>
        public int MaxWidth
        {
            get
            {
                return this._maxWidth;
            }

            set
            {
                CheckValue(value);
                this._maxWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets the height of the resulting image.
        /// </summary>
        public int Height
        {
            get
            {
                return this._height;
            }

            set
            {
                CheckValue(value);
                this._height = value;
            }
        }

        /// <summary>
        /// Gets or sets the max height of the resulting image.
        /// </summary>
        public int MaxHeight
        {
            get
            {
                return this._maxHeight;
            }

            set
            {
                CheckValue(value);
                this._maxHeight = value;
            }
        }

        /// <summary>
        /// Gets or sets the border width of the resulting image.
        /// </summary>
        public int Border
        {
            get
            {
                return this._border;
            }

            set
            {
                CheckValue(value);
                this._border = value;
            }
        }

        /// <summary>
        /// Gets or sets the Backcolor.
        /// </summary>
        public Color BackColor { get; set; } = Color.White;

        /// <summary>
        /// Processes an input image applying a resize image transformation.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
            if (image == null)
            {
                return null;
            }

            if (this.MaxWidth > 0)
            {
                this.Width = image.Width > this.MaxWidth ? this.MaxWidth : image.Width;
            }

            if (this.MaxHeight > 0)
            {
                this.Height = image.Height > this.MaxHeight ? this.MaxHeight : image.Height;
            }

            int scaledHeight = (int)(image.Height * ((float)this.Width / (float)image.Width));
            int scaledWidth = (int)(image.Width * ((float)this.Height / (float)image.Height));

            Image procImage;
            switch (this.Mode)
            {
                case ImageResizeMode.Fit:
                    procImage = this.FitImage(image, scaledHeight, scaledWidth);
                    break;
                case ImageResizeMode.Crop:
                    procImage = this.CropImage(image, scaledHeight, scaledWidth);
                    break;
                case ImageResizeMode.FitSquare:
                    procImage = this.FitSquareImage(image);
                    break;
                case ImageResizeMode.Fill:
                    procImage = this.FillImage(image);
                    break;
                default:
                    Debug.Fail("Should not reach this");
                    return null;
            }

            return procImage;
        }

        public override string ToString()
        {
            return "ImageResizeTransform";
        }

        private static void CheckValue(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private Image FitImage(Image img, int scaledHeight, int scaledWidth)
        {
            int resizeWidth;
            int resizeHeight;
            if (this.Height == 0)
            {
                resizeWidth = this.Width;
                resizeHeight = scaledHeight;
            }
            else if (this.Width == 0)
            {
                resizeWidth = scaledWidth;
                resizeHeight = this.Height;
            }
            else
            {
                if ((float)this.Width / (float)img.Width < this.Height / (float)img.Height)
                {
                    resizeWidth = this.Width;
                    resizeHeight = scaledHeight;
                }
                else
                {
                    resizeWidth = scaledWidth;
                    resizeHeight = this.Height;
                }
            }

            var newimage = new Bitmap(resizeWidth + (2 * this._border), resizeHeight + (2 * this._border));
            var graphics = Graphics.FromImage(newimage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;

            graphics.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, 0, resizeWidth + (2 * this._border), resizeHeight + (2 * this._border)));
            graphics.DrawImage(img, this._border, this._border, resizeWidth, resizeHeight);

            return newimage;
        }

        private Image FitSquareImage(Image img)
        {
            int resizeWidth;
            int resizeHeight;

            int newDim = this.Width > 0 ? this.Width : this.Height;

            if (img.Height > img.Width)
            {
                resizeWidth = Convert.ToInt32((float)img.Width / (float)img.Height * newDim);
                resizeHeight = newDim;
            }
            else
            {
                resizeWidth = newDim;
                resizeHeight = Convert.ToInt32((float)img.Height / (float)img.Width * newDim);
            }

            var newimage = new Bitmap(newDim + (2 * this._border), newDim + (2 * this._border));
            var graphics = Graphics.FromImage(newimage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;

            graphics.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, 0, newDim + (2 * this._border), newDim + (2 * this._border)));
            graphics.DrawImage(img, ((newDim - resizeWidth) / 2) + this._border, ((newDim - resizeHeight) / 2) + this._border, resizeWidth, resizeHeight);
            return newimage;
        }

        private Image CropImage(Image img, int scaledHeight, int scaledWidth)
        {
            int resizeWidth;
            int resizeHeight;
            if ((float)this.Width / (float)img.Width > this.Height / (float)img.Height)
            {
                resizeWidth = this.Width;
                resizeHeight = scaledHeight;
            }
            else
            {
                resizeWidth = scaledWidth;
                resizeHeight = this.Height;
            }

            var newImage = new Bitmap(this.Width, this.Height);
            var graphics = Graphics.FromImage(newImage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;
            graphics.PixelOffsetMode = this.PixelOffsetMode;

            graphics.DrawImage(img, (this.Width - resizeWidth) / 2, (this.Height - resizeHeight) / 2, resizeWidth, resizeHeight);
            return newImage;
        }

        private Image FillImage(Image img)
        {
            int resizeHeight = this.Height;
            int resizeWidth = this.Width;

            var newImage = new Bitmap(this.Width, this.Height);
            var graphics = Graphics.FromImage(newImage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = this.CompositingQuality;
            graphics.InterpolationMode = this.InterpolationMode;
            graphics.SmoothingMode = this.SmoothingMode;
            graphics.PixelOffsetMode = this.PixelOffsetMode;

            graphics.DrawImage(img, (this.Width - resizeWidth) / 2, (this.Height - resizeHeight) / 2, resizeWidth, resizeHeight);
            return newImage;
        }
    }
}
