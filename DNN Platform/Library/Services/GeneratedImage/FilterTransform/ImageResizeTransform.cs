using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
	public class ImageResizeTransform : ImageTransform
	{
	    private int _width, _height, _border, _maxWidth, _maxHeight;
		private Color _backColor = Color.White;

		/// <summary>
		/// Sets the resize mode. The default value is Fit.
		/// </summary>
		public ImageResizeMode Mode { get; set; }
        
		/// <summary>
		/// Sets the width of the resulting image
		/// </summary>
		public int Width {
			get { return _width; }
		    set
		    {
		        CheckValue(value);
		        _width = value;
		    }
		}

        /// <summary>
        /// Sets the Max width of the resulting image
        /// </summary>
        public int MaxWidth
        {
            get
            {
                return _maxWidth;
            }
            set
            {
                CheckValue(value);
                _maxWidth = value;
            }
        }

		/// <summary>
		/// Sets the height of the resulting image
		/// </summary>
		public int Height {
			get {
				return _height;
			}
			set {
				CheckValue(value);
				_height = value;
			}
		}

        /// <summary>
        /// Sets the max height of the resulting image
        /// </summary>
        public int MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                CheckValue(value);
                _maxHeight = value;
            }
        }

		/// <summary>
		/// Sets the border width of the resulting image
		/// </summary>
		public int Border
		{
			get
			{
				return _border;
			}
			set
			{
				CheckValue(value);
				_border = value;
			}
		}

		/// <summary>
		/// Sets the Backcolor 
		/// </summary>
		public Color BackColor 
		{
			get	{ return _backColor; }
			set { _backColor = value; }
		}

		public ImageResizeTransform() {
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
			Mode = ImageResizeMode.Fit;
		}

		private static void CheckValue(int value) {
			if (value < 0) {
				throw new ArgumentOutOfRangeException("value");
			}
		}

		public override Image ProcessImage(Image img)
		{
            if (this.MaxWidth > 0)
            {
                if (img.Width > this.MaxWidth)
                    this.Width = this.MaxWidth;
                else
                    this.Width = img.Width;
            }

            if (this.MaxHeight > 0)
            {
                if (img.Height > this.MaxHeight)
                    this.Height = this.MaxHeight;
                else
                    this.Height = img.Height;
            }

            int scaledHeight = (int)(img.Height * ((float)this.Width / (float)img.Width));
			int scaledWidth = (int)(img.Width * ((float)this.Height / (float)img.Height));

			Image procImage;
			switch (Mode) {
				case ImageResizeMode.Fit:
					procImage =  FitImage(img, scaledHeight, scaledWidth);
					break;
				case ImageResizeMode.Crop:
					procImage = CropImage(img, scaledHeight, scaledWidth);
					break;
				case ImageResizeMode.FitSquare:
					procImage = FitSquareImage(img, scaledHeight, scaledWidth);
					break;
				case ImageResizeMode.Fill:
                    procImage =FillImage(img, scaledHeight, scaledWidth);
					break;
                default:
					Debug.Fail("Should not reach this");
					return null;
			}
			return procImage;
		}

		private Image FitImage(Image img, int scaledHeight, int scaledWidth) {
			int resizeWidth = 0;
			int resizeHeight = 0;
			if (this.Height == 0) {
				resizeWidth = this.Width;
				resizeHeight = scaledHeight;
			}
			else if (this.Width == 0) {
				resizeWidth = scaledWidth;
				resizeHeight = this.Height;
			}
			else {
				if (((float)this.Width / (float)img.Width < this.Height / (float)img.Height)) {
					resizeWidth = this.Width;
					resizeHeight = scaledHeight;
				}
				else {
					resizeWidth = scaledWidth;
					resizeHeight = this.Height;
				}
			}

			Bitmap newimage = new Bitmap(resizeWidth + 2 * _border, resizeHeight + 2 * _border);
			Graphics graphics = Graphics.FromImage(newimage);

			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;

			graphics.FillRectangle(new SolidBrush(BackColor), new Rectangle(0, 0, resizeWidth + 2 * _border, resizeHeight + 2 * _border));
			graphics.DrawImage(img, _border, _border, resizeWidth, resizeHeight);

	
			return newimage;
		}

		private Image FitSquareImage(Image img, int scaledHeight, int scaledWidth)
		{
			int resizeWidth = 0;
			int resizeHeight = 0;

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

            Bitmap newimage = new Bitmap(newDim + 2 * _border, newDim + 2 * _border);
			
			Graphics graphics = Graphics.FromImage(newimage);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;

			graphics.FillRectangle(new SolidBrush(BackColor),new Rectangle(0,0,newDim + 2*_border ,newDim + 2*_border));
			graphics.DrawImage(img, (newDim - resizeWidth) / 2 + _border, (newDim - resizeHeight) / 2 + _border, resizeWidth, resizeHeight);
			return newimage;
		}

		private Image CropImage(Image img, int scaledHeight, int scaledWidth) {
			int resizeWidth = 0;
			int resizeHeight = 0;
			if (((float)this.Width / (float)img.Width > this.Height / (float)img.Height)) {
				resizeWidth = this.Width;
				resizeHeight = scaledHeight;
			}
			else 
			{
				resizeWidth = scaledWidth;
				resizeHeight = this.Height;
			}

			Bitmap newImage = new Bitmap(this.Width, this.Height);
			
			Graphics graphics = Graphics.FromImage(newImage);
			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;
			graphics.PixelOffsetMode = PixelOffsetMode;

			graphics.DrawImage(img, (this.Width - resizeWidth) / 2, (this.Height - resizeHeight) / 2, resizeWidth, resizeHeight);
			return newImage;
		}

        private Image FillImage(Image img, int scaledHeight, int scaledWidth)
        {
            int resizeHeight = this.Height;
            int resizeWidth = this.Width;
            
            Bitmap newImage = new Bitmap(this.Width, this.Height);

            Graphics graphics = Graphics.FromImage(newImage);
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality;
            graphics.InterpolationMode = InterpolationMode;
            graphics.SmoothingMode = SmoothingMode;
            graphics.PixelOffsetMode = PixelOffsetMode;

            graphics.DrawImage(img, (this.Width - resizeWidth) / 2, (this.Height - resizeHeight) / 2, resizeWidth, resizeHeight);
            return newImage;
        }

		[Browsable(false)]
		public override string UniqueString {
			get {
				return base.UniqueString + Width + InterpolationMode.ToString() + Height + Mode.ToString();
			}
		}

		public override string ToString() {
			return "ImageResizeTransform";
		}
	}
}