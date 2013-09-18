#region Copyright
// 
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
#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Search.Entities
{
    /// <summary>
    /// Criteria to search for.
    /// </summary>
    /// <remarks>This object should be passed to SearchController to search for Content. KeyWords and PortalId must be specified</remarks>
    [Serializable]
    public class SearchQuery
    {
        /// <summary>
        /// A key to uniquely identify a document in the Index
        /// </summary>
        public string UniqueKey { get; set; }

        /// <summary>
        /// Keywords to search for.
        /// </summary>
        public string KeyWords { get; set; }

        /// <summary>
        /// A collection of Portal Ids of the Site to perform Search upon. This field must be specified or else Portal 0 will be searched by default.
        /// </summary>
        /// <remarks>Search cannot be executed across Sites</remarks>
        public IEnumerable<int> PortalIds { get; set; }

        /// <summary>
        /// A collection of Search Type Ids that should be searched upon [Optional]
        /// </summary>
        public IEnumerable<int> SearchTypeIds { get; set; }

        /// <summary>
        /// A collection of Module Def Ids that should be searched upon [Optional]. Match is performed only when a SearchTypeId for Module Search Crawler Id
        /// </summary>
        public IEnumerable<int> ModuleDefIds { get; set; }

        /// <summary>
        /// Module Id to restrict Search. Value > 0 is used only.
        /// </summary>
        public int ModuleId { get; set; }

        /// <summary>
        /// Role Id to restrict Search. Value > 0 is used only.
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Tab Id to restrict Search. Value > 0 is used only.
        /// </summary>
        public int TabId { get; set; }

        /// <summary>
        /// Locale to restrict Search to. This field can be left empty for single language site
        /// </summary>
        /// <remarks>E.g. A value en-US or nl-NL can specified to restrict search to a single locale .</remarks> 
        public string Locale { get; set; }
        
        /// <summary>
        /// Begin Date of the time when Content was last modified (in Utc). This field is optional.
        /// </summary>
        public DateTime BeginModifiedTimeUtc { get; set; }

        /// <summary>
        /// End Date of the time when Content was last modified (in Utc). This field is optional.
        /// </summary>
        public DateTime EndModifiedTimeUtc { get; set; }

        /// <summary>
        /// Restrict search to specific tags. This field is optional.
        /// </summary>
        public IEnumerable<string> Tags { get; set; }

        /// <summary>
        /// Page Index for the result, e.g. pageIndex=1 and pageSize=10 indicates first 10 hits. Default value is 1
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size of the search result. Default value is 10.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Maximum length of highlighted title field in the results
        /// </summary>
        public int TitleSnippetLength { get; set; }

        /// <summary>
        /// Maximum length of highlighted title field in the results
        /// </summary>
        public int BodySnippetLength { get; set; }

        /// <summary>
        /// Culture Code associated with the content.
        /// </summary>
        /// <remarks>Culture-Neutral content is always returned even though this value is specfied</remarks>
        public string CultureCode { get; set; }

        /// <summary>
        /// Sort option of the search result. This field is optional.
        /// </summary>
        public SortFields SortField { get; set; }

        /// <summary>
        /// Sort option of the search result. This field is optional.
        /// </summary>
        public SortDirections SortDirection { get; set; }

        /// <summary>
        /// Name of the custom sort field, works with SortFields.CustomNumericField or SortFields.CustomStringField option.
        /// </summary>
        /// <remarks> Enum SortFields can be used to sort on Relevance, LastModified and Title. Additional fields such as one provided under
        /// SearchDocument.Keywords, SearchDocument.NumericKeys or Tags can be specified.
        /// can be specified by using CustomSortField property. One needs to ensure that the field name is a valid one.
        /// </remarks>
        /// <example>authorid or authorname</example>
        public string CustomSortField { get; set; }

        /// <summary>
        /// Set this to true to perform perform WildCard Search.
        /// </summary>
        /// <remarks>This property is not respected when Keywords contain special boolean phrases "~", "*", "\"", "\'", "and", "or", "+", "-".
        /// When this is enabled, an additional OR is performed, e.g. (keyword OR keyword*). It adds asterisk at then end to find any words starting with the keyword.
        /// There can be performance implications with this setting turned on.</remarks>
        public bool WildCardSearch { get; set; }

        #region constructor

        public SearchQuery()
        {
            Tags = new string[0];
            PortalIds = new int[0];
            SearchTypeIds = new int[0];
            ModuleDefIds = new int[0];
            TitleSnippetLength = 60;
            BodySnippetLength = 100;
            PageSize = 10;
            PageIndex = 1;
        }

        #endregion 
    }
}