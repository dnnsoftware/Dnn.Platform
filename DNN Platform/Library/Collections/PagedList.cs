#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace DotNetNuke.Collections
{
    // Taken from Rob Conery's Blog post on the ASP.Net MVC PagedList Helper
    // http://blog.wekeroad.com/2007/12/10/aspnet-mvc-pagedlistt/

    /// <summary>
    ///   Represents a snapshot of a single page of objects from a data store
    /// </summary>
    /// <typeparam name = "T">The type of objects contained in this list</typeparam>
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        #region Constructors

        /// <summary>
        ///  Initializes a new instance of the <see cref="PagedList{T}"/> a paged list containing objects from the selected enumerable source
        /// </summary>
        /// <param name = "source">The <see cref = "IEnumerable{T}" /> data store containing objects to be retrieved</param>
        /// <param name = "pageIndex">The index of the page to retrieve</param>
        /// <param name = "pageSize">The size of the page to retrieve</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var enumerable = source as T[] ?? source.ToArray();
            CommonConstruct(enumerable.Skip(pageIndex * pageSize).Take(pageSize).ToList(), enumerable.Count(), pageIndex, pageSize);
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="PagedList{T}"/> a paged list containing objects from the selected enumerable source
        /// </summary>
        /// <param name="items">The items that constitute the page</param>
        /// <param name="totalCount">The total number of items in the original source</param>
        /// <param name = "pageIndex">The index of the page to retrieve</param>
        /// <param name = "pageSize">The size of the page to retrieve</param>
        public PagedList(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
            CommonConstruct(items, totalCount, pageIndex, pageSize);
        }

        #endregion

        #region Private Methods

        private void CommonConstruct(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
            PageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (PageCount == 0)
            {
                if (pageIndex > 0)
                {
                    throw new IndexOutOfRangeException("Invalid Page Index");
                }
            }
            else
            {
                if (pageIndex < 0)
                {
                    throw new IndexOutOfRangeException("Index cannot be negative");
                }
                if (pageIndex >= PageCount)
                {
                    throw new IndexOutOfRangeException("Invalid Page Index");
                }
            }
            TotalCount = totalCount;
            PageSize = pageSize;
            PageIndex = pageIndex;
            AddRange(items);

            HasNextPage = (PageIndex < (PageCount - 1));
            HasPreviousPage = (PageIndex > 0);
            IsFirstPage = (PageIndex <= 0);
            IsLastPage = (PageIndex >= (PageCount - 1));
        }

        #endregion

        #region IPagedList<T> Members

        /// <summary>
        ///   Gets a boolean indicating if there is a next page available
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        ///   Gets a boolean indicating if there is a previous page available
        /// </summary>
        public bool HasPreviousPage { get; private set; }

        /// <summary>
        ///   Gets a boolean indicating if this is the first page
        /// </summary>
        public bool IsFirstPage { get; private set; }

        /// <summary>
        ///   Gets a boolean indicating if this is the last page
        /// </summary>
        public bool IsLastPage { get; private set; }

        /// <summary>
        ///   Gets or sets the number of pages in this list 
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        ///   Gets or sets the index of the page contained in this list
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        ///   Gets or sets the size of the page in this list 
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///   Gets or ses the total number of objects in the data store 
        /// </summary>
        public int TotalCount { get; set; }

        #endregion
    }
}
