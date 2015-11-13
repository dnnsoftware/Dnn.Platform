using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace DotNetNuke.Services.GeneratedImage.StartTransform
{
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

		public override string UniqueString
		{
			get { return base.UniqueString + this.Width.ToString() + "-" + this.Height.ToString() + "-" + this.Color.ToString() + "-" + this.BackColor.ToString() + "-" + this.Text; }
		}

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

		public override Image ProcessImage(Image image)
		{
			// Check dimensions
			if (Width == 0 && Height > 0)
				Width = Height;
			if (Width > 0 && Height == 0)
				Height = Width;
			
			Bitmap bitmap = new Bitmap(Width, Height);
			Brush backColorBrush = new SolidBrush(BackColor);
			Brush colorBrush = new SolidBrush(Color);
			Pen colorPen = new Pen(Color,2);
			string text = (string.IsNullOrEmpty(this.Text) ? string.Format("{0}x{1}", this.Width, this.Height) : this.Text);

			using (Graphics objGraphics = Graphics.FromImage(bitmap))
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
				int fontSize = 13;
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
				StringFormat stringFormat = new StringFormat();
				stringFormat.Alignment = StringAlignment.Center;
				stringFormat.LineAlignment = StringAlignment.Center;

				Rectangle rectangle = new Rectangle(5, 5, Width - 10, Height - 10);
				objGraphics.DrawString(text, font, colorBrush, rectangle, stringFormat);

				// Save indicator to file
				objGraphics.Flush();
			}
			return (Image)bitmap;
		}
	}
}
