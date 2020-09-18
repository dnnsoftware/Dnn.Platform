// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections
{
    using System.Collections.Generic;

    // Taken from Rob Conery's Blog post on the ASP.Net MVC PagedList Helper
    // http://blog.wekeroad.com/2007/12/10/aspnet-mvc-pagedlistt/

    /// <summary>
    ///   Provides an interface to a paged list, which contains a snapshot
    ///   of a single page of data from the data store.
    /// </summary>
    /// <typeparam name = "T">The type of objects stored in the list.</typeparam>
    public interface IPagedList<T> : IList<T>
    {
        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if there is a next page available.
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if there is a previous page available.
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if this is the first page.
        /// </summary>
        bool IsFirstPage { get; }

        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if this is the last page.
        /// </summary>
        bool IsLastPage { get; }

        /// <summary>
        ///   Gets or sets the no of pages in this list.
        /// </summary>
        int PageCount { get; set; }

        /// <summary>
        ///   Gets or sets the index of the page contained in this list.
        /// </summary>
        int PageIndex { get; set; }

        /// <summary>
        ///   Gets or sets the size of the page in this list.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        ///   Gets or sets the total number of objects in the data store.
        /// </summary>
        int TotalCount { get; set; }
    }
}
