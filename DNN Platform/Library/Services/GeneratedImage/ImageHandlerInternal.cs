using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DotNetNuke.Services.GeneratedImage.ImageQuantization;

namespace DotNetNuke.Services.GeneratedImage
{
    internal class ImageHandlerInternal {
        private static TimeSpan s_defaultClientCacheExpiration = new TimeSpan(0, 10, 0);

        private TimeSpan _clientCacheExpiration = s_defaultClientCacheExpiration;
        private IImageStore _imageStore;
        private DateTime? _now;

        public TimeSpan ClientCacheExpiration {
            get {
                return _clientCacheExpiration;
            }
            set {
                if (value.Ticks < 0) {
                    throw new ArgumentOutOfRangeException("value", "ClientCacheExpiration must be positive");
                }
                _clientCacheExpiration = value;
                EnableClientCache = true;
            }
        }

        public ImageFormat ContentType { get; set; }

		public long ImageCompression { get; set; }

		private DateTime DateTime_Now {
            get {
                return _now ?? DateTime.Now;
            }
        }

        private IImageStore ImageStore {
            get {
                return _imageStore ?? DiskImageStore.Instance;
            }
        }

        public bool EnableClientCache { get; set; }

        public bool EnableServerCache { get; set; }

	    public string[] AllowedDomains { get; set; }

	    public bool EnableSecurityExceptions { get; set; }

	    public List<ImageTransform> ImageTransforms {
            get;
            private set;
        }

        public ImageHandlerInternal() {
            ContentType = ImageFormat.Jpeg;
			ImageCompression = 95;
            ImageTransforms = new List<ImageTransform>();
        }

        internal ImageHandlerInternal(IImageStore imageStore, DateTime now)
            : this() {
            _imageStore = imageStore;
            _now = now;
        }

        internal static string GetImageMimeType(ImageFormat format) {
            string mimeType = "image/x-unknown";

            if (format.Equals(ImageFormat.Gif)) {
                mimeType = "image/gif";
            }
            else if (format.Equals(ImageFormat.Jpeg)) {
                mimeType = "image/jpeg";
            }
            else if (format.Equals(ImageFormat.Png)) {
                mimeType = "image/png";
            }
            else if (format.Equals(ImageFormat.Bmp) || format.Equals(ImageFormat.MemoryBmp)) {
                mimeType = "image/bmp";
            }
            else if (format.Equals(ImageFormat.Tiff)) {
                mimeType = "image/tiff";
            }
            else if (format.Equals(ImageFormat.Icon)) {
                mimeType = "image/x-icon";
            }

            return mimeType;
        }

        public void HandleImageRequest(HttpContextBase context, Func<NameValueCollection, ImageInfo> imageGenCallback, string uniqueIdStringSeed) {
            context.Response.Clear();
            context.Response.ContentType = GetImageMimeType(ContentType);

            string cacheId = GetUniqueIDString(context, uniqueIdStringSeed);

            var cachePolicy = context.Response.Cache;
            cachePolicy.SetValidUntilExpires(true);
            if (EnableClientCache) {
                if (!String.IsNullOrEmpty(context.Request.Headers["If-Modified-Since"]) && !String.IsNullOrEmpty(context.Request.Headers["If-None-Match"]))
                {
                    CultureInfo provider = CultureInfo.InvariantCulture;
                    var lastMod = DateTime.ParseExact(context.Request.Headers["If-Modified-Since"], "r", provider).ToLocalTime();
                    var etag = context.Request.Headers["If-None-Match"];
                    if (lastMod + ClientCacheExpiration > DateTime_Now && etag == cacheId)
                    {
                        context.Response.StatusCode = 304;
                        context.Response.StatusDescription = "Not Modified";
                        context.Response.End();
                        return;
                    }
                }
                cachePolicy.SetCacheability(HttpCacheability.Public);
                cachePolicy.SetLastModified(DateTime_Now);
                cachePolicy.SetExpires(DateTime_Now + ClientCacheExpiration);
                cachePolicy.SetETag(cacheId);
            }

            if (EnableServerCache) {
                if (ImageStore.TryTransmitIfContains(cacheId, context.Response)) {
                    context.Response.End();
                    return;
                }
            }

            ImageInfo imageMethodData = imageGenCallback(context.Request.QueryString);

            if (imageMethodData == null) {
                throw new InvalidOperationException("The image generation handler cannot return null.");
            }

            if (imageMethodData.HttpStatusCode != null) {
                context.Response.StatusCode = (int)imageMethodData.HttpStatusCode;
                context.Response.End();
                return;
            }

            MemoryStream imageOutputBuffer = new MemoryStream();

            Debug.Assert(!(imageMethodData.Image == null && imageMethodData.ImageByteBuffer == null));
            if (imageMethodData.Image != null) {
                RenderImage(GetImageThroughTransforms(imageMethodData.Image), imageOutputBuffer);
            }
            else if (imageMethodData.ImageByteBuffer != null) {
                RenderImage(GetImageThroughTransforms(imageMethodData.ImageByteBuffer), imageOutputBuffer);
            }

            byte[] buffer = imageOutputBuffer.GetBuffer();
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);

            if (EnableServerCache) {
                ImageStore.Add(cacheId, buffer);
            }

            context.Response.End();
        }

        private string GetUniqueIDString(HttpContextBase context, string uniqueIdStringSeed) {
            StringBuilder builder = new StringBuilder();
            builder.Append(uniqueIdStringSeed);
            foreach (var key in context.Request.QueryString.AllKeys.OrderBy(k => k)) {
                builder.Append(key);
                builder.Append(context.Request.QueryString.Get(key));
            }
            foreach (var tran in ImageTransforms) {
                builder.Append(tran.UniqueString);
            }

            return GetIDFromBytes(ASCIIEncoding.ASCII.GetBytes(builder.ToString()));
        }

        private static string GetIDFromBytes(byte[] buffer) {
            byte[] result = SHA1.Create().ComputeHash(buffer);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++) {
                sb.Append(result[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        private Image GetImageThroughTransforms(Image image) {
             Image temp = image;

            foreach (var tran in ImageTransforms) {
                temp = tran.ProcessImage(temp);
            }
            return temp;
        }

        private Image GetImageThroughTransforms(byte[] buffer) {
            MemoryStream memoryStream = new MemoryStream(buffer);
            return GetImageThroughTransforms(Image.FromStream(memoryStream));
        }

		private void RenderImage(Image image, Stream outStream)
		{
			if (ContentType == ImageFormat.Gif)
			{
				OctreeQuantizer quantizer = new OctreeQuantizer(255, 8);
				using (Bitmap quantized = quantizer.Quantize(image))
				{
					quantized.Save(outStream, ImageFormat.Gif);
				}
			}
			else
			{
				EncoderParameters eps = new EncoderParameters(1);
				eps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, ImageCompression);
				ImageCodecInfo ici = GetEncoderInfo(GetImageMimeType(ContentType));
				image.Save(outStream, ici, eps);
			}
		}

		/// <summary>
		/// Returns the encoder for the specified mime type
		/// </summary>
		/// <param name="mimeType">The mime type of the content</param>
		/// <returns>ImageCodecInfo</returns>
		private static ImageCodecInfo GetEncoderInfo(String mimeType)
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			ImageCodecInfo e = encoders.Where(x => x.MimeType == mimeType).FirstOrDefault();
			return e;
		}
    }
}
