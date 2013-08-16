using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Collections
{
    /// <summary>
    ///   Provides options to allow the consumer to select a page of data from a paged data store
    /// </summary>
    /// <typeparam name = "T">The type of object in the data store</typeparam>
    public class PageSelector<T>
    {
        private readonly int _pageSize;
        private readonly IEnumerable<T> _source;

        #region Constructors

        /// <summary>
        /// Constructs a new PageSelector for use on the specified data store
        /// </summary>
        /// <param name = "source">The data store to page</param>
        /// <param name = "pageSize">The size of each page</param>
        public PageSelector(IEnumerable<T> source, int pageSize)
        {
            _source = source;
            _pageSize = pageSize;
        }

        #endregion

        #region Public Methods

        /// <summary>
        ///   Retrieves the specified page as a <see cref = "IPagedList{T}" />
        /// </summary>
        /// <param name = "pageIndex">The index (zero-based) of the page to retrieve</param>
        /// <returns>
        /// An <see cref = "IPagedList{T}" /> containing the page of data, or an 
        /// empty list if the page does not exist
        /// </returns>
        public IPagedList<T> GetPage(int pageIndex)
        {
            return new PagedList<T>(_source, pageIndex, _pageSize);
        }

        #endregion
    }
}
