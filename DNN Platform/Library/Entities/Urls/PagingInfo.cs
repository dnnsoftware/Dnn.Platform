// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Urls
{
    /// <summary>
    /// Class used as a utility to help manage paging in database queries
    /// </summary>
    public class PagingInfo
    {
        public PagingInfo(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public int PageNumber { get; private set; }

        public int PageSize { get; private set; }

        public int FirstRow { get; private set; }

        public int LastRow { get; private set; }

        public int TotalRows { get; private set; }

        public int TotalPages { get; private set; }

        public bool IsLastPage
        {
            get
            {
                if (LastRow >= (TotalRows))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void UpdatePageResults(int firstRow, int lastRow, int totalRows, int totalPages)
        {
            FirstRow = firstRow;
            LastRow = lastRow;
            TotalRows = totalRows;
            TotalPages = totalPages;
        }
    }
}
