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

using System.Linq;
using Dnn.ExportImport.Components.Dto.Taxonomy;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Interfaces;
using Dnn.ExportImport.Components.Providers;
using DotNetNuke.Common.Utilities;

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

        public void ImportData(ExportImportJob importJob, IExportImportRepository repository)
        {
            ProgressPercentage = 0;

            var otherScopeTypes = repository.GetAllItems<TaxonomyScopeType>();
            var localScopeTypes = CBO.FillCollection<TaxonomyScopeType>(DataProvider.Instance().GetAllScopeTypes());
            foreach (var other in otherScopeTypes)
            {
                var local = localScopeTypes.FirstOrDefault(s => s.ScopeType == other.ScopeType);
                if (local == null)
                {
                    //TODO: add remote object to local database
                }
                else
                {
                    other.LocalId = local.ScopeTypeID;
                    //TODO: check collision and behave accordingly
                }
            }
            ProgressPercentage += 25;

            var otherSocabularyTypes = repository.GetAllItems<TaxonomyVocabularyType>();
            var localVocabularyTypes = CBO.FillCollection<TaxonomyVocabularyType>(DataProvider.Instance().GetAllVocabularyTypes());
            foreach (var other in otherSocabularyTypes)
            {
                var local = localVocabularyTypes.FirstOrDefault(s => s.VocabularyType == other.VocabularyType);
                if (local == null)
                {
                    //TODO: add remote object to local database
                }
                else
                {
                    //TODO: check collision and behave accordingly
                }
            }
            ProgressPercentage += 25;

            var otherTaxonomyTerms = repository.GetAllItems<TaxonomyTerm>();
            var localTaxonomyTerms = CBO.FillCollection<TaxonomyTerm>(DataProvider.Instance().GetAllTerms());
            foreach (var other in otherTaxonomyTerms)
            {
                //TODO
                if (localTaxonomyTerms.Any()) { }
            }
            ProgressPercentage += 25;

            var otherTaxonomyVocabulary = repository.GetAllItems<TaxonomyVocabulary>();
            var localTaxonomyVocabulary = CBO.FillCollection<TaxonomyVocabulary>(DataProvider.Instance().GetAllVocabularies());
            foreach (var other in otherTaxonomyVocabulary)
            {
                //TODO
                if (localTaxonomyVocabulary.Any()) { }
            }
            ProgressPercentage += 25;
        }
    }
}