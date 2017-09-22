using System;
using System.Collections;
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.GeneratedImage.ImageQuantization;
using DotNetNuke.Services.Log.EventLog;

namespace DotNetNuke.Services.GeneratedImage
{
    internal class ImageHandlerInternal
    {
        private static TimeSpan defaultClientCacheExpiration = new TimeSpan(0, 10, 0);

        private TimeSpan _clientCacheExpiration = defaultClientCacheExpiration;
        private IImageStore _imageStore;
        private DateTime? _now;

        public TimeSpan ClientCacheExpiration
        {
            get
            {
                return _clientCacheExpiration;
            }
            set
            {
                if (value.Ticks < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "ClientCacheExpiration must be positive");
                }
                _clientCacheExpiration = value;
                EnableClientCache = true;
            }
        }

        public ImageFormat ContentType { get; set; }

        public long ImageCompression { get; set; }

        private DateTime DateTime_Now
        {
            get
            {
                return _now ?? DateTime.Now;
            }
        }

        private IImageStore ImageStore
        {
            get
            {
                return _imageStore ?? DiskImageStore.Instance;
            }
        }

        public int IPCountMax
        {
            set { IPCount.MaxCount = value; }
            get { return IPCount.MaxCount; }
        }

        public TimeSpan IpCountPurgeInterval
        {
            set { IPCount.PurgeInterval = value; }
            get { return IPCount.PurgeInterval; }
        }

        public bool EnableClientCache { get; set; }

        public bool EnableServerCache { get; set; }

        public bool EnableIPCount { get; set; }

        public bool AllowStandalone { get; set; }

        public string[] AllowedDomains { get; set; }

        public bool LogSecurity { get; set; }

        public List<ImageTransform> ImageTransforms
        {
            get;
            private set;
        }

        public ImageHandlerInternal()
        {
            ContentType = ImageFormat.Jpeg;
            ImageCompression = 95;
            ImageTransforms = new List<ImageTransform>();
            AllowStandalone = false;
        }

        internal ImageHandlerInternal(IImageStore imageStore, DateTime now)
            : this()
        {
            _imageStore = imageStore;
            _now = now;
        }

        internal static string GetImageMimeType(ImageFormat format)
        {
            string mimeType = "image/x-unknown";

            if (format.Equals(ImageFormat.Gif))
            {
                mimeType = "image/gif";
            }
            else if (format.Equals(ImageFormat.Jpeg))
            {
                mimeType = "image/jpeg";
            }
            else if (format.Equals(ImageFormat.Png))
            {
                mimeType = "image/png";
            }
            else if (format.Equals(ImageFormat.Bmp) || format.Equals(ImageFormat.MemoryBmp))
            {
                mimeType = "image/bmp";
            }
            else if (format.Equals(ImageFormat.Tiff))
            {
                mimeType = "image/tiff";
            }
            else if (format.Equals(ImageFormat.Icon))
            {
                mimeType = "image/x-icon";
            }

            return mimeType;
        }

