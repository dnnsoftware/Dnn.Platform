// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;

using DotNetNuke.Entities.Controllers;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// This is responsible for the filters chain that analyzes search documents/queries
    /// </summary>
    internal class SearchQueryAnalyzer : Analyzer
    {
        private readonly bool _useStemmingFilter;

        public SearchQueryAnalyzer(bool useStemmingFilter)
        {
            _useStemmingFilter = useStemmingFilter;
        }

        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var wordLengthMinMax = SearchHelper.Instance.GetSearchMinMaxLength();

            //Note: the order of filtering is important for both operation and performane, so we try to make it work faster
            // Also, note that filters are applied from the innermost outwards.
            var filter =
                    new ASCIIFoldingFilter( // accents filter
                        new LowerCaseFilter(
                            new LengthFilter(
                                new StandardFilter(
                                    new StandardTokenizer(Constants.LuceneVersion, reader)
                                )
                            , wordLengthMinMax.Item1, wordLengthMinMax.Item2)
                        )
                    );

            if (!_useStemmingFilter)
                return filter;

            return new PorterStemFilter(filter);
        }
    }
}
