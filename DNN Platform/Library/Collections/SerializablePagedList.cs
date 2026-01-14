// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections
{
    using System.Collections.Generic;

    /// <summary>
    /// This class facilitates the serialization of IPagedList results to the client.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    public class SerializablePagedList<T>
    {
        /// <summary>
        /// Gets or sets the data contained in the page.
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        /// <summary>
        /// Gets or sets the index of the current page.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Gets or sets the size of each page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the first page in the list.
        /// </summary>
        public bool IsFirstPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the last page in the list.
        /// </summary>
        public bool IsLastPage { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of items in the list.
        /// </summary>
        public int TotalCount { get; set; }
    }
}