        public void HandleImageRequest(HttpContextBase context, Func<NameValueCollection, ImageInfo> imageGenCallback, string uniqueIdStringSeed)
        {
            context.Response.Clear();

            string ipAddress = IPCount.GetVisitorIPAddress(context);

            // Check if allowed standalone
            if (!AllowStandalone && context.Request.UrlReferrer == null && !context.Request.IsLocal)
            {
                string message = "Not allowed to use standalone";
                if (LogSecurity)
                {
                    EventLogController logController = new EventLogController();
                    var logInfo = new LogInfo
                    {
                        LogUserID = PortalSettings.Current.UserId,
                        LogPortalID = PortalSettings.Current.PortalId,
                        LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString()
                    };
                    logInfo.AddProperty("DnnImageHandler", message);
                    logInfo.AddProperty("IP", ipAddress);
                    logController.AddLog(logInfo);
                }
                context.Response.StatusCode = 403;
                context.Response.StatusDescription = message;
                context.Response.End();
                return;
            }

            // Check if domain is allowed to embed image
            if (!string.IsNullOrEmpty(AllowedDomains[0]) &&
                context.Request.UrlReferrer != null &&
                context.Request.UrlReferrer.Host.ToLower() != context.Request.Url.Host.ToLower())
            {
                bool allowed = false;
                string allowedDomains = "";
                foreach (string allowedDomain in AllowedDomains)
                {
                    if (!string.IsNullOrEmpty(allowedDomain))
                    {
                        allowedDomains += allowedDomain + ",";
                        if (context.Request.UrlReferrer.Host.ToLower().Contains(allowedDomain.ToLower()))
                            allowed = true;
                    }
                }

                if (!allowed)
                {
                    string message = $"Not allowed to use from referrer '{context.Request.UrlReferrer.Host}'";
                    if (LogSecurity)
                    {
                        EventLogController logController = new EventLogController();
                        var logInfo = new LogInfo
                        {
                            LogUserID = PortalSettings.Current.UserId,
                            LogPortalID = PortalSettings.Current.PortalId,
                            LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString()
                        };
                        logInfo.AddProperty("DnnImageHandler", message);
                        logInfo.AddProperty("IP", ipAddress);
                        logInfo.AddProperty("AllowedDomains", allowedDomains);
                        logController.AddLog(logInfo);
                    }

                    context.Response.StatusCode = 403;
                    context.Response.StatusDescription = "Forbidden";
                    context.Response.End();
                    return;
                }
            }

            // Generate Image
            var imageMethodData = imageGenCallback(context.Request.QueryString);

            context.Response.ContentType = GetImageMimeType(ContentType);
            if (imageMethodData == null)
            {
                throw new InvalidOperationException("The DnnImageHandler cannot return null.");
            }
            if (imageMethodData.IsEmptyImage)
            {
                using (var imageOutputBuffer = new MemoryStream())
                {
                    RenderImage(imageMethodData.Image, imageOutputBuffer);
                    var buffer = imageOutputBuffer.GetBuffer();
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.End();
                    return;
                }
            }

            string cacheId = GetUniqueIDString(context, uniqueIdStringSeed);

            var cacheCleared = false;
            var profilepic = context.Request.QueryString["mode"];
            if (profilepic != null && profilepic == "profilepic")
            {
                if (int.TryParse(context.Request.QueryString["userId"], out int userId))
                    cacheCleared = ClearDiskImageCacheIfNecessary(userId, PortalSettings.Current.PortalId, cacheId);
            }
            // Handle client cache
            var cachePolicy = context.Response.Cache;
            cachePolicy.SetValidUntilExpires(true);
            if (EnableClientCache)
            {
                if (!string.IsNullOrEmpty(context.Request.Headers["If-Modified-Since"]) && !string.IsNullOrEmpty(context.Request.Headers["If-None-Match"]) && !cacheCleared)
                {
                    var provider = CultureInfo.InvariantCulture;
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
            
            // Handle Server cache
            if (EnableServerCache)
            {
                if (ImageStore.TryTransmitIfContains(cacheId, context.Response))
                {
                    context.Response.End();
                    return;
                }
            }

            // Check IP Cout boundaries
            if (EnableIPCount)
            {
                if (!IPCount.CheckIp(ipAddress))
                {
                    string message = "Too many requests";

                    if (LogSecurity)
                    {
                        EventLogController logController = new EventLogController();
                        var logInfo = new LogInfo
                        {
                            LogUserID = PortalSettings.Current.UserId,
                            LogPortalID = PortalSettings.Current.PortalId,
                            LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString()
                        };
                        logInfo.AddProperty("DnnImageHandler", message);
                        logInfo.AddProperty("IP", ipAddress);
                        logController.AddLog(logInfo);
                    }
                    context.Response.StatusCode = 429;
                    context.Response.StatusDescription = message;
                    context.Response.End();
                    return;
                }
            }
            
            if (imageMethodData.HttpStatusCode != null)
            {
                context.Response.StatusCode = (int)imageMethodData.HttpStatusCode;
                context.Response.End();
                return;
            }

            using (var imageOutputBuffer = new MemoryStream())
            {
                Debug.Assert(!(imageMethodData.Image == null && imageMethodData.ImageByteBuffer == null));
                if (imageMethodData.Image != null)
                {
                    RenderImage(GetImageThroughTransforms(imageMethodData.Image), imageOutputBuffer);
                }
                else if (imageMethodData.ImageByteBuffer != null)
                {
                    RenderImage(GetImageThroughTransforms(imageMethodData.ImageByteBuffer), imageOutputBuffer);
                }

                byte[] buffer = imageOutputBuffer.GetBuffer();

                context.Response.OutputStream.Write(buffer, 0, buffer.Length);

                if (EnableServerCache)
                {
                    ImageStore.Add(cacheId, buffer);
                }

                context.Response.End();
            }
        }

        private string GetUniqueIDString(HttpContextBase context, string uniqueIdStringSeed)
        {
            var builder = new StringBuilder();
            builder.Append(uniqueIdStringSeed);
            foreach (var key in context.Request.QueryString.AllKeys.OrderBy(k => k))
            {
                builder.Append(key);
                builder.Append(context.Request.QueryString.Get(key));
            }
            foreach (var tran in ImageTransforms)
            {
                builder.Append(tran.UniqueString);
            }

            return GetIDFromBytes(ASCIIEncoding.ASCII.GetBytes(builder.ToString()));
        }

        private static string GetIDFromBytes(byte[] buffer)
        {
            using (var hasher = SHA1.Create())
            {
                byte[] result = hasher.ComputeHash(buffer);
                var sb = new StringBuilder();
                foreach (var b in result)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private Image GetImageThroughTransforms(Image image)
        {
            try
            {
                Image temp = image;

                foreach (var tran in ImageTransforms)
                {
                    temp = tran.ProcessImage(temp);
                }
                return temp;
            }
            finally
            {
                image?.Dispose();
            }
        }

        //Clear the user image disk cache if userid is found in clear list and is within ClientCacheExpiration time.
        private bool ClearDiskImageCacheIfNecessary(int userId, int portalId, string cacheId)
        {
            var cacheKey = Constants.UserIdListToClearDiskImageCacheKey + portalId;
            Dictionary<int, DateTime> userIds;
            if ((userIds = DataCache.GetCache<Dictionary<int, DateTime>>(cacheKey)) == null || !userIds.ContainsKey(userId)) return false;
            ImageStore.ForcePurgeFromServerCache(cacheId);
            //The clear mechanism is performed for ClientCacheExpiration timespan so that all active clients clears the cache and don't see old data.
            if (!userIds.TryGetValue(userId, out DateTime expiry) || DateTime.UtcNow <= expiry.Add(ClientCacheExpiration)) return true;
            //Remove the userId from the clear list when timespan is > ClientCacheExpiration.
            userIds.Remove(userId);
            DataCache.SetCache(cacheKey, userIds);
            return true;
        }

        private Image GetImageThroughTransforms(byte[] buffer)
        {
            using (var memoryStream = new MemoryStream(buffer))
            {
                return GetImageThroughTransforms(Image.FromStream(memoryStream));
            }
        }

        private void RenderImage(Image image, Stream outStream)
        {
            try
            {
                if (ContentType == ImageFormat.Gif)
                {
                    var quantizer = new OctreeQuantizer(255, 8);
                    using (var quantized = quantizer.Quantize(image))
                    {
                        quantized.Save(outStream, ImageFormat.Gif);
                    }
                }
                else
                {
                    var eps = new EncoderParameters(1)
                    {
                        Param = {[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, ImageCompression)}
                    };
                    var ici = GetEncoderInfo(GetImageMimeType(ContentType));
                    image.Save(outStream, ici, eps);
                }
            }
            finally
            {
                image?.Dispose();
            }
        }

        /// <summary>
        /// Returns the encoder for the specified mime type
        /// </summary>
        /// <param name="mimeType">The mime type of the content</param>
        /// <returns>ImageCodecInfo</returns>
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            var encoders = ImageCodecInfo.GetImageEncoders();
            var e = encoders.FirstOrDefault(x => x.MimeType == mimeType);
            return e;
        }
    }
}
