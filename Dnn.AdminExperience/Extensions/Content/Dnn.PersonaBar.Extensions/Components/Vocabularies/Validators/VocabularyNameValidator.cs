﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Linq;
using Dnn.PersonaBar.Vocabularies.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Web.Validators;

#endregion

namespace Dnn.PersonaBar.Vocabularies.Validators
{
    public class VocabularyNameValidator : ObjectValidator
    {
        private IVocabularyController _vocabularyController;
        private ITermController _termController;

        public VocabularyNameValidator(IVocabularyController vocabularyController, ITermController termController)
        {
            _vocabularyController = vocabularyController;
            _termController = termController;
        }

        public override ValidationResult ValidateObject(object target)
        {
            if (target is Vocabulary)
            {
                var vocabulary = target as Vocabulary;
                var existVocabulary =
                    _vocabularyController.GetVocabularies().FirstOrDefault(v => v.Name == vocabulary.Name && v.ScopeId == vocabulary.ScopeId);

                if (existVocabulary != null && (vocabulary.VocabularyId == Null.NullInteger || existVocabulary.VocabularyId != vocabulary.VocabularyId))
                {
                    return new ValidationResult(new[] { new ValidationError { ErrorMessage = Constants.VocabularyExistsError, PropertyName = Constants.VocabularyValidationPropertyName, Validator = this } });
                }
            }
            else if (target is Term)
            {
                var term = target as Term;
                var vocabulary = _vocabularyController.GetVocabularies().FirstOrDefault(v => v.VocabularyId == term.VocabularyId);
                var terms = _termController.GetTermsByVocabulary(term.VocabularyId);

                if (vocabulary != null)
                {
                    if (vocabulary.IsHeirarchical)
                    {
                        if (term.ParentTermId > 0)
                        {
                            var existTerm = terms.FirstOrDefault(v => v.Name == term.Name && v.TermId != term.TermId && v.ParentTermId == term.ParentTermId);
                            if (existTerm != null)
                            {
                                return
                                    new ValidationResult(new[]
                                    {
                                        new ValidationError
                                        {
                                            ErrorMessage = Constants.TermValidationError,
                                            PropertyName = Constants.TermValidationPropertyName,
                                            Validator = this
                                        }
                                    });
                            }
                        }
                    }
                    else
                    {
                        var existTerm = terms.FirstOrDefault(v => v.Name == term.Name && v.TermId != term.TermId);
                        if (existTerm != null)
                        {
                            return new ValidationResult(new[] { new ValidationError { ErrorMessage = Constants.TermValidationError, PropertyName = Constants.TermValidationPropertyName, Validator = this } });
                        }
                    }
                }
                else
                {
                    return new ValidationResult(new[] { new ValidationError { ErrorMessage = Constants.VocabularyValidationError, PropertyName = "", Validator = this } });
                }
            }

            return ValidationResult.Successful;
        }
    }
}
