using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
    /// <summary>
    /// Placeholder ImageTransform class
    /// </summary>
	public class PlaceholderTransform : ImageTransform
	{
		/// <summary>
		/// Sets the width of the placeholder image
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Sets the Height of the placeholder image
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// Sets the Color of the border and text element
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Sets the backcolor of the placeholder element
		/// </summary>
		public Color BackColor { get; set; }

		/// <summary>
		/// Sets the text of the placeholder image. if blank dimension will be used
		/// </summary>
		public string Text { get; set; }

        /// <summary>
        /// Provides an Unique String for the image transformation
        /// </summary>
        public override string UniqueString => base.UniqueString + Width + "-" + Height + "-" + Color + "-" + BackColor + "-" + Text;

        public PlaceholderTransform()
		{
			InterpolationMode = InterpolationMode.HighQualityBicubic;
			SmoothingMode = SmoothingMode.HighQuality;
			PixelOffsetMode = PixelOffsetMode.HighQuality;
			CompositingQuality = CompositingQuality.HighQuality;
			BackColor = Color.LightGray;
			Color = Color.LightSlateGray;
			Width = 0;
			Height = 0;
			Text = "";
		}

        /// <summary>
        /// Processes an input image returning a placeholder image
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after image transformation</returns>
        public override Image ProcessImage(Image image)
		{
			// Check dimensions
			if (Width == 0 && Height > 0)
				Width = Height;
			if (Width > 0 && Height == 0)
				Height = Width;
			
			var bitmap = new Bitmap(Width, Height);
			Brush backColorBrush = new SolidBrush(BackColor);
			Brush colorBrush = new SolidBrush(Color);
			var colorPen = new Pen(Color,2);
			var text = string.IsNullOrEmpty(Text) ? $"{Width}x{Height}" : Text;

			using (var objGraphics = Graphics.FromImage(bitmap))
			{
				// Initialize graphics
				objGraphics.Clear(Color.White);
				objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
				objGraphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

				// Fill bitmap with backcolor
				
				objGraphics.FillRectangle(backColorBrush,0,0, Width,Height);
				
				// Draw border
				objGraphics.DrawRectangle(colorPen,1,1,Width-3,Height-3);

				// Determine fontsize
				var fontSize = 13;
				if (Width < 101)
					fontSize = 8;
				else if (Width < 151)
					fontSize = 10;
				else if (Width < 201)
					fontSize = 12;
				else if (Width < 301)
					fontSize = 14;
				else
					fontSize = 24;

				// Draw text on image
				// Use rectangle for text and align text to center of rectangle
				var font = new Font("Arial", fontSize, FontStyle.Bold);
			    var stringFormat = new StringFormat
			    {
			        Alignment = StringAlignment.Center,
			        LineAlignment = StringAlignment.Center
			    };

			    var rectangle = new Rectangle(5, 5, Width - 10, Height - 10);
				objGraphics.DrawString(text, font, colorBrush, rectangle, stringFormat);

				// Save indicator to file
				objGraphics.Flush();
			}
			return bitmap;
		}
	}
}
