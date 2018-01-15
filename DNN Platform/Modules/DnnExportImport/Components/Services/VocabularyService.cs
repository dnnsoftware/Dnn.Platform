#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
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

namespace Dnn.ExportImport.Components.Services
{
    public class VocabularyService : BasePortableService
    {
        public override string Category => Constants.Category_Vocabularies;

        public override string ParentCategory => null;

        public override uint Priority => 5;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(exportJob)) return;

            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();
            List<TaxonomyVocabularyType> vocabularyTypes = null;

            if (CheckPoint.Stage == 0)
            {
                var taxonomyTerms = GetTaxonomyTerms(exportDto.PortalId, toDate, fromDate);
                var taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                if (taxonomyTerms.Count > 0 || taxonomyVocabularies.Count > 0)
                {
                    var scopeTypes = CBO.FillCollection<TaxonomyScopeType>(DataProvider.Instance().GetAllScopeTypes());
                    //Update the total items count in the check points. This should be updated only once.
                    CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? scopeTypes.Count : CheckPoint.TotalItems;
                    if (CheckPoint.TotalItems == scopeTypes.Count)
                    {
                        vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
                        taxonomyTerms = GetTaxonomyTerms(exportDto.PortalId, toDate, fromDate);
                        taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                        CheckPoint.TotalItems += taxonomyTerms.Count + taxonomyVocabularies.Count;
                    }
                    CheckPointStageCallback(this);

                    Repository.CreateItems(scopeTypes);
                    //Result.AddSummary("Exported Taxonomy Scopes", scopeTypes.Count.ToString()); -- not imported so don't show
                    //CheckPoint.ProcessedItems += scopeTypes.Count;
                }
                CheckPoint.Progress = 25;

                if (taxonomyVocabularies == null) taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                if (taxonomyTerms.Count > 0 || taxonomyVocabularies.Count > 0)
                {
                    if (vocabularyTypes == null)
                        vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
                    Repository.CreateItems(vocabularyTypes);
                    //Result.AddSummary("Exported Vocabulary Types", vocabularyTypes.Count.ToString()); -- not imported so don't show
                    //CheckPoint.ProcessedItems += vocabularyTypes.Count;
                }

                Repository.CreateItems(taxonomyTerms);
                Result.AddSummary("Exported Vocabularies", taxonomyTerms.Count.ToString());
                CheckPoint.Progress = 75;
                CheckPoint.ProcessedItems += taxonomyTerms.Count;
                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;

                if (taxonomyVocabularies == null) taxonomyVocabularies = GetTaxonomyVocabularies(exportDto.PortalId, toDate, fromDate);
                Repository.CreateItems(taxonomyVocabularies);
                Result.AddSummary("Exported Terms", taxonomyVocabularies.Count.ToString());
                CheckPoint.Progress = 100;
                CheckPoint.ProcessedItems += taxonomyVocabularies.Count;
                CheckPoint.Stage++;
                CheckPoint.Completed = true;
                CheckPointStageCallback(this);
            }
        }

        private static List<TaxonomyTerm> GetTaxonomyTerms(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms(portalId, toDate, fromDate));
        }

        private static List<TaxonomyVocabulary> GetTaxonomyVocabularies(int portalId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies(portalId, toDate, fromDate));
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(importJob)) return;

            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? GetImportTotal() : CheckPoint.TotalItems;
            if (CheckPoint.Stage == 0)
            {
                var otherScopeTypes = Repository.GetAllItems<TaxonomyScopeType>().ToList();
                //the table Taxonomy_ScopeTypes is used for lookup only and never changed/updated in the database
                //CheckPoint.Progress = 10;

                //var otherVocabularyTypes = Repository.GetAllItems<TaxonomyVocabularyType>().ToList();
                //the table Taxonomy_VocabularyTypes is used for lookup only and never changed/updated in the database
                CheckPoint.Progress = 20;

                var otherVocabularies = Repository.GetAllItems<TaxonomyVocabulary>().ToList();
                ProcessVocabularies(importJob, importDto, otherScopeTypes, otherVocabularies);
                Repository.UpdateItems(otherVocabularies);
                Result.AddSummary("Imported Vocabularies", otherVocabularies.Count.ToString());
                CheckPoint.Progress = 60;
                CheckPoint.ProcessedItems += otherVocabularies.Count;

                var otherTaxonomyTerms = Repository.GetAllItems<TaxonomyTerm>().ToList();
                ProcessTaxonomyTerms(importJob, importDto, otherVocabularies, otherTaxonomyTerms);
                Repository.UpdateItems(otherTaxonomyTerms);
                Result.AddSummary("Imported Terms", otherTaxonomyTerms.Count.ToString());
                CheckPoint.Progress = 100;
                CheckPoint.ProcessedItems += otherTaxonomyTerms.Count;
                CheckPoint.Stage++;
                CheckPoint.Completed = true;
                CheckPointStageCallback(this);
            }
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<TaxonomyVocabulary>() + Repository.GetCount<TaxonomyTerm>();
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
                            Result.AddLogEntry("Ignored vocabulary", other.Name);
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
                            Result.AddLogEntry("Updated vocabulary", other.Name);
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
                    Result.AddLogEntry("Added vocabulary", other.Name);
                    changed = true;
                }
            }
            if (changed)
                DataCache.ClearCache(DataCache.VocabularyCacheKey);
        }

        private void ProcessTaxonomyTerms(ExportImportJob importJob, ImportDto importDto,
            IList<TaxonomyVocabulary> otherVocabularies, IList<TaxonomyTerm> otherTaxonomyTerms)
        {
            var dataService = Util.GetDataService();
            //var vocabularyController = new VocabularyController();
            var localTaxonomyTerms = GetTaxonomyTerms(importDto.PortalId, DateUtils.GetDatabaseUtcTime().AddYears(1), null);
            foreach (var other in otherTaxonomyTerms)
            {
                var createdBy = Common.Util.GetUserIdByName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdByName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);

                var vocabulary = otherVocabularies.FirstOrDefault(v => v.VocabularyID == other.VocabularyID);
                var vocabularyId = vocabulary?.LocalId ?? 0;
                var local = localTaxonomyTerms.FirstOrDefault(t => t.Name == other.Name && t.VocabularyID == vocabularyId);

                if (local != null)
                {
                    other.LocalId = local.TermID;
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored taxonomy", other.Name);
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

                            if (term.ParentTermId.HasValue)
                            {
                                dataService.UpdateHeirarchicalTerm(term, modifiedBy);
                            }
                            else
                            {
                                dataService.UpdateSimpleTerm(term, modifiedBy);
                            }
                            DataCache.ClearCache(string.Format(DataCache.TermCacheKey, term.TermId));
                            Result.AddLogEntry("Updated taxonomy", other.Name);
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


                    other.LocalId = term.ParentTermId.HasValue
                        ? dataService.AddHeirarchicalTerm(term, createdBy)
                        : dataService.AddSimpleTerm(term, createdBy);
                    Result.AddLogEntry("Added taxonomy", other.Name);
                }
            }
        }
    }
}