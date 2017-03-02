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
using System.Runtime.Remoting;
using System.Web.UI.WebControls;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Dto.Taxonomy;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Users;
using PlatformDataProvider = DotNetNuke.Data.DataProvider;

namespace Dnn.ExportImport.Components.Services
{
    public class VocabularyService : IPortable2
    {
        private int _progressPercentage;

        public string Category => "VOCABULARY";

        public uint Priority => 2;

        public bool CanCancel => true;

        public bool CanRollback => false;

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

        public void ExportData(ExportImportJob exportJob, IExportImportRepository repository)
        {
            ProgressPercentage = 0;

            var scopeTypes = CBO.FillCollection<TaxonomyScopeType>(DataProvider.Instance().GetAllScopeTypes());
            repository.CreateItems(scopeTypes, null);
            ProgressPercentage += 25;

            var vocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
            repository.CreateItems(vocabularyTypes, null);
            ProgressPercentage += 25;

            var taxonomyTerms = CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms());
            repository.CreateItems(taxonomyTerms, null);
            ProgressPercentage += 25;

            var taxonomyVocabulary = CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies());
            repository.CreateItems(taxonomyVocabulary, null);
            ProgressPercentage += 25;
        }

        public void ImportData(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository)
        {
            ProgressPercentage = 0;

            var otherScopeTypes = repository.GetAllItems<TaxonomyScopeType>().ToList();
            ProcessScopeTypes(exporteDto, otherScopeTypes);
            ProgressPercentage += 25;

            //var otherVocabularyTypes = repository.GetAllItems<TaxonomyVocabularyType>();
            //the table Taxonomy_VocabularyTypes is used for lookup only and never changed/updated in the database
            ProgressPercentage += 25;

            var otherVocabularies = repository.GetAllItems<TaxonomyVocabulary>();
            ProcessVocabularies(importJob, exporteDto, repository, otherScopeTypes, otherVocabularies);
            ProgressPercentage += 25;

            var otherTaxonomyTerms = repository.GetAllItems<TaxonomyTerm>().ToList();
            ProcessTaxonomyTerms(importJob, exporteDto, repository, otherScopeTypes, otherTaxonomyTerms);
            ProgressPercentage += 25;
        }

        private static void ProcessScopeTypes(ExportDto exporteDto,
            IEnumerable<TaxonomyScopeType> otherScopeTypes)
        {
            var dataService = Util.GetDataService();
            var localScopeTypes = CBO.FillCollection<TaxonomyScopeType>(DataProvider.Instance().GetAllScopeTypes());

            foreach (var other in otherScopeTypes)
            {
                var local = localScopeTypes.FirstOrDefault(s => s.ScopeType == other.ScopeType);
                if (local != null)
                {
                    switch (exporteDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            break;
                        case CollisionResolution.Overwrite:
                            var sType = new ScopeType
                            {
                                ScopeTypeId = local.ScopeTypeID,
                                ScopeType = other.ScopeType,
                            };
                            dataService.UpdateScopeType(sType);
                            other.LocalId = local.ScopeTypeID;
                            break;
                        case CollisionResolution.Duplicate:
                            local = null; // so we can write new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                    }
                }

                if (local == null)
                {
                    var sType = new ScopeType
                    {
                        ScopeType = other.ScopeType
                    };

                    other.LocalId = Util.GetDataService().AddScopeType(sType);
                }
            }
        }

        private void ProcessVocabularies(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository,
            IList<TaxonomyScopeType> otherScopeTypes, IEnumerable<TaxonomyVocabulary> otherVocabularies)
        {
            var dataService = Util.GetDataService();
            var localVocabularies = CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies());
            foreach (var other in otherVocabularies)
            {
                var addedBy = GetUserId(importJob, other.CreatedByUserID ?? -1, other.CreatedByUserName);
                var modifiedBy = GetUserId(importJob, other.LastModifiedByUserID ?? -1, other.LastModifiedByUserName);
                var local = localVocabularies.FirstOrDefault(t => t.Name == other.Name);

                if (local != null)
                {
                    switch (exporteDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            break;
                        case CollisionResolution.Overwrite:
                            //TODO: add logic here
                            break;
                        case CollisionResolution.Duplicate:
                            local = null; // so we can add new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                    }
                }

                if (local == null)
                {
                    //TODO add new
                }
            }
        }

        private void ProcessTaxonomyTerms(ExportImportJob importJob, ExportDto exporteDto, IExportImportRepository repository,
            IList<TaxonomyScopeType> otherScopeTypes, IEnumerable<TaxonomyTerm> otherTaxonomyTerms)
        {
            var dataService = Util.GetDataService();
            var localTaxonomyTerms = CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms());
            foreach (var other in otherTaxonomyTerms)
            {
                var addedBy = GetUserId(importJob, other.CreatedByUserID ?? -1, other.CreatedByUserName);
                var modifiedBy = GetUserId(importJob, other.LastModifiedByUserID ?? -1, other.LastModifiedByUserName);
                var local = localTaxonomyTerms.FirstOrDefault(t => t.Name == other.Name);

                if (local != null)
                {
                    switch (exporteDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            break;
                        case CollisionResolution.Overwrite:
                            //TODO: adjust VocabularyID
                            var term = new Term(other.Name, other.Description, other.VocabularyID)
                            {
                                //Name = other.Name,
                                //VocabularyID = other.VocabularyID, //TODO: adjust ID
                                //ParentTermID = other.ParentTermID,
                                //Description = other.Description,
                                //Weight = other.Weight,
                                //TermLeft = other.TermLeft,
                                //CreatedByUserID = addedBy,
                                //CreatedOnDate = other.CreatedOnDate,
                                //LastModifiedByUserID = modifiedBy,
                                //LastModifiedOnDate = other.LastModifiedOnDate,
                            };

                            //dataService.UpdateSimpleTerm(term);
                            break;
                        case CollisionResolution.Duplicate:
                            local = null; // so we can write new one below
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(exporteDto.CollisionResolution.ToString());
                    }

                }

                if (local == null)
                {
                    //TODO add new
                    var term = new Term
                    {

                    };

                    //dataService.AddHeirarchicalTerm()
                }
            }
        }

        private int GetUserId(ExportImportJob importJob, int exportedUserId, string exportUsername)
        {
            if (exportedUserId <= 0)
                return -1;
            if (exportedUserId == 1)
                return 1;

            var user = UserController.GetUserByName(importJob.PortalId, exportUsername);
            return user.UserID < 0 ? importJob.CreatedBy : user.UserID;
        }
    }
}