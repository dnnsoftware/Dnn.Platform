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
using System.Linq;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    internal delegate bool SecurityCheckerDelegate(Document luceneResult);

    internal class SearchSecurityTrimmer : Collector
    {
        private readonly SecurityCheckerDelegate _securityChecker;
        private readonly IndexSearcher _searcher;

        private Scorer _scorer;
        private int _docBase, _totalHits;
        private readonly LuceneQuery _query;
        private readonly List<ScoreDoc> _hitDocs;
        private List<ScoreDoc> _scoreDocs;

        public SearchSecurityTrimmer(IndexSearcher searcher, SecurityCheckerDelegate securityChecker, LuceneQuery luceneQuery)
        {
            _securityChecker = securityChecker;
            _searcher = searcher;
            _query = luceneQuery;
            _hitDocs = new List<ScoreDoc>(16);
        }

        public override bool AcceptsDocsOutOfOrder { get { return false; } }

        public override void SetNextReader(IndexReader reader, int docBase)
        {
            _docBase = docBase;
        }

        public override void SetScorer(Scorer scorer)
        {
            _scorer = scorer;
        }

        public override void Collect(int doc)
        {
            _hitDocs.Add(new ScoreDoc(doc + _docBase, _scorer.Score()));
        }

        public int TotalHits
        {
            get
            {
                if (_scoreDocs == null) PrepareScoreDocs();
                return _totalHits;
            }
        }

        public List<ScoreDoc> ScoreDocs
        {
            get
            {
                if (_scoreDocs == null) PrepareScoreDocs();
                return _scoreDocs;
            }
        }

        private void PrepareScoreDocs()
        {
            int skippedSoFar;
            var collectedSoFar = skippedSoFar = 0;
            var pageSize = _query.PageSize;
            var toSkip = _query.PageIndex <= 1 ? 0 : ((_query.PageIndex - 1) * pageSize);
            _scoreDocs = new List<ScoreDoc>(pageSize);
            IEnumerable<ScoreDoc> tempDocs;

            _totalHits = _hitDocs.Count;
 
            if (ReferenceEquals(Sort.RELEVANCE, _query.Sort))
            {
                tempDocs = _hitDocs.OrderByDescending(d => d.Score).ThenBy(d => d.Doc);
            }
            else
            {
                //HACK: order by document date descending based on date's "StringValue" as below
                //      and we don't take into consideration all of the Sort fields; only the first
                tempDocs = _hitDocs.Select(d => new { SDoc = d, Document = _searcher.Doc(d.Doc) })
                    .OrderByDescending(rec => rec.Document.GetField(Constants.ModifiedTimeTag).StringValue)
                    .ThenBy(rec => rec.Document.Boost)
                    .Select(rec => rec.SDoc);
            }

           foreach (var scoreDoc in tempDocs)
            {
                if (_securityChecker == null || _securityChecker(_searcher.Doc(scoreDoc.Doc)))
                {
                    if (skippedSoFar < toSkip)
                    {
                        skippedSoFar++;
                        continue;
                    }

                    _scoreDocs.Add(scoreDoc);
                    if (++collectedSoFar == pageSize)
                    {
                        break;
                    }
                }
                else
                {
                    _totalHits--;
                }
            }
        }
    }
}
