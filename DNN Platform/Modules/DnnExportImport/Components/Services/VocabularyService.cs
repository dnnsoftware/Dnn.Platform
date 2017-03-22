#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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
using Dnn.ExportImport.Components.Dto.Taxonomy;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using Util = DotNetNuke.Entities.Content.Common.Util;

namespace Dnn.ExportImport.Components.Services
{
    public class VocabularyService : Portable2Base
    {
        private int _progressPercentage;

        public override string Category => Constants.Category_Vocabularies;

        public override string ParentCategory => Constants.Category_Portal;

        public override uint Priority => 1;

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            private set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _progressPercentage = value;
            }
        }

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            var sinceDate = exportDto.SinceTime?.DateTime;
            var tillDate = exportJob.CreatedOnDate;
            ProgressPercentage = 0;
            if (CheckPoint.Stage > 3) return;

            if (CheckPoint.Stage == 0)
            {
                if (CheckCancelled(exportJob)) return;
                var scopeTypes = CBO.FillCollection<TaxonomyScopeType>(DataProvider.Instance().GetAllScopeTypes());
                Repository.CreateItems(scopeTypes, null);
                //Result.AddSummary("Exported Taxonomy Scopes", scopeTypes.Count.ToString()); -- not imported so don't show
                ProgressPercentage = 25;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 1)
            {
                if (CheckCancelled(exportJob)) return;
                var vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
                Repository.CreateItems(vocabularyTypes, null);
                //Result.AddSummary("Exported Vocabulary Types", vocabularyTypes.Count.ToString()); -- not imported so don't show
                ProgressPercentage = 50;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 2)
            {
                if (CheckCancelled(exportJob)) return;
                var taxonomyTerms = CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms(tillDate, sinceDate));
                Repository.CreateItems(taxonomyTerms, null);
                Result.AddSummary("Exported Terms", taxonomyTerms.Count.ToString());
                ProgressPercentage = 75;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 3)
            {
                if (CheckCancelled(exportJob)) return;
                var taxonomyVocabularies =
                    CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies(tillDate, sinceDate));
                Repository.CreateItems(taxonomyVocabularies, null);
                Result.AddSummary("Exported Vocabularies", taxonomyVocabularies.Count.ToString());
                ProgressPercentage = 100;

                CheckPoint.Stage++;
                CheckPointStageCallback(this);
            }
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exportDto)
        {
            ProgressPercentage = 0;

            if (CheckPoint.Stage > 3) return;

            if (CheckCancelled(importJob)) return;
            var otherScopeTypes = Repository.GetAllItems<TaxonomyScopeType>().ToList();

            if (CheckPoint.Stage == 0)
            {
                //the table Taxonomy_ScopeTypes is used for lookup only and never changed/updated in the database
                ProgressPercentage = 10;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 1)
            {
                //var otherVocabularyTypes = Repository.GetAllItems<TaxonomyVocabularyType>().ToList();
                //the table Taxonomy_VocabularyTypes is used for lookup only and never changed/updated in the database
                ProgressPercentage = 20;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckCancelled(importJob)) return;
            var otherVocabularies = Repository.GetAllItems<TaxonomyVocabulary>().ToList();

            if (CheckPoint.Stage == 2)
            {
                ProcessVocabularies(importJob, exportDto, otherScopeTypes, otherVocabularies);
                Repository.UpdateItems(otherVocabularies);
                Result.AddSummary("Imported Terms", otherVocabularies.Count.ToString());
                ProgressPercentage = 60;

                CheckPoint.Stage++;
                if (CheckPointStageCallback(this)) return;
            }

            if (CheckPoint.Stage == 3)
            {
                if (CheckCancelled(importJob)) return;
                var otherTaxonomyTerms = Repository.GetAllItems<TaxonomyTerm>().ToList();
                ProcessTaxonomyTerms(importJob, exportDto, otherVocabularies, otherTaxonomyTerms);
                Repository.UpdateItems(otherTaxonomyTerms);
                Result.AddSummary("Imported Vocabularies", otherTaxonomyTerms.Count.ToString());
                ProgressPercentage = 100;

                CheckPoint.Stage++;
                CheckPointStageCallback(this);
            }
        }

        private void ProcessVocabularies(ExportImportJob importJob, ExportDto exportDto,
            IList<TaxonomyScopeType> otherScopeTypes, IEnumerable<TaxonomyVocabulary> otherVocabularies)
        {
            var changed = false;
            var dataService = Util.GetDataService();
            var localVocabularies = CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies(DateTime.UtcNow.AddYears(1), null));
            foreach (var other in otherVocabularies)
            {
                if (CheckCancelled(importJob)) return;
                var createdBy = Common.Util.GetUserIdOrName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdOrName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var local = localVocabularies.FirstOrDefault(t => t.Name == other.Name);
                var scope = otherScopeTypes.FirstOrDefault(s => s.ScopeTypeID == other.ScopeTypeID);

                if (local != null)
                {
                    other.LocalId = local.VocabularyID;
                    switch (exportDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored vocabulary", other.Name);
                            break;
                        case CollisionResolution.Overwrite:
                            var vocabulary = new Vocabulary(other.Name, other.Description)
                            {
                                IsSystem = other.IsSystem,
                                Weight = other.Weight,
                                ScopeId = other.ScopeID ?? 0,
                                ScopeTypeId = scope?.LocalId ?? other.ScopeTypeID,
                            };
                            dataService.UpdateVocabulary(vocabulary, modifiedBy);
                            Result.AddLogEntry("Updated vocabulary", other.Name);
                            changed = true;
                            break;
                        case CollisionResolution.Duplicate:
                            local = null; // so we can add new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                    }
                }

                if (local == null)
                {
                    var vocabulary = new Vocabulary(other.Name, other.Description, (VocabularyType)other.VocabularyTypeID)
                    {
                        IsSystem = other.IsSystem,
                        Weight = other.Weight,
                        ScopeId = other.ScopeID ?? 0,
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

        private void ProcessTaxonomyTerms(ExportImportJob importJob, ExportDto exportDto,
            IList<TaxonomyVocabulary> otherVocabularies, IList<TaxonomyTerm> otherTaxonomyTerms)
        {
            var dataService = Util.GetDataService();
            var localTaxonomyTerms = CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms(DateTime.UtcNow.AddYears(1), null));
            foreach (var other in otherTaxonomyTerms)
            {
                if (CheckCancelled(importJob)) return;
                var createdBy = Common.Util.GetUserIdOrName(importJob, other.CreatedByUserID, other.CreatedByUserName);
                var modifiedBy = Common.Util.GetUserIdOrName(importJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                var local = localTaxonomyTerms.FirstOrDefault(t => t.Name == other.Name);
                var vocabulary = otherVocabularies.FirstOrDefault(v => v.VocabularyID == other.VocabularyID);
                var vocabularyId = vocabulary?.LocalId ?? 0;

                if (local != null)
                {
                    other.LocalId = local.TermID;
                    switch (exportDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored taxonomy", other.Name);
                            break;
                        case CollisionResolution.Overwrite:
                            var parent = other.ParentTermID.HasValue
                                ? otherVocabularies.FirstOrDefault(v => v.VocabularyID == other.ParentTermID.Value)
                                : null;
                            var term = new Term(other.Name, other.Description, vocabularyId)
                            {
                                Name = other.Name,
                                ParentTermId = parent?.LocalId,
                                Weight = other.Weight,
                            };

                            if (term.ParentTermId.HasValue)
                                dataService.UpdateHeirarchicalTerm(term, modifiedBy);
                            else
                                dataService.UpdateSimpleTerm(term, modifiedBy);
                            DataCache.ClearCache(string.Format(DataCache.TermCacheKey, term.TermId));
                            Result.AddLogEntry("Updated taxonomy", other.Name);
                            break;
                        case CollisionResolution.Duplicate:
                            local = null; // so we can write new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exportDto.CollisionResolution.ToString());
                    }
                }

                if (local == null)
                {
                    var parent = other.ParentTermID.HasValue
                        ? otherTaxonomyTerms.FirstOrDefault(v => v.TermID == other.ParentTermID.Value)
                        : null;
                    var term = new Term(other.Name, other.Description, vocabularyId)
                    {
                        Name = other.Name,
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