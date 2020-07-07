// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Dto.Taxonomy;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;

    using Util = DotNetNuke.Entities.Content.Common.Util;

    public class VocabularyService : BasePortableService
    {
        public override string Category => Constants.Category_Vocabularies;

        public override string ParentCategory => null;

        public override uint Priority => 5;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();
            List<TaxonomyVocabularyType> vocabularyTypes = null;

            if (this.CheckPoint.Stage == 0)
            {
                var taxonomyTerms = GetTaxonomyTerms(exportDto.PortalId, toDate, fromDate);
                var taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                if (taxonomyTerms.Count > 0 || taxonomyVocabularies.Count > 0)
                {
                    var scopeTypes = CBO.FillCollection<TaxonomyScopeType>(DataProvider.Instance().GetAllScopeTypes());

                    // Update the total items count in the check points. This should be updated only once.
                    this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? scopeTypes.Count : this.CheckPoint.TotalItems;
                    if (this.CheckPoint.TotalItems == scopeTypes.Count)
                    {
                        vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
                        taxonomyTerms = GetTaxonomyTerms(exportDto.PortalId, toDate, fromDate);
                        taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                        this.CheckPoint.TotalItems += taxonomyTerms.Count + taxonomyVocabularies.Count;
                    }

                    this.CheckPointStageCallback(this);

                    this.Repository.CreateItems(scopeTypes);

                    // Result.AddSummary("Exported Taxonomy Scopes", scopeTypes.Count.ToString()); -- not imported so don't show
                    // CheckPoint.ProcessedItems += scopeTypes.Count;
                }

                this.CheckPoint.Progress = 25;

                if (taxonomyVocabularies == null)
                {
                    taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                }

                if (taxonomyTerms.Count > 0 || taxonomyVocabularies.Count > 0)
                {
                    if (vocabularyTypes == null)
                    {
                        vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
                    }

                    this.Repository.CreateItems(vocabularyTypes);

                    // Result.AddSummary("Exported Vocabulary Types", vocabularyTypes.Count.ToString()); -- not imported so don't show
                    // CheckPoint.ProcessedItems += vocabularyTypes.Count;
                }

                this.Repository.CreateItems(taxonomyTerms);
                this.Result.AddSummary("Exported Vocabularies", taxonomyTerms.Count.ToString());
                this.CheckPoint.Progress = 75;
                this.CheckPoint.ProcessedItems += taxonomyTerms.Count;
                this.CheckPoint.Stage++;
                if (this.CheckPointStageCallback(this))
                {
                    return;
                }

                if (taxonomyVocabularies == null)
                {
                    taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                }

                this.Repository.CreateItems(taxonomyVocabularies);
                this.Result.AddSummary("Exported Terms", taxonomyVocabularies.Count.ToString());
                this.CheckPoint.Progress = 100;
                this.CheckPoint.ProcessedItems += taxonomyVocabularies.Count;
                this.CheckPoint.Stage++;
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
            }
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(importJob))
            {
                return;
            }

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? this.GetImportTotal() : this.CheckPoint.TotalItems;
            if (this.CheckPoint.Stage == 0)
            {
                var otherScopeTypes = this.Repository.GetAllItems<TaxonomyScopeType>().ToList();

                // the table Taxonomy_ScopeTypes is used for lookup only and never changed/updated in the database
                // CheckPoint.Progress = 10;

                // var otherVocabularyTypes = Repository.GetAllItems<TaxonomyVocabularyType>().ToList();
                // the table Taxonomy_VocabularyTypes is used for lookup only and never changed/updated in the database
                this.CheckPoint.Progress = 20;

                var otherVocabularies = this.Repository.GetAllItems<TaxonomyVocabulary>().ToList();
                this.ProcessVocabularies(importJob, importDto, otherScopeTypes, otherVocabularies);
                this.Repository.UpdateItems(otherVocabularies);
                this.Result.AddSummary("Imported Vocabularies", otherVocabularies.Count.ToString());
                this.CheckPoint.Progress = 60;
                this.CheckPoint.ProcessedItems += otherVocabularies.Count;

                var otherTaxonomyTerms = this.Repository.GetAllItems<TaxonomyTerm>().ToList();
                this.ProcessTaxonomyTerms(importJob, importDto, otherVocabularies, otherTaxonomyTerms);
                this.Repository.UpdateItems(otherTaxonomyTerms);
                this.Result.AddSummary("Imported Terms", otherTaxonomyTerms.Count.ToString());
                this.CheckPoint.Progress = 100;
                this.CheckPoint.ProcessedItems += otherTaxonomyTerms.Count;
                this.CheckPoint.Stage++;
                this.CheckPoint.Completed = true;
                this.CheckPointStageCallback(this);
            }
        }

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<TaxonomyVocabulary>() + this.Repository.GetCount<TaxonomyTerm>();
        }

        private static List<TaxonomyTerm> GetTaxonomyTerms(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms(portalId, toDate, fromDate));
        }

        private static List<TaxonomyVocabulary> GetTaxonomyVocabularies(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies(portalId, toDate, fromDate));
        }

        private void ProcessVocabularies(ExportImportJob importJob, ImportDto importDto,
            IList<TaxonomyScopeType> otherScopeTypes, IEnumerable<TaxonomyVocabulary> otherVocabularies)
        {
            var changed = false;
            var dataService = Util.GetDataService();
            var localVocabularies = GetTaxonomyVocabularies(importDto.PortalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null);
            foreach (var other in otherVocabularies)
            {
                var createdBy = Common.Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var local = localVocabularies.FirstOrDefault(t => t.Name == other.Name);
                var scope = otherScopeTypes.FirstOrDefault(s => s.ScopeTypeID == other.ScopeTypeID);

                var scopeId = other.ScopeID ?? Null.NullInteger;
                if (scope != null && scope.ScopeType.Equals("Application", StringComparison.InvariantCultureIgnoreCase))
                {
                    scopeId = Null.NullInteger;
                }
                else if (scope != null && scope.ScopeType.Equals("Portal", StringComparison.InvariantCultureIgnoreCase))
                {
                    scopeId = importDto.PortalId;
                }

                if (local != null)
                {
                    other.LocalId = local.VocabularyID;
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored vocabulary", other.Name);
                            break;
                        case CollisionResolution.Overwrite:
                            var vocabulary = new Vocabulary(other.Name, other.Description)
                            {
                                IsSystem = other.IsSystem,
                                Weight = other.Weight,
                                ScopeId = scopeId,
                                ScopeTypeId = scope?.LocalId ?? other.ScopeTypeID,
                            };
                            dataService.UpdateVocabulary(vocabulary, modifiedBy);
                            this.Result.AddLogEntry("Updated vocabulary", other.Name);
                            changed = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var vocabulary = new Vocabulary(other.Name, other.Description, (VocabularyType)other.VocabularyTypeID)
                    {
                        IsSystem = other.IsSystem,
                        Weight = other.Weight,
                        ScopeId = scopeId,
                        ScopeTypeId = scope?.LocalId ?? other.ScopeTypeID,
                    };
                    other.LocalId = dataService.AddVocabulary(vocabulary, createdBy);
                    this.Result.AddLogEntry("Added vocabulary", other.Name);
                    changed = true;
                }
            }

            if (changed)
            {
                DataCache.ClearCache(DataCache.VocabularyCacheKey);
            }
        }

        private void ProcessTaxonomyTerms(ExportImportJob importJob, ImportDto importDto,
            IList<TaxonomyVocabulary> otherVocabularies, IList<TaxonomyTerm> otherTaxonomyTerms)
        {
            var dataService = Util.GetDataService();

            // var vocabularyController = new VocabularyController();
            var localTaxonomyTerms = GetTaxonomyTerms(importDto.PortalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null);
            foreach (var other in otherTaxonomyTerms)
            {
                var createdBy = Common.Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);

                var vocabulary = otherVocabularies.FirstOrDefault(v => v.VocabularyID == other.VocabularyID);
                var vocabularyId = vocabulary?.LocalId ?? 0;
                var isHierarchical = vocabulary != null && vocabulary.VocabularyTypeID == (int)VocabularyType.Hierarchy;
                var local = localTaxonomyTerms.FirstOrDefault(t => t.Name == other.Name && t.VocabularyID == vocabularyId);

                if (local != null)
                {
                    other.LocalId = local.TermID;
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored taxonomy", other.Name);
                            break;
                        case CollisionResolution.Overwrite:
                            var parent = other.ParentTermID.HasValue
                                ? otherTaxonomyTerms.FirstOrDefault(v => v.TermID == other.ParentTermID.Value)
                                : null;
                            var term = new Term(other.Name, other.Description, vocabularyId)
                            {
                                TermId = local.TermID,
                                ParentTermId = parent?.LocalId,
                                Weight = other.Weight,
                            };

                            if (isHierarchical)
                            {
                                dataService.UpdateHeirarchicalTerm(term, modifiedBy);
                            }
                            else
                            {
                                dataService.UpdateSimpleTerm(term, modifiedBy);
                            }

                            DataCache.ClearCache(string.Format(DataCache.TermCacheKey, term.TermId));
                            this.Result.AddLogEntry("Updated taxonomy", other.Name);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var parent = other.ParentTermID.HasValue
                        ? otherTaxonomyTerms.FirstOrDefault(v => v.TermID == other.ParentTermID.Value)
                        : null;
                    var term = new Term(other.Name, other.Description, vocabularyId)
                    {
                        ParentTermId = parent?.LocalId,
                        Weight = other.Weight,
                    };

                    other.LocalId = isHierarchical
                        ? dataService.AddHeirarchicalTerm(term, createdBy)
                        : dataService.AddSimpleTerm(term, createdBy);
                    this.Result.AddLogEntry("Added taxonomy", other.Name);
                }
            }
        }
    }
}
