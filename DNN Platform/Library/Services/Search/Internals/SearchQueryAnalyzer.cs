// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.IO;

    using DotNetNuke.Entities.Controllers;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Analysis.Core;
    using Lucene.Net.Analysis.En;
    using Lucene.Net.Analysis.Miscellaneous;

    /// <summary>
    /// This is responsible for the filters chain that analyzes search documents/queries.
    /// </summary>
    internal class SearchQueryAnalyzer : Analyzer
    {
        private readonly bool _useStemmingFilter;

        public SearchQueryAnalyzer(bool useStemmingFilter)
        {
            this._useStemmingFilter = useStemmingFilter;
        }

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            var wordLengthMinMax = SearchHelper.Instance.GetSearchMinMaxLength();

            // Note: the order of filtering is important for both operation and performane, so we try to make it work faster
            // Also, note that filters are applied from the innermost outwards.
            var tokenizer = new StandardTokenizer(Constants.LuceneVersion, reader);

            var standardFilter = new StandardFilter(Constants.LuceneVersion, tokenizer);
            var lengthFilter = new LengthFilter(Constants.LuceneVersion, standardFilter, wordLengthMinMax.Item1, wordLengthMinMax.Item2);
            var lowerCaseFilter = new LowerCaseFilter(Constants.LuceneVersion, lengthFilter);
            var filter = new ASCIIFoldingFilter(lowerCaseFilter);

            if (!this._useStemmingFilter)
            {
                return new TokenStreamComponents(tokenizer, filter);
            }

            return new TokenStreamComponents(tokenizer, new PorterStemFilter(filter));
        }
    }
}
