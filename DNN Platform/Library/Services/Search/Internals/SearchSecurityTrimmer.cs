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
using System.Globalization;
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

        private string GetStringFromField(Document doc, SortField sortField)
        {
            var field = doc.GetField(sortField.Field);
            return field == null ? "" : field.StringValue;
        }

        private long GetLongFromField(Document doc, SortField sortField)
        {
            var field = doc.GetField(sortField.Field);
            if (field == null) return 0;

            long data;
            if (long.TryParse(field.StringValue, out data) && data >= 0) return data;
            
            return 0;
        }

        private void PrepareScoreDocs()
        {
            int skippedSoFar;
            var collectedSoFar = skippedSoFar = 0;
            var pageSize = _query.PageSize;
            var toSkip = _query.PageIndex <= 1 ? 0 : ((_query.PageIndex - 1) * pageSize);
            IEnumerable<ScoreDoc> tempDocs = new List<ScoreDoc>();

            _totalHits = _hitDocs.Count;

            var useRelevance = false;
            if (ReferenceEquals(Sort.RELEVANCE, _query.Sort))
            {
                useRelevance = true;
            }
            else
            {
                var fields = _query.Sort.GetSort();
                if (fields == null || fields.Count() != 1)
                {
                    useRelevance = true;
                }
                else
                {
                    var field = fields[0];
                    if (field.Type == SortField.INT || field.Type == SortField.LONG)
                    {
                        if(field.Reverse)
                            tempDocs = _hitDocs.Select(d => new { SDoc = d, Document = _searcher.Doc(d.Doc) })
                                       .OrderByDescending(rec => GetLongFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        else
                            tempDocs = _hitDocs.Select(d => new { SDoc = d, Document = _searcher.Doc(d.Doc) })
                                       .OrderBy(rec => GetLongFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                    }
                    else
                    {
                        if (field.Reverse)
                            tempDocs = _hitDocs.Select(d => new {SDoc = d, Document = _searcher.Doc(d.Doc)})
                                           .OrderByDescending(rec => GetStringFromField(rec.Document, field))
                                           .ThenByDescending(rec => rec.Document.Boost)
                                           .Select(rec => rec.SDoc);
                        else
                            tempDocs = _hitDocs.Select(d => new { SDoc = d, Document = _searcher.Doc(d.Doc) })
                                       .OrderBy(rec => GetStringFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                    }
                }
            }

            if (useRelevance)
                tempDocs = _hitDocs.OrderByDescending(d => d.Score).ThenBy(d => d.Doc);

            var scoreDocSize = Math.Min(tempDocs.Count(), pageSize);
            _scoreDocs = new List<ScoreDoc>(scoreDocSize);

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
