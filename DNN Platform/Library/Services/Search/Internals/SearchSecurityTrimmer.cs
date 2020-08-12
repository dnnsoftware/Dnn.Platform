// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Common.Utilities;

    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;

    internal delegate bool SecurityCheckerDelegate(Document luceneResult, SearchQuery searchQuery);

    internal class SearchSecurityTrimmer : ICollector
    {
        private readonly SecurityCheckerDelegate _securityChecker;
        private readonly IndexSearcher _searcher;
        private readonly LuceneQuery _luceneQuery;
        private readonly SearchQuery _searchQuery;
        private readonly List<ScoreDoc> _hitDocs;

        private Scorer _scorer;
        private int _docBase;
        private int _totalHits;
        private List<ScoreDoc> _scoreDocs;

        private ICollector _baseCollector;

        public SearchSecurityTrimmer(SearchSecurityTrimmerContext searchContext)
        {
            this._securityChecker = searchContext.SecurityChecker;
            this._searcher = searchContext.Searcher;
            this._luceneQuery = searchContext.LuceneQuery;
            this._searchQuery = searchContext.SearchQuery;
            this._hitDocs = new List<ScoreDoc>(16);

            _baseCollector = Collector.NewAnonymous(
                (scorer) => this._scorer = scorer, 
                (doc) => this._hitDocs.Add(new ScoreDoc(doc + this._docBase, this._scorer.GetScore())), 
                (context) => this._docBase = context.DocBase, 
                () => false);
        }

        public void SetScorer(Scorer scorer)
        {
            _baseCollector.SetScorer(scorer);
        }

        public void Collect(int doc)
        {
            _baseCollector.Collect(doc);
        }

        public void SetNextReader(AtomicReaderContext context)
        {
            _baseCollector.SetNextReader(context);
        }

        public bool AcceptsDocsOutOfOrder
        {
            get { return _baseCollector.AcceptsDocsOutOfOrder; }
        }

        public int TotalHits
        {
            get
            {
                if (this._scoreDocs == null)
                {
                    this.PrepareScoreDocs();
                }

                return this._totalHits;
            }
        }

        public List<ScoreDoc> ScoreDocs
        {
            get
            {
                if (this._scoreDocs == null)
                {
                    this.PrepareScoreDocs();
                }

                return this._scoreDocs;
            }
        }

        

        private string GetStringFromField(Document doc, SortField sortField)
        {
            var field = doc.GetField(sortField.Field);
            return field == null ? string.Empty : field.GetStringValue();
        }

        private long GetLongFromField(Document doc, SortField sortField)
        {
            var value = doc.GetField(sortField.Field)?.GetInt64Value() ?? Null.NullInteger;
            return value > 0 ? value : 0;
        }

        private void PrepareScoreDocs()
        {
            int skippedSoFar;
            var collectedSoFar = skippedSoFar = 0;
            var pageSize = this._luceneQuery.PageSize;
            var toSkip = this._luceneQuery.PageIndex <= 1 ? 0 : ((this._luceneQuery.PageIndex - 1) * pageSize);
            IEnumerable<ScoreDoc> tempDocs = new List<ScoreDoc>();

            this._totalHits = this._hitDocs.Count;

            var useRelevance = false;
            if (ReferenceEquals(Sort.RELEVANCE, this._luceneQuery.Sort))
            {
                useRelevance = true;
            }
            else
            {
                var fields = this._luceneQuery.Sort.GetSort();
                if (fields == null || fields.Count() != 1)
                {
                    useRelevance = true;
                }
                else
                {
                    var field = fields[0];
                    if (field.Type == SortFieldType.INT32 || field.Type == SortFieldType.INT64)
                    {
                        if (field.IsReverse)
                        {
                            tempDocs = this._hitDocs.Select(d => new { SDoc = d, Document = this._searcher.Doc(d.Doc) })
                                       .OrderByDescending(rec => this.GetLongFromField(rec.Document, field))
                                       //.ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                        else
                        {
                            tempDocs = this._hitDocs.Select(d => new { SDoc = d, Document = this._searcher.Doc(d.Doc) })
                                       .OrderBy(rec => this.GetLongFromField(rec.Document, field))
                                       //.ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                    }
                    else
                    {
                        if (field.IsReverse)
                        {
                            tempDocs = this._hitDocs.Select(d => new { SDoc = d, Document = this._searcher.Doc(d.Doc) })
                                           .OrderByDescending(rec => this.GetStringFromField(rec.Document, field))
                                           //.ThenByDescending(rec => rec.Document.Boost)
                                           .Select(rec => rec.SDoc);
                        }
                        else
                        {
                            tempDocs = this._hitDocs.Select(d => new { SDoc = d, Document = this._searcher.Doc(d.Doc) })
                                       .OrderBy(rec => this.GetStringFromField(rec.Document, field))
                                       //.ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                    }
                }
            }

            if (useRelevance)
            {
                tempDocs = this._hitDocs.OrderByDescending(d => d.Score).ThenBy(d => d.Doc);
            }

            var scoreDocSize = Math.Min(tempDocs.Count(), pageSize);
            this._scoreDocs = new List<ScoreDoc>(scoreDocSize);

            foreach (var scoreDoc in tempDocs)
            {
                if (this._securityChecker == null || this._securityChecker(this._searcher.Doc(scoreDoc.Doc), this._searchQuery))
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

                    this._scoreDocs.Add(scoreDoc);
                    ++collectedSoFar;
                }
                else
                {
                    this._totalHits--;
                }
            }
        }
    }
}
