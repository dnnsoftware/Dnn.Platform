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