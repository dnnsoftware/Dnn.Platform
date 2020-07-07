// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;

    using DotNetNuke.Framework;

    /// <summary>
    /// Class responsible to parse the Search Query String parameter.
    /// </summary>
    public class SearchQueryStringParser
                            : ServiceLocator<ISearchQueryStringParser, SearchQueryStringParser>,
                            ISearchQueryStringParser
    {
        private static readonly Regex TagRegex = new Regex(@"\[(.*?)\]", RegexOptions.Compiled);

        private static readonly Regex DateRegex = new Regex(@"after:(\w+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TypeRegex = new Regex(@"type:([^,]+(,[^,]+)*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Gets the list of tags parsing the search keywords.
        /// </summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the tags.</param>
        /// <returns>List of tags.</returns>
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
                        tags.Add(HttpUtility.HtmlEncode(tag.Trim()));
                    }

                    m = m.NextMatch();
                }

                outputKeywords = TagRegex.Replace(keywords, string.Empty).Trim();
            }

            return tags;
        }

        /// <summary>
        /// Gets the Last Modified Date parsing the search keywords.
        /// </summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the last modified date.</param>
        /// <returns>Last Modified Date.</returns>
        public DateTime GetLastModifiedDate(string keywords, out string outputKeywords)
        {
            var m = DateRegex.Match(keywords);
            var date = string.Empty;
            while (m.Success && string.IsNullOrEmpty(date))
            {
                date = m.Groups[1].ToString();
            }

            var result = DateTime.MinValue;
            if (!string.IsNullOrEmpty(date))
            {
                switch (date.ToLowerInvariant())
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
        /// Gets the list of Search Types parsing the search keywords.
        /// </summary>
        /// <param name="keywords">search keywords.</param>
        /// <param name="outputKeywords">output keywords removing the Search Type.</param>
        /// <returns>List of Search Types.</returns>
        public IList<string> GetSearchTypeList(string keywords, out string outputKeywords)
        {
            var m = TypeRegex.Match(keywords);
            var types = string.Empty;
            while (m.Success && string.IsNullOrEmpty(types))
            {
                types = m.Groups[1].ToString().Trim();
            }

            var typesList = new List<string>();
            if (!string.IsNullOrEmpty(types))
            {
                typesList = types.Split(',').ToList();
            }

            outputKeywords = TypeRegex.Replace(keywords, string.Empty).Trim();
            return typesList;
        }

        protected override Func<ISearchQueryStringParser> GetFactory()
        {
            return () => new SearchQueryStringParser();
        }
    }
}
