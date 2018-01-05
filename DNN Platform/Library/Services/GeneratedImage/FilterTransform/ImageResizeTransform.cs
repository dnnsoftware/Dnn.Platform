using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DotNetNuke.Services.GeneratedImage.FilterTransform
{
    /// <summary>
    /// Resize ImageTransform class
    /// </summary>
	public class ImageResizeTransform : ImageTransform
	{
	    private int _width, _height, _border, _maxWidth, _maxHeight;

        /// <summary>
		/// Sets the resize mode. The default value is Fit.
		/// </summary>
		public ImageResizeMode Mode { get; set; }
        
		/// <summary>
		/// Sets the width of the resulting image
		/// </summary>
		public int Width {
		    get
		    {
		        return _width;
		    }
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
		public Color BackColor { get; set; } = Color.White;

        public ImageResizeTransform() {
            InterpolationMode = InterpolationMode.HighQualityBicubic;
            SmoothingMode = SmoothingMode.HighQuality;
            PixelOffsetMode = PixelOffsetMode.HighQuality;
            CompositingQuality = CompositingQuality.HighQuality;
			Mode = ImageResizeMode.Fit;
		}

        /// <summary>
        /// Processes an input image applying a resize image transformation
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Image result after image transformation</returns>
        public override Image ProcessImage(Image image)
        {
            if (image == null)
                return null;    

            if (MaxWidth > 0)
            {
                Width = image.Width > MaxWidth ? MaxWidth : image.Width;
            }

            if (MaxHeight > 0)
            {
                Height = image.Height > MaxHeight ? MaxHeight : image.Height;
            }

            int scaledHeight = (int)(image.Height * ((float)Width / (float)image.Width));
			int scaledWidth = (int)(image.Width * ((float)Height / (float)image.Height));

			Image procImage;
			switch (Mode) {
				case ImageResizeMode.Fit:
					procImage = FitImage(image, scaledHeight, scaledWidth);
					break;
				case ImageResizeMode.Crop:
					procImage = CropImage(image, scaledHeight, scaledWidth);
					break;
				case ImageResizeMode.FitSquare:
					procImage = FitSquareImage(image);
					break;
				case ImageResizeMode.Fill:
                    procImage = FillImage(image);
					break;
                default:
					Debug.Fail("Should not reach this");
					return null;
			}
			return procImage;
		}
        
        private static void CheckValue(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        private Image FitImage(Image img, int scaledHeight, int scaledWidth) {
			int resizeWidth;
			int resizeHeight;
			if (Height == 0) {
				resizeWidth = Width;
				resizeHeight = scaledHeight;
			}
			else if (Width == 0) {
				resizeWidth = scaledWidth;
				resizeHeight = Height;
			}
			else {
				if (((float)Width / (float)img.Width < Height / (float)img.Height)) {
					resizeWidth = Width;
					resizeHeight = scaledHeight;
				}
				else {
					resizeWidth = scaledWidth;
					resizeHeight = Height;
				}
			}

			var newimage = new Bitmap(resizeWidth + 2 * _border, resizeHeight + 2 * _border);
			var graphics = Graphics.FromImage(newimage);

			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;

			graphics.FillRectangle(new SolidBrush(BackColor), new Rectangle(0, 0, resizeWidth + 2 * _border, resizeHeight + 2 * _border));
			graphics.DrawImage(img, _border, _border, resizeWidth, resizeHeight);
            
			return newimage;
		}

		private Image FitSquareImage(Image img)
		{
			int resizeWidth;
			int resizeHeight;

            int newDim = Width > 0 ? Width : Height;

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

            var newimage = new Bitmap(newDim + 2 * _border, newDim + 2 * _border);
			var graphics = Graphics.FromImage(newimage);

			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;

			graphics.FillRectangle(new SolidBrush(BackColor),new Rectangle(0,0,newDim + 2*_border ,newDim + 2*_border));
			graphics.DrawImage(img, (newDim - resizeWidth) / 2 + _border, (newDim - resizeHeight) / 2 + _border, resizeWidth, resizeHeight);
			return newimage;
		}

		private Image CropImage(Image img, int scaledHeight, int scaledWidth) {
			int resizeWidth;
			int resizeHeight;
			if ((float)Width / (float)img.Width > Height / (float)img.Height) {
				resizeWidth = Width;
				resizeHeight = scaledHeight;
			}
			else 
			{
				resizeWidth = scaledWidth;
				resizeHeight = Height;
			}

			var newImage = new Bitmap(Width, Height);
			var graphics = Graphics.FromImage(newImage);

			graphics.CompositingMode = CompositingMode.SourceCopy;
			graphics.CompositingQuality = CompositingQuality;
			graphics.InterpolationMode = InterpolationMode;
			graphics.SmoothingMode = SmoothingMode;
			graphics.PixelOffsetMode = PixelOffsetMode;

			graphics.DrawImage(img, (Width - resizeWidth) / 2, (Height - resizeHeight) / 2, resizeWidth, resizeHeight);
			return newImage;
		}

        private Image FillImage(Image img)
        {
            int resizeHeight = Height;
            int resizeWidth = Width;
            
            var newImage = new Bitmap(Width, Height);
            var graphics = Graphics.FromImage(newImage);

            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.CompositingQuality = CompositingQuality;
            graphics.InterpolationMode = InterpolationMode;
            graphics.SmoothingMode = SmoothingMode;
            graphics.PixelOffsetMode = PixelOffsetMode;

            graphics.DrawImage(img, (Width - resizeWidth) / 2, (Height - resizeHeight) / 2, resizeWidth, resizeHeight);
            return newImage;
        }

        /// <summary>
        /// Provides an Unique String for this transformation
        /// </summary>
		[Browsable(false)]
		public override string UniqueString => base.UniqueString + Width + InterpolationMode + Height + Mode;

        public override string ToString() {
			return "ImageResizeTransform";
		}
	}
}