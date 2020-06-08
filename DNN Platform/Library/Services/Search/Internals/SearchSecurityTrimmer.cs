// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotNetNuke.Services.Search.Entities;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    internal delegate bool SecurityCheckerDelegate(Document luceneResult, SearchQuery searchQuery);

    internal class SearchSecurityTrimmer : Collector
    {
        private readonly SecurityCheckerDelegate _securityChecker;
        private readonly IndexSearcher _searcher;

        private Scorer _scorer;
        private int _docBase, _totalHits;
        private readonly LuceneQuery _luceneQuery;
        private readonly SearchQuery _searchQuery;
        private readonly List<ScoreDoc> _hitDocs;
        private List<ScoreDoc> _scoreDocs;

        public SearchSecurityTrimmer(SearchSecurityTrimmerContext searchContext)
        {
            _securityChecker = searchContext.SecurityChecker;
            _searcher = searchContext.Searcher;
            _luceneQuery = searchContext.LuceneQuery;
            _searchQuery = searchContext.SearchQuery;
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
            var pageSize = _luceneQuery.PageSize;
            var toSkip = _luceneQuery.PageIndex <= 1 ? 0 : ((_luceneQuery.PageIndex - 1) * pageSize);
            IEnumerable<ScoreDoc> tempDocs = new List<ScoreDoc>();

            _totalHits = _hitDocs.Count;

            var useRelevance = false;
            if (ReferenceEquals(Sort.RELEVANCE, _luceneQuery.Sort))
            {
                useRelevance = true;
            }
            else
            {
                var fields = _luceneQuery.Sort.GetSort();
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
                if (_securityChecker == null || _securityChecker(_searcher.Doc(scoreDoc.Doc), _searchQuery))
                {
                    if (skippedSoFar < toSkip)
                    {
                        skippedSoFar++;
                        continue;
                    }

                    if (collectedSoFar >= pageSize)
                    {
                        continue;
                    }

                    _scoreDocs.Add(scoreDoc);
                    ++collectedSoFar;
                }
                else
                {
                    _totalHits--;
                }
            }
        }
    }
}
