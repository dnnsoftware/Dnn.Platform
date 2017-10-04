#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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
