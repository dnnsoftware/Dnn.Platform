using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Web;

namespace DotNetNuke.Services.GeneratedImage
{
    /// <summary>
    /// Image Handler abstract class
    /// </summary>
    public abstract class ImageHandler : IHttpHandler
    {
        private ImageHandlerInternal Implementation { get; set; }

        /// <summary>
        /// Enables server-side caching of the result
        /// </summary>
        public bool EnableServerCache
        {
            get { return Implementation.EnableServerCache; }
            set { Implementation.EnableServerCache = value; }
        }

        /// <summary>
        /// Enables client-side caching of the result
        /// </summary>
        public bool EnableClientCache
        {
            get { return Implementation.EnableClientCache; }
            set { Implementation.EnableClientCache = value; }
        }

        /// <summary>
        /// Sets the client-side cache expiration time
        /// </summary>
        public TimeSpan ClientCacheExpiration
        {
            get { return Implementation.ClientCacheExpiration; }
            set { Implementation.ClientCacheExpiration = value; }
        }

        /// <summary>
        /// List of Domains who are allowed to use the imagehandler when security is enabled
        /// </summary>
        public string[] AllowedDomains
        {
            get { return Implementation.AllowedDomains; }
            set { Implementation.AllowedDomains = value; }
        }

        public bool AllowStandalone
        {
            get { return Implementation.AllowStandalone; }
            set { Implementation.AllowStandalone = value; }
        }

        public bool LogSecurity
        {
            get { return Implementation.LogSecurity; }
            set { Implementation.LogSecurity = value; }
        }

        /// <summary>
        /// Sets the type of the result image. The handler will return ouput with MIME type matching this content
        /// </summary>
        public ImageFormat ContentType
        {
            get { return Implementation.ContentType; }
            set { Implementation.ContentType = value; }
        }

        /// <summary>
        /// Sets the image compression encoding for the result image. Default is 50L
        /// </summary>
        public long ImageCompression
        {
            get { return Implementation.ImageCompression; }
            set { Implementation.ImageCompression = value; }
        }

        /// <summary>
        /// Enables block mechanism for DDOS by referring IP
        /// </summary>
        public bool EnableIPCount
        {
            get { return Implementation.EnableIPCount; }
            set { Implementation.EnableIPCount = value; }
        }
        
        /// <summary>
        /// Sets the maximum amount of images an IP address is allowed to generate 
        /// in the defined purge interval
        /// </summary>
        public int IPCountMaxCount
        {
            get { return Implementation.IPCountMax; }
            set { Implementation.IPCountMax = value; }
        }

        /// <summary>
        /// Timespan for resetting the blocking 
        /// </summary>
        public TimeSpan IPCountPurgeInterval
        {
            get { return Implementation.IpCountPurgeInterval; }
            set { Implementation.IpCountPurgeInterval = value; }
        }

        /// <summary>
        /// A list of image transforms that will be applied successively to the image
        /// </summary>
        protected List<ImageTransform> ImageTransforms => Implementation.ImageTransforms;

        protected ImageHandler()
            : this(new ImageHandlerInternal())
        {
        }

        private ImageHandler(ImageHandlerInternal implementation)
        {
            Implementation = implementation;
        }

        internal ImageHandler(IImageStore imageStore, DateTime now)
            : this(new ImageHandlerInternal(imageStore, now))
        {
        }

        public abstract ImageInfo GenerateImage(NameValueCollection parameters);

        public virtual bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            HttpContextBase contextWrapper = new HttpContextWrapper(context);
            ProcessRequest(contextWrapper);
        }

        internal void ProcessRequest(HttpContextBase context)
        {
            Debug.Assert(context != null);
            Implementation.HandleImageRequest(context, GenerateImage, ToString());
        }
    }
}
