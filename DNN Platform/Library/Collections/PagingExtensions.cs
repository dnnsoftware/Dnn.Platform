﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;

#endregion

namespace DotNetNuke.Collections
{
    /// <summary>
    ///   Contains filters that can be applied to <see cref = "IEnumerable" /> stores
    /// </summary>
    public static class PagingExtensions
    {
        #region Public Extension Methods

        /// <summary>
        ///   Filters the incoming store to retrieve pages of a specified size.
        /// </summary>
        /// <typeparam name = "T">The type of the object being filtered</typeparam>
        /// <param name = "source">The source object being filtered</param>
        /// <param name = "pageSize">The page size to use</param>
        /// <returns>
        ///   A <see cref = "PageSelector{T}" /> object that is used to select a single
        ///   page of data from the data source.
        /// </returns>
        public static PageSelector<T> InPagesOf<T>(this IEnumerable<T> source, int pageSize)
        {
            return new PageSelector<T>(source, pageSize);
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> into an <see cref="IPagedList{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the items in the <paramref name="source"/></typeparam> 
        /// <param name = "source">The source <see cref="IEnumerable{T}"/> to convert</param>
        /// <param name = "pageIndex">The page index requested</param>
        /// <param name = "pageSize">The page size requested</param>
        /// <returns>A <see cref="IPagedList{T}"/> object that is used to select a single 
        /// page of data from the data source</returns>
        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize)
        {
            return new PagedList<T>(source, pageIndex, pageSize);
        }

        #endregion
    }
}
