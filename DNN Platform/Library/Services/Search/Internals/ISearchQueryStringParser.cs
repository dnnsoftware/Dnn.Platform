#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;

namespace DotNetNuke.Services.Search.Internals
{
    public interface ISearchQueryStringParser
    {
        /// <summary>
        /// Gets the list of tags parsing the search keywords
        /// </summary>
        /// <param name="keywords">search keywords</param>
        /// <param name="outputKeywords">output keywords removing the tags</param>
        /// <returns>List of tags</returns>
        IList<string> GetTags(string keywords, out string outputKeywords);

        /// <summary>
        /// Gets the Last Modified Date parsing the search keywords
        /// </summary>
        /// <param name="keywords">search keywords</param>
        /// <param name="outputKeywords">output keywords removing the last modified date</param>
        /// <returns>Last Modified Date</returns>
        DateTime GetLastModifiedDate(string keywords, out string outputKeywords);

        /// <summary>
        /// Gets the list of Search Types parsing the search keywords
        /// </summary>
        /// <param name="keywords">search keywords</param>
        /// <param name="outputKeywords">output keywords removing the Search Type</param>
        /// <returns>List of Search Types</returns>
        IList<string> GetSearchTypeList(string keywords, out string outputKeywords);
    }
}
