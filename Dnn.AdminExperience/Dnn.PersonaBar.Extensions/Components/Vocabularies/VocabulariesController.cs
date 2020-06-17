// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Vocabularies.Components
{
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Vocabularies.Exceptions;
    using Dnn.PersonaBar.Vocabularies.Services.Dto;
    using Dnn.PersonaBar.Vocabularies.Validators;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.Validators;

    public class VocabulariesController
    {
        private TermController _termController;
        private VocabularyController _vocabularyController;
        private Validator _validator;

        public VocabulariesController()
        {
            this._termController = new TermController();
            this._vocabularyController = new VocabularyController();
            this._validator = new Validator(new DataAnnotationsObjectValidator());
            this._validator.Validators.Add(new VocabularyNameValidator(this._vocabularyController, this._termController));
        }

        public List<Vocabulary> GetVocabularies(int portalId, int pageIndex, int pageSize, int scopeTypeId, out int total)
        {
            var vocabularies = this._vocabularyController.GetVocabularies().Where(v => v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == portalId));
            if (scopeTypeId != Null.NullInteger)
            {
                vocabularies = vocabularies.Where(v => v.ScopeTypeId == scopeTypeId);
            }
            total = vocabularies.Count();
            return vocabularies.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public bool IsSystemVocabulary(int vocabularyId)
        {
            return this._vocabularyController.GetVocabularies().Any(v => v.VocabularyId == vocabularyId && v.IsSystem);
        }

        public int AddVocabulary(Vocabulary vocabulary)
        {
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
            }
            var result = this._validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                return this._vocabularyController.AddVocabulary(vocabulary);
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
            var result = this._validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                this._vocabularyController.UpdateVocabulary(vocabulary);
            }
            else
            {
                throw new VocabularyValidationException();
            }
        }

        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            this._vocabularyController.DeleteVocabulary(vocabulary);
        }

        public List<Term> GetTermsByVocabulary(int vocabularyId)
        {
            var vocabulary = this._vocabularyController.GetVocabularies().SingleOrDefault(v => v.VocabularyId == vocabularyId);
            return this._termController.GetTermsByVocabulary(vocabularyId).ToList();
        }

        public int AddTerm(Term term)
        {
            var result = this._validator.ValidateObject(term);
            if (result.IsValid)
            {
                return this._termController.AddTerm(term);
            }
            else
            {
                throw new TermValidationException();
            }
        }

        public void UpdateTerm(Term term)
        {
            var result = this._validator.ValidateObject(term);
            if (result.IsValid)
            {
                this._termController.UpdateTerm(term);
            }
            else
            {
                throw new TermValidationException();
            }
        }

        public void DeleteTerm(Term term)
        {
            this._termController.DeleteTerm(term);
        }

        public Term GetTerm(int termId)
        {
            return this._termController.GetTerm(termId);
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
                        this.AddChildNodes(termList, node);
                        parentNode.ChildTerms.Add(node);
                    }
                }
            }
        }
    }
}
