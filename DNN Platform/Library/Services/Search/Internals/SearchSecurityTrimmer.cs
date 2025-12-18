// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Documents;
    using Lucene.Net.Index;
    using Lucene.Net.Search;

    internal delegate bool SecurityCheckerDelegate(Document luceneResult, SearchQuery searchQuery);

    internal class SearchSecurityTrimmer : Collector
    {
        private readonly SecurityCheckerDelegate securityChecker;
        private readonly IndexSearcher searcher;
        private readonly LuceneQuery luceneQuery;
        private readonly SearchQuery searchQuery;
        private readonly List<ScoreDoc> hitDocs;

        private Scorer scorer;
        private int docBase;
        private int totalHits;
        private List<ScoreDoc> scoreDocs;

        /// <summary>Initializes a new instance of the <see cref="SearchSecurityTrimmer"/> class.</summary>
        /// <param name="searchContext">The search context.</param>
        public SearchSecurityTrimmer(SearchSecurityTrimmerContext searchContext)
        {
            this.securityChecker = searchContext.SecurityChecker;
            this.searcher = searchContext.Searcher;
            this.luceneQuery = searchContext.LuceneQuery;
            this.searchQuery = searchContext.SearchQuery;
            this.hitDocs = new List<ScoreDoc>(16);
        }

        /// <inheritdoc/>
        public override bool AcceptsDocsOutOfOrder
        {
            get { return false; }
        }

        public int TotalHits
        {
            get
            {
                if (this.scoreDocs == null)
                {
                    this.PrepareScoreDocs();
                }

                return this.totalHits;
            }
        }

        public List<ScoreDoc> ScoreDocs
        {
            get
            {
                if (this.scoreDocs == null)
                {
                    this.PrepareScoreDocs();
                }

                return this.scoreDocs;
            }
        }

        /// <inheritdoc/>
        public override void SetNextReader(IndexReader reader, int docBase)
        {
            this.docBase = docBase;
        }

        /// <inheritdoc/>
        public override void SetScorer(Scorer scorer)
        {
            this.scorer = scorer;
        }

        /// <inheritdoc/>
        public override void Collect(int doc)
        {
            this.hitDocs.Add(new ScoreDoc(doc + this.docBase, this.scorer.Score()));
        }

        private string GetStringFromField(Document doc, SortField sortField)
        {
            var field = doc.GetField(sortField.Field);
            return field == null ? string.Empty : field.StringValue;
        }

        private long GetLongFromField(Document doc, SortField sortField)
        {
            var field = doc.GetField(sortField.Field);
            if (field == null)
            {
                return 0;
            }

            long data;
            if (long.TryParse(field.StringValue, out data) && data >= 0)
            {
                return data;
            }

            return 0;
        }

        private void PrepareScoreDocs()
        {
            int skippedSoFar;
            var collectedSoFar = skippedSoFar = 0;
            var pageSize = this.luceneQuery.PageSize;
            var toSkip = this.luceneQuery.PageIndex <= 1 ? 0 : ((this.luceneQuery.PageIndex - 1) * pageSize);
            IEnumerable<ScoreDoc> tempDocs = new List<ScoreDoc>();

            this.totalHits = this.hitDocs.Count;

            var useRelevance = false;
            if (ReferenceEquals(Sort.RELEVANCE, this.luceneQuery.Sort))
            {
                useRelevance = true;
            }
            else
            {
                var fields = this.luceneQuery.Sort.GetSort();
                if (fields == null || fields.Count() != 1)
                {
                    useRelevance = true;
                }
                else
                {
                    var field = fields[0];
                    if (field.Type == SortField.INT || field.Type == SortField.LONG)
                    {
                        if (field.Reverse)
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                       .OrderByDescending(rec => this.GetLongFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                        else
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                       .OrderBy(rec => this.GetLongFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                    }
                    else
                    {
                        if (field.Reverse)
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                           .OrderByDescending(rec => this.GetStringFromField(rec.Document, field))
                                           .ThenByDescending(rec => rec.Document.Boost)
                                           .Select(rec => rec.SDoc);
                        }
                        else
                        {
                            tempDocs = this.hitDocs.Select(d => new { SDoc = d, Document = this.searcher.Doc(d.Doc) })
                                       .OrderBy(rec => this.GetStringFromField(rec.Document, field))
                                       .ThenByDescending(rec => rec.Document.Boost)
                                       .Select(rec => rec.SDoc);
                        }
                    }
                }
            }

            if (useRelevance)
            {
                tempDocs = this.hitDocs.OrderByDescending(d => d.Score).ThenBy(d => d.Doc);
            }

            var scoreDocSize = Math.Min(tempDocs.Count(), pageSize);
            this.scoreDocs = new List<ScoreDoc>(scoreDocSize);

            foreach (var scoreDoc in tempDocs)
            {
                if (this.securityChecker == null || this.securityChecker(this.searcher.Doc(scoreDoc.Doc), this.searchQuery))
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

                    this.scoreDocs.Add(scoreDoc);
                    ++collectedSoFar;
                }
                else
                {
                    this.totalHits--;
                }
            }
        }
    }
}
