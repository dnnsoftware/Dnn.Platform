// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Vocabularies.Validators;

using System.Linq;

using Dnn.PersonaBar.Vocabularies.Components;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Web.Validators;

public class VocabularyNameValidator : ObjectValidator
{
    private IVocabularyController vocabularyController;
    private ITermController termController;

    /// <summary>Initializes a new instance of the <see cref="VocabularyNameValidator"/> class.</summary>
    /// <param name="vocabularyController"></param>
    /// <param name="termController"></param>
    public VocabularyNameValidator(IVocabularyController vocabularyController, ITermController termController)
    {
        this.vocabularyController = vocabularyController;
        this.termController = termController;
    }

    public override ValidationResult ValidateObject(object target)
    {
        if (target is Vocabulary)
        {
            var vocabulary = target as Vocabulary;
            var existVocabulary =
                this.vocabularyController.GetVocabularies().FirstOrDefault(v => v.Name == vocabulary.Name && v.ScopeId == vocabulary.ScopeId);

            if (existVocabulary != null && (vocabulary.VocabularyId == Null.NullInteger || existVocabulary.VocabularyId != vocabulary.VocabularyId))
            {
                return new ValidationResult(new[] { new ValidationError { ErrorMessage = Constants.VocabularyExistsError, PropertyName = Constants.VocabularyValidationPropertyName, Validator = this } });
            }
        }
        else if (target is Term)
        {
            var term = target as Term;
            var vocabulary = this.vocabularyController.GetVocabularies().FirstOrDefault(v => v.VocabularyId == term.VocabularyId);
            var terms = this.termController.GetTermsByVocabulary(term.VocabularyId);

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
                                        Validator = this,
                                    },
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
                return new ValidationResult(new[] { new ValidationError { ErrorMessage = Constants.VocabularyValidationError, PropertyName = string.Empty, Validator = this } });
            }
        }

        return ValidationResult.Successful;
    }
}
