// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;
using System.Linq;
using Dnn.PersonaBar.Vocabularies.Exceptions;
using Dnn.PersonaBar.Vocabularies.Services.Dto;
using Dnn.PersonaBar.Vocabularies.Validators;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.Validators;

#endregion

namespace Dnn.PersonaBar.Vocabularies.Components
{
    public class VocabulariesController
    {
        private TermController _termController;
        private VocabularyController _vocabularyController;
        private Validator _validator;

        public VocabulariesController()
        {
            _termController = new TermController();
            _vocabularyController = new VocabularyController();
            _validator = new Validator(new DataAnnotationsObjectValidator());
            _validator.Validators.Add(new VocabularyNameValidator(_vocabularyController, _termController));
        }

        public List<Vocabulary> GetVocabularies(int portalId, int pageIndex, int pageSize, int scopeTypeId, out int total)
        {
            var vocabularies = _vocabularyController.GetVocabularies().Where(v => v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == portalId));
            if (scopeTypeId != Null.NullInteger)
            {
                vocabularies = vocabularies.Where(v => v.ScopeTypeId == scopeTypeId);
            }
            total = vocabularies.Count();
            return vocabularies.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public bool IsSystemVocabulary(int vocabularyId)
        {
            return _vocabularyController.GetVocabularies().Any(v => v.VocabularyId == vocabularyId && v.IsSystem);
        }

        public int AddVocabulary(Vocabulary vocabulary)
        {
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
            }
            var result = _validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                return _vocabularyController.AddVocabulary(vocabulary);
            }
            else
            {
                throw new VocabularyNameAlreadyExistsException();
            }
        }

        public void UpdateVocabulary(Vocabulary vocabulary)
        {
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
            }
            var result = _validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                _vocabularyController.UpdateVocabulary(vocabulary);
            }
            else
            {
                throw new VocabularyValidationException();
            }
        }

        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            _vocabularyController.DeleteVocabulary(vocabulary);
        }

        public List<Term> GetTermsByVocabulary(int vocabularyId)
        {
            var vocabulary = _vocabularyController.GetVocabularies().SingleOrDefault(v => v.VocabularyId == vocabularyId);
            return _termController.GetTermsByVocabulary(vocabularyId).ToList();
        }

        public int AddTerm(Term term)
        {
            var result = _validator.ValidateObject(term);
            if (result.IsValid)
            {
                return _termController.AddTerm(term);
            }
            else
            {
                throw new TermValidationException();
            }
        }

        public void UpdateTerm(Term term)
        {
            var result = _validator.ValidateObject(term);
            if (result.IsValid)
            {
                _termController.UpdateTerm(term);
            }
            else
            {
                throw new TermValidationException();
            }
        }

        public void DeleteTerm(Term term)
        {
            _termController.DeleteTerm(term);
        }

        public Term GetTerm(int termId)
        {
            return _termController.GetTerm(termId);
        }

        private void AddChildNodes(List<Term> termList, TermDto parentNode)
        {
            if (parentNode.ChildTerms != null)
            {
                parentNode.ChildTerms.Clear();
                var parentId = parentNode.TermId;
                var terms = termList.Where(v => v.ParentTermId == parentId);

                foreach (var term in terms)
                {
                    if (term.ParentTermId == parentId)
                    {
                        var node = new TermDto
                        {
                            TermId = term.TermId,
                            Description = term.Description,
                            Name = term.Name,
                            ParentTermId = term.ParentTermId ?? Null.NullInteger,
                            VocabularyId = term.VocabularyId,
                            ChildTerms = new List<TermDto>()
                        };
                        AddChildNodes(termList, node);
                        parentNode.ChildTerms.Add(node);
                    }
                }
            }
        }
    }
}
