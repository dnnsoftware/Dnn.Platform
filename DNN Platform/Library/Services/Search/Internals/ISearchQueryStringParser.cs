// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;

    public interface ISearchQueryStringParser
    {
        /// <summary>
        /// Gets the list of tags parsing the search keywords.
        /// </summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the tags.</param>
        /// <returns>List of tags.</returns>
        IList<string> GetTags(string keywords, out string outputKeywords);

        /// <summary>
        /// Gets the Last Modified Date parsing the search keywords.
        /// </summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the last modified date.</param>
        /// <returns>Last Modified Date.</returns>
        DateTime GetLastModifiedDate(string keywords, out string outputKeywords);

        /// <summary>
        /// Gets the list of Search Types parsing the search keywords.
        /// </summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the Search Type.</param>
        /// <returns>List of Search Types.</returns>
        IList<string> GetSearchTypeList(string keywords, out string outputKeywords);
    }
}
