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
