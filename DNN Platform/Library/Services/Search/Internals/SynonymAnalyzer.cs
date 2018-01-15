#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Search.Entities;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// This is responsible for the filters chain that analyzes search documents/queries
    /// </summary>
    internal class SynonymAnalyzer : Analyzer
    {
        public override TokenStream TokenStream(string fieldName, TextReader reader)
        {
            var stops = GetStopWords();
            var wordLengthMinMax = SearchHelper.Instance.GetSearchMinMaxLength();

            //Note: the order of filtering is important for both operation and performane, so we try to make it work faster
            // Also, note that filters are applied from the innermost outwards.
            // According to Lucene's documentaiton the StopFilter performs a case-sensitive lookup of each token in a set of stop
            // words. It relies on being fed already lowercased tokens. Therefore, DO NOT reverse the order of these filters.
            return
                new PorterStemFilter( // stemming filter
                    new ASCIIFoldingFilter( // accents filter
                        new SynonymFilter(
                            new StopFilter(true,
                                new LowerCaseFilter(
                                    new LengthFilter(
                                        new StandardFilter(
                                            new StandardTokenizer(Constants.LuceneVersion, reader)
                                        )
                                        , wordLengthMinMax.Item1, wordLengthMinMax.Item2)
                                )
                            , stops)
                        )
                    )
                )
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
                        cultureCode = portalInfo.DefaultLanguage;
                }
            }

            var stops = StopAnalyzer.ENGLISH_STOP_WORDS_SET;
            var searchStopWords = SearchHelper.Instance.GetSearchStopWords(portalId, cultureCode);

            if (searchStopWords != null && !string.IsNullOrEmpty(searchStopWords.StopWords))
            {
                //TODO Use cache from InternalSearchController
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