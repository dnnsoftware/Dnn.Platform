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
        private TermController termController;
        private VocabularyController vocabularyController;
        private Validator validator;

        public VocabulariesController()
        {
            this.termController = new TermController();
            this.vocabularyController = new VocabularyController();
            this.validator = new Validator(new DataAnnotationsObjectValidator());
            this.validator.Validators.Add(new VocabularyNameValidator(this.vocabularyController, this.termController));
        }

        public List<Vocabulary> GetVocabularies(int portalId, int pageIndex, int pageSize, int scopeTypeId, out int total)
        {
            var vocabularies = this.vocabularyController.GetVocabularies().Where(v => v.ScopeType.ScopeType == "Application" || (v.ScopeType.ScopeType == "Portal" && v.ScopeId == portalId));
            if (scopeTypeId != Null.NullInteger)
            {
                vocabularies = vocabularies.Where(v => v.ScopeTypeId == scopeTypeId);
            }

            total = vocabularies.Count();
            return vocabularies.Skip(pageIndex * pageSize).Take(pageSize).ToList();
        }

        public bool IsSystemVocabulary(int vocabularyId)
        {
            return this.vocabularyController.GetVocabularies().Any(v => v.VocabularyId == vocabularyId && v.IsSystem);
        }

        public int AddVocabulary(Vocabulary vocabulary)
        {
            if (vocabulary.ScopeType.ScopeType == Constants.VocabularyScopeTypeWebsite)
            {
                vocabulary.ScopeId = PortalSettings.Current.PortalId;
            }

            var result = this.validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                return this.vocabularyController.AddVocabulary(vocabulary);
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

            var result = this.validator.ValidateObject(vocabulary);
            if (result.IsValid)
            {
                this.vocabularyController.UpdateVocabulary(vocabulary);
            }
            else
            {
                throw new VocabularyValidationException();
            }
        }

        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            this.vocabularyController.DeleteVocabulary(vocabulary);
        }

        public List<Term> GetTermsByVocabulary(int vocabularyId)
        {
            var vocabulary = this.vocabularyController.GetVocabularies().SingleOrDefault(v => v.VocabularyId == vocabularyId);
            return this.termController.GetTermsByVocabulary(vocabularyId).ToList();
        }

        public int AddTerm(Term term)
        {
            var result = this.validator.ValidateObject(term);
            if (result.IsValid)
            {
                return this.termController.AddTerm(term);
            }
            else
            {
                throw new TermValidationException();
            }
        }

        public void UpdateTerm(Term term)
        {
            var result = this.validator.ValidateObject(term);
            if (result.IsValid)
            {
                this.termController.UpdateTerm(term);
            }
            else
            {
                throw new TermValidationException();
            }
        }

        public void DeleteTerm(Term term)
        {
            this.termController.DeleteTerm(term);
        }

        public Term GetTerm(int termId)
        {
            return this.termController.GetTerm(termId);
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
                            ChildTerms = new List<TermDto>(),
                        };
                        this.AddChildNodes(termList, node);
                        parentNode.ChildTerms.Add(node);
                    }
                }
            }
        }
    }
}
