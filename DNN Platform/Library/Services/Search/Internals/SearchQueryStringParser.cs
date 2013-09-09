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
using System.Linq;
using System.Text.RegularExpressions;
using DotNetNuke.Framework;

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// Class responsible to parse the Search Query String parameter
    /// </summary>
    public class SearchQueryStringParser
                            : ServiceLocator<ISearchQueryStringParser, SearchQueryStringParser>
                            , ISearchQueryStringParser
    {
        protected override Func<ISearchQueryStringParser> GetFactory()
        {
            return () => new SearchQueryStringParser();
        }

        private static readonly Regex TagRegex = new Regex(@"\[(.*?)\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DateRegex = new Regex(@"after:(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TypeRegex = new Regex(@"type:(\w+(,\w+)*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Gets the list of tags parsing the search keywords
        /// </summary>
        /// <param name="keywords">search keywords</param>
        /// <param name="outputKeywords">output keywords removing the tags</param>
        /// <returns>List of tags</returns>
        public IList<string> GetTags(string keywords, out string outputKeywords)
        {
            var tags = new List<string>();
            if (string.IsNullOrEmpty(keywords))
            {
                outputKeywords = string.Empty;
            }
            else
            {
                var m = TagRegex.Match(keywords);
                while (m.Success)
                {
                    var tag = m.Groups[1].ToString();
                    if (!string.IsNullOrEmpty(tag))
                    {
                        tags.Add(tag.Trim());
                    }
                    m = m.NextMatch();
                }

                outputKeywords = TagRegex.Replace(keywords, string.Empty).Trim();
            }
            return tags;
        }

        /// <summary>
        /// Gets the Last Modified Date parsing the search keywords
        /// </summary>
        /// <param name="keywords">search keywords</param>
        /// <param name="outputKeywords">output keywords removing the last modified date</param>
        /// <returns>Last Modified Date</returns>
        public DateTime GetLastModifiedDate(string keywords, out string outputKeywords)
        {
            var m = DateRegex.Match(keywords);
            var date = "";
            while (m.Success && string.IsNullOrEmpty(date))
            {
                date = m.Groups[1].ToString();
            }

            var result = DateTime.MinValue;
            if (!string.IsNullOrEmpty(date))
            {
                switch (date.ToLower())
                {
                    case "day":
                        result = DateTime.UtcNow.AddDays(-1);
                        break;
                    case "week":
                        result = DateTime.UtcNow.AddDays(-7);
                        break;
                    case "month":
                        result = DateTime.UtcNow.AddMonths(-1);
                        break;
                    case "quarter":
                        result = DateTime.UtcNow.AddMonths(-3);
                        break;
                    case "year":
                        result = DateTime.UtcNow.AddYears(-1);
                        break;
                }
            }

            outputKeywords = DateRegex.Replace(keywords, string.Empty).Trim();
            return result;
        }

        /// <summary>
        /// Gets the list of Search Types parsing the search keywords
        /// </summary>
        /// <param name="keywords">search keywords</param>
        /// <param name="outputKeywords">output keywords removing the Search Type</param>
        /// <returns>List of Search Types</returns>
        public IList<string> GetSearchTypeList(string keywords, out string outputKeywords)
        {
            var m = TypeRegex.Match(keywords);
            var types = "";
            while (m.Success && string.IsNullOrEmpty(types))
            {
                types = m.Groups[1].ToString();
            }

            var typesList = new List<string>();
            if (!string.IsNullOrEmpty(types))
            {
                typesList = types.Split(',').ToList();
            }

            outputKeywords = TypeRegex.Replace(keywords, string.Empty).Trim();
            return typesList;
        }
    }
}
