﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;

#endregion

namespace DotNetNuke.Collections
{
    // Taken from Rob Conery's Blog post on the ASP.Net MVC PagedList Helper
    // http://blog.wekeroad.com/2007/12/10/aspnet-mvc-pagedlistt/

    /// <summary>
    ///   Provides an interface to a paged list, which contains a snapshot
    ///   of a single page of data from the data store
    /// </summary>
    /// <typeparam name = "T">The type of objects stored in the list</typeparam>
    public interface IPagedList<T> : IList<T>
    {
        /// <summary>
        ///   Gets a boolean indicating if there is a next page available
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        ///   Gets a boolean indicating if there is a previous page available
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        ///   Gets a boolean indicating if this is the first page
        /// </summary>
        bool IsFirstPage { get; }

        /// <summary>
        ///   Gets a boolean indicating if this is the last page
        /// </summary>
        bool IsLastPage { get; }

        /// <summary>
        ///   The no of pages in this list
        /// </summary>
        int PageCount { get; set; }

        /// <summary>
        ///   The index of the page contained in this list
        /// </summary>
        int PageIndex { get; set; }

        /// <summary>
        ///   The size of the page in this list
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        ///   The total number of objects in the data store
        /// </summary>
        int TotalCount { get; set; }
    }
}
