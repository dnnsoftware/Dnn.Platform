// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;

    /// <summary>
    /// Placeholder ImageTransform class.
    /// </summary>
    public class PlaceholderTransform : ImageTransform
    {
        public PlaceholderTransform()
        {
            this.InterpolationMode = InterpolationMode.HighQualityBicubic;
            this.SmoothingMode = SmoothingMode.HighQuality;
            this.PixelOffsetMode = PixelOffsetMode.HighQuality;
            this.CompositingQuality = CompositingQuality.HighQuality;
            this.BackColor = Color.LightGray;
            this.Color = Color.LightSlateGray;
            this.Width = 0;
            this.Height = 0;
            this.Text = string.Empty;
        }

        /// <summary>
        /// Gets provides an Unique String for the image transformation.
        /// </summary>
        public override string UniqueString => base.UniqueString + this.Width + "-" + this.Height + "-" + this.Color + "-" + this.BackColor + "-" + this.Text;

        /// <summary>
        /// Gets or sets the width of the placeholder image.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the Height of the placeholder image.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the Color of the border and text element.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the backcolor of the placeholder element.
        /// </summary>
        public Color BackColor { get; set; }

        /// <summary>
        /// Gets or sets the text of the placeholder image. if blank dimension will be used.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Processes an input image returning a placeholder image.
        /// </summary>
        /// <param name="image">Input image.</param>
        /// <returns>Image result after image transformation.</returns>
        public override Image ProcessImage(Image image)
        {
            // Check dimensions
            if (this.Width == 0 && this.Height > 0)
            {
                this.Width = this.Height;
            }

            if (this.Width > 0 && this.Height == 0)
            {
                this.Height = this.Width;
            }

            var bitmap = new Bitmap(this.Width, this.Height);
            Brush backColorBrush = new SolidBrush(this.BackColor);
            Brush colorBrush = new SolidBrush(this.Color);
            var colorPen = new Pen(this.Color, 2);
            var text = string.IsNullOrEmpty(this.Text) ? $"{this.Width}x{this.Height}" : this.Text;

            using (var objGraphics = Graphics.FromImage(bitmap))
            {
                // Initialize graphics
                objGraphics.Clear(Color.White);
                objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                objGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                // Fill bitmap with backcolor
                objGraphics.FillRectangle(backColorBrush, 0, 0, this.Width, this.Height);

                // Draw border
                objGraphics.DrawRectangle(colorPen, 1, 1, this.Width - 3, this.Height - 3);

                // Determine fontsize
                var fontSize = 13;
                if (this.Width < 101)
                {
                    fontSize = 8;
                }
                else if (this.Width < 151)
                {
                    fontSize = 10;
                }
                else if (this.Width < 201)
                {
                    fontSize = 12;
                }
                else if (this.Width < 301)
                {
                    fontSize = 14;
                }
                else
                {
                    fontSize = 24;
                }

                // Draw text on image
                // Use rectangle for text and align text to center of rectangle
                var font = new Font("Arial", fontSize, FontStyle.Bold);
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };

                var rectangle = new Rectangle(5, 5, this.Width - 10, this.Height - 10);
                objGraphics.DrawString(text, font, colorBrush, rectangle, stringFormat);

                // Save indicator to file
                objGraphics.Flush();
            }

            return bitmap;
        }
    }
}
