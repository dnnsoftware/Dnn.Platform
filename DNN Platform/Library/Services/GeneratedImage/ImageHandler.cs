// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Drawing.Imaging;
    using System.Web;

    /// <summary>
    /// Image Handler abstract class.
    /// </summary>
    public abstract class ImageHandler : IHttpHandler
    {
        internal ImageHandler(IImageStore imageStore, DateTime now)
            : this(new ImageHandlerInternal(imageStore, now))
        {
        }

        protected ImageHandler()
            : this(new ImageHandlerInternal())
        {
        }

        private ImageHandler(ImageHandlerInternal implementation)
        {
            this.Implementation = implementation;
        }

        public virtual bool IsReusable => false;

        /// <summary>
        /// Gets or sets a value indicating whether enables server-side caching of the result.
        /// </summary>
        public bool EnableServerCache
        {
            get { return this.Implementation.EnableServerCache; }
            set { this.Implementation.EnableServerCache = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether enables client-side caching of the result.
        /// </summary>
        public bool EnableClientCache
        {
            get { return this.Implementation.EnableClientCache; }
            set { this.Implementation.EnableClientCache = value; }
        }

        /// <summary>
        /// Gets or sets the client-side cache expiration time.
        /// </summary>
        public TimeSpan ClientCacheExpiration
        {
            get { return this.Implementation.ClientCacheExpiration; }
            set { this.Implementation.ClientCacheExpiration = value; }
        }

        /// <summary>
        /// Gets or sets list of Domains who are allowed to use the imagehandler when security is enabled.
        /// </summary>
        public string[] AllowedDomains
        {
            get { return this.Implementation.AllowedDomains; }
            set { this.Implementation.AllowedDomains = value; }
        }

        public bool AllowStandalone
        {
            get { return this.Implementation.AllowStandalone; }
            set { this.Implementation.AllowStandalone = value; }
        }

        public bool LogSecurity
        {
            get { return this.Implementation.LogSecurity; }
            set { this.Implementation.LogSecurity = value; }
        }

        /// <summary>
        /// Gets or sets the type of the result image. The handler will return ouput with MIME type matching this content.
        /// </summary>
        public ImageFormat ContentType
        {
            get { return this.Implementation.ContentType; }
            set { this.Implementation.ContentType = value; }
        }

        /// <summary>
        /// Gets or sets the image compression encoding for the result image. Default is 50L.
        /// </summary>
        public long ImageCompression
        {
            get { return this.Implementation.ImageCompression; }
            set { this.Implementation.ImageCompression = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether enables block mechanism for DDOS by referring IP.
        /// </summary>
        public bool EnableIPCount
        {
            get { return this.Implementation.EnableIPCount; }
            set { this.Implementation.EnableIPCount = value; }
        }

        /// <summary>
        /// Gets or sets the maximum amount of images an IP address is allowed to generate
        /// in the defined purge interval.
        /// </summary>
        public int IPCountMaxCount
        {
            get { return this.Implementation.IPCountMax; }
            set { this.Implementation.IPCountMax = value; }
        }

        /// <summary>
        /// Gets or sets timespan for resetting the blocking.
        /// </summary>
        public TimeSpan IPCountPurgeInterval
        {
            get { return this.Implementation.IpCountPurgeInterval; }
            set { this.Implementation.IpCountPurgeInterval = value; }
        }

        /// <summary>
        /// Gets a list of image transforms that will be applied successively to the image.
        /// </summary>
        protected List<ImageTransform> ImageTransforms => this.Implementation.ImageTransforms;

        private ImageHandlerInternal Implementation { get; set; }

        public abstract ImageInfo GenerateImage(NameValueCollection parameters);

        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpContextBase contextWrapper = new HttpContextWrapper(context);
            this.ProcessRequest(contextWrapper);
        }

        internal void ProcessRequest(HttpContextBase context)
        {
            Debug.Assert(context != null);
            this.Implementation.HandleImageRequest(context, this.GenerateImage, this.ToString());
        }
    }
}
