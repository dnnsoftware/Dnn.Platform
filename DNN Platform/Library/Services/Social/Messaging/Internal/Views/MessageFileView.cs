﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    /// <summary>The MessageFileView class contains details about the attachment</summary>
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging.Views
    /// Class:      MessageFileView
    /// -----------------------------------------------------------------------------
    /// -----------------------------------------------------------------------------
    public class MessageFileView
    {
        /// <summary>The _size</summary>
        private string _size;

        /// <summary>Gets or sets the file identifier.</summary>
        /// <value>The file identifier.</value>
        public int FileId { get; set; }

        /// <summary>Gets or sets the name of the file with extension</summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>Gets or sets the size of the File with Unit, e.g. 100 B, 12 KB, 200 MB, etc.</summary>
        /// <value>The size.</value>
        public string Size
        {
            get { return _size; }
            set
            {
                long bytes;
                if (!long.TryParse(value, out bytes)) return;
                const int scale = 1024;
                var orders = new[] { "GB", "MB", "KB", "B" };
                var max = (long)Math.Pow(scale, orders.Length - 1);

                foreach (var order in orders)
                {
                    if (bytes > max)
                    {
                        _size = string.Format("{0:##.##} {1}", decimal.Divide(bytes, max), order);
                        return;
                    }

                    max /= scale;
                }
                _size = "0 B";
            }
        }

        /// <summary>
        /// Gets or sets the url of the file to download
        /// </summary>
        public string Url { get; set; }
    }
}
