// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Collections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Taken from Rob Conery's Blog post on the ASP.Net MVC PagedList Helper
    // http://blog.wekeroad.com/2007/12/10/aspnet-mvc-pagedlistt/

    /// <summary>
    ///   Represents a snapshot of a single page of objects from a data store.
    /// </summary>
    /// <typeparam name = "T">The type of objects contained in this list.</typeparam>
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        ///  Initializes a new instance of the <see cref="PagedList{T}"/> a paged list containing objects from the selected enumerable source.
        /// </summary>
        /// <param name = "source">The <see cref = "IEnumerable{T}" /> data store containing objects to be retrieved.</param>
        /// <param name = "pageIndex">The index of the page to retrieve.</param>
        /// <param name = "pageSize">The size of the page to retrieve.</param>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var enumerable = source as T[] ?? source.ToArray();
            this.CommonConstruct(enumerable.Skip(pageIndex * pageSize).Take(pageSize).ToList(), enumerable.Count(), pageIndex, pageSize);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        ///  Initializes a new instance of the <see cref="PagedList{T}"/> a paged list containing objects from the selected enumerable source.
        /// </summary>
        /// <param name="items">The items that constitute the page.</param>
        /// <param name="totalCount">The total number of items in the original source.</param>
        /// <param name = "pageIndex">The index of the page to retrieve.</param>
        /// <param name = "pageSize">The size of the page to retrieve.</param>
        public PagedList(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
            this.CommonConstruct(items, totalCount, pageIndex, pageSize);
        }

        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if there is a next page available.
        /// </summary>
        public bool HasNextPage { get; private set; }

        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if there is a previous page available.
        /// </summary>
        public bool HasPreviousPage { get; private set; }

        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if this is the first page.
        /// </summary>
        public bool IsFirstPage { get; private set; }

        /// <summary>
        ///   Gets a value indicating whether gets a boolean indicating if this is the last page.
        /// </summary>
        public bool IsLastPage { get; private set; }

        /// <summary>
        ///   Gets or sets the number of pages in this list.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        ///   Gets or sets the index of the page contained in this list.
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        ///   Gets or sets the size of the page in this list.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///   Gets or sets or ses the total number of objects in the data store.
        /// </summary>
        public int TotalCount { get; set; }

        private void CommonConstruct(IEnumerable<T> items, int totalCount, int pageIndex, int pageSize)
        {
            this.PageCount = (int)Math.Ceiling(totalCount / (double)pageSize);

            if (this.PageCount == 0)
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

                if (pageIndex >= this.PageCount)
                {
                    throw new IndexOutOfRangeException("Invalid Page Index");
                }
            }

            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.AddRange(items);

            this.HasNextPage = this.PageIndex < (this.PageCount - 1);
            this.HasPreviousPage = this.PageIndex > 0;
            this.IsFirstPage = this.PageIndex <= 0;
            this.IsLastPage = this.PageIndex >= (this.PageCount - 1);
        }
    }
}
