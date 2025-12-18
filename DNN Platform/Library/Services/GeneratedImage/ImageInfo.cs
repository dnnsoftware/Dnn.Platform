// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage
{
    using System;
    using System.Drawing;
    using System.Net;

    /// <summary>Image info class.</summary>
    public class ImageInfo
    {
        /// <summary>Initializes a new instance of the <see cref="ImageInfo"/> class.</summary>
        /// <param name="statusCode">The HTTP status code.</param>
        public ImageInfo(HttpStatusCode statusCode)
        {
            this.HttpStatusCode = statusCode;
        }

        /// <summary>Initializes a new instance of the <see cref="ImageInfo"/> class.</summary>
        /// <param name="image">The image.</param>
        public ImageInfo(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            this.Image = image;
        }

        /// <summary>Initializes a new instance of the <see cref="ImageInfo"/> class.</summary>
        /// <param name="imageBuffer">An array of bytes representing the contents of the image.</param>
        public ImageInfo(byte[] imageBuffer)
        {
            if (imageBuffer == null)
            {
                throw new ArgumentNullException(nameof(imageBuffer));
            }

            this.ImageByteBuffer = imageBuffer;
        }

        /// <summary>Gets image.</summary>
        public Image Image { get; private set; }

        /// <summary>Gets image byte buffer.</summary>
        public byte[] ImageByteBuffer { get; private set; }

        /// <summary>Gets http status code.</summary>
        public HttpStatusCode? HttpStatusCode { get; private set; }

        public bool IsEmptyImage { get; set; } = false;
    }
}
