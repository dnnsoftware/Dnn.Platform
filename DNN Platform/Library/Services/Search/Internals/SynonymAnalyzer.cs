// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;

    /// <summary>
    /// This is responsible for the filters chain that analyzes search documents/queries.
    /// </summary>
    internal class SynonymAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var stops = GetStopWords();
            var wordLengthMinMax = SearchHelper.Instance.GetSearchMinMaxLength();

            // Note: the order of filtering is important for both operation and performane, so we try to make it work faster
            // Also, note that filters are applied from the innermost outwards.
            // According to Lucene's documentaiton the StopFilter performs a case-sensitive lookup of each token in a set of stop
            // words. It relies on being fed already lowercased tokens. Therefore, DO NOT reverse the order of these filters.
            return
                new PorterStemFilter(// stemming filter
                    new ASCIIFoldingFilter(// accents filter
                        new SynonymFilter(
                            new StopFilter(
                                true,
                                new LowerCaseFilter(
                                    new LengthFilter(
                                        new StandardFilter(
                                            new StandardTokenizer(Constants.LuceneVersion, reader)),
                                        wordLengthMinMax.Item1, wordLengthMinMax.Item2)),
                                stops))))
            ;
        }

        private static ISet<string> GetStopWords()
        {
            int portalId;
            string cultureCode;

            var searchDoc = Thread.GetData(Thread.GetNamedDataSlot(Constants.TlsSearchInfo)) as SearchDocument;
            if (searchDoc == null)
            {
                portalId = 0; // default
                cultureCode = Thread.CurrentThread.CurrentCulture.Name;
            }
            else
            {
                portalId = searchDoc.PortalId;
                cultureCode = searchDoc.CultureCode;
                if (string.IsNullOrEmpty(cultureCode))
                {
                    var portalInfo = PortalController.Instance.GetPortal(portalId);
                    if (portalInfo != null)
                    {
                        cultureCode = portalInfo.DefaultLanguage;
                    }
                }
            }

            var stops = StopAnalyzer.ENGLISH_STOP_WORDS_SET;
            var searchStopWords = SearchHelper.Instance.GetSearchStopWords(portalId, cultureCode);

            if (searchStopWords != null && !string.IsNullOrEmpty(searchStopWords.StopWords))
            {
                // TODO Use cache from InternalSearchController
                var cultureInfo = new CultureInfo(cultureCode ?? "en-US");
                var strArray = searchStopWords.StopWords.Split(',').Select(s => s.ToLower(cultureInfo)).ToArray();
                var set = new CharArraySet(strArray.Length, false);
                set.AddAll(strArray);
                stops = CharArraySet.UnmodifiableSet(set);
            }

            return stops;
        }
    }
}
