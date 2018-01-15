#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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