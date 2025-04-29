// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals;

using System.IO;

using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

/// <summary>This is responsible for the filters chain that analyzes search documents/queries.</summary>
internal class SearchQueryAnalyzer : Analyzer
{
    private readonly bool useStemmingFilter;

    /// <summary>Initializes a new instance of the <see cref="SearchQueryAnalyzer"/> class.</summary>
    /// <param name="useStemmingFilter"></param>
    public SearchQueryAnalyzer(bool useStemmingFilter)
    {
        this.useStemmingFilter = useStemmingFilter;
    }

    /// <inheritdoc/>
    public override TokenStream TokenStream(string fieldName, TextReader reader)
    {
        var wordLengthMinMax = SearchHelper.Instance.GetSearchMinMaxLength();

        // Note: the order of filtering is important for both operation and performance, so we try to make it work faster
        // Also, note that filters are applied from the innermost outwards.
        var filter =
            new ASCIIFoldingFilter(
                new LowerCaseFilter(
                    new LengthFilter(
                        new StandardFilter(
                            new StandardTokenizer(Constants.LuceneVersion, reader)),
                        wordLengthMinMax.Item1,
                        wordLengthMinMax.Item2)));

        if (!this.useStemmingFilter)
        {
            return filter;
        }

        return new PorterStemFilter(filter);
    }
}
