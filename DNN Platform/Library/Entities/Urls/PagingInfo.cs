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