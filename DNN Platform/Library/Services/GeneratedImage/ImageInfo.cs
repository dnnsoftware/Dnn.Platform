// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Drawing;
using System.Net;

namespace DotNetNuke.Services.GeneratedImage
{
    /// <summary>
    /// Image info class
    /// </summary>
    public class ImageInfo
    {
        /// <summary>
        /// Image
        /// </summary>
        public Image Image { get; private set; }

        /// <summary>
        /// Image byte buffer
        /// </summary>
        public byte[] ImageByteBuffer { get; private set; }

        /// <summary>
        /// Http status code
        /// </summary>
        public HttpStatusCode? HttpStatusCode { get; private set; }

        public bool IsEmptyImage { get; set; } = false;

        public ImageInfo(HttpStatusCode statusCode)
        {
            this.HttpStatusCode = statusCode;
        }

        public ImageInfo(Image image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            this.Image = image;
        }

        public ImageInfo(byte[] imageBuffer)
        {
            if (imageBuffer == null)
            {
                throw new ArgumentNullException(nameof(imageBuffer));
            }

            this.ImageByteBuffer = imageBuffer;
        }
    }
}
