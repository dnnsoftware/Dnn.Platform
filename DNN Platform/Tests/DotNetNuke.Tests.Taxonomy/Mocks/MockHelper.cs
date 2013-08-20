#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Linq.Expressions;

using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Web.Validators;

using Moq;

namespace DotNetNuke.Tests.Taxonomy.Mocks
{
    public static class MockHelper
    {
        internal static readonly ValidationResult InvalidResult = new ValidationResult(new[] { new ValidationError { ErrorMessage = "Foo", PropertyName = "Bar" } });

        internal static Mock<IScopeTypeController> CreateMockScopeTypeController()
        {
            // Create the mock
            var mockScopeTypes = new Mock<IScopeTypeController>();
            mockScopeTypes.Setup(s => s.GetScopeTypes()).Returns(TestScopeTypes);

            //Register Mock
            return RegisterMockController(mockScopeTypes);
        }

        internal static Mock<ITermController> CreateMockTermController()
        {
            // Create the mock
            var mockTerms = new Mock<ITermController>();
            mockTerms.Setup(t => t.GetTermsByVocabulary(Constants.VOCABULARY_ValidVocabularyId)).Returns(TestTerms);

            //Return Mock
            return mockTerms;
        }

        internal static Mock<IVocabularyController> CreateMockVocabularyController()
        {
            // Create the mock
            var mockVocabularies = new Mock<IVocabularyController>();
            mockVocabularies.Setup(v => v.GetVocabularies()).Returns(TestVocabularies);

            //Register Mock
            return RegisterMockController(mockVocabularies);
        }

        internal static Mock<ObjectValidator> EnableValidMockValidator<T>(Validator validator, T target) where T : class
        {
            return EnableMockValidator(validator, ValidationResult.Successful, target);
        }

        internal static Mock<ObjectValidator> EnableInvalidMockValidator<T>(Validator validator, T target) where T : class
        {
            return EnableMockValidator(validator, InvalidResult, target);
        }

        internal static Mock<ObjectValidator> EnableMockValidator<T>(Validator validator, ValidationResult result, T target) where T : class
        {
            var mockValidator = new Mock<ObjectValidator>();

            Expression<Func<ObjectValidator, ValidationResult>> setupExpression;
            if (target == null)
            {
                setupExpression = v => v.ValidateObject(It.IsAny<T>());
            }
            else
            {
                setupExpression = v => v.ValidateObject(target);
            }
            mockValidator.Setup(setupExpression).Returns(result);

            validator.Validators.Clear();
            validator.Validators.Add(mockValidator.Object);
            return mockValidator;
        }

        private static Mock<TMock> RegisterMockController<TMock>(Mock<TMock> mock) where TMock : class
        {
            if (ComponentFactory.Container == null)
            {
                //Create a Container
                ComponentFactory.Container = new SimpleContainer();
            }

            //Try and get mock
            var getMock = ComponentFactory.GetComponent<Mock<TMock>>();

            if (getMock == null)
            {
                // Create the mock
                getMock = mock;

                // Add both mock and mock.Object to Container
                ComponentFactory.RegisterComponentInstance<Mock<TMock>>(getMock);
                ComponentFactory.RegisterComponentInstance<TMock>(getMock.Object);
            }
            return getMock;
        }

        internal static IQueryable<ScopeType> TestScopeTypes
        {
            get
            {
                var scopeTypes = new List<ScopeType> { new ScopeType { ScopeTypeId = 1, ScopeType = "Application" }, new ScopeType { ScopeTypeId = 2, ScopeType = "Portal" } };

                return scopeTypes.AsQueryable();
            }
        }

        internal static IQueryable<Term> TestTerms
        {
            get
            {
                List<Term> terms = new List<Term>();

                for (int i = Constants.TERM_ValidTermId; i < Constants.TERM_ValidTermId + Constants.TERM_ValidCount; i++)
                {
                    Term term = new Term(Constants.VOCABULARY_ValidVocabularyId);
                    term.TermId = i;
                    term.Name = ContentTestHelper.GetTermName(i);
                    term.Description = ContentTestHelper.GetTermName(i);
                    term.Weight = Constants.TERM_ValidWeight;

                    terms.Add(term);
                }

                return terms.AsQueryable();
            }
        }

        internal static IQueryable<Vocabulary> TestVocabularies
        {
            get
            {
                List<Vocabulary> vocabularies = new List<Vocabulary>();

                for (int i = Constants.VOCABULARY_ValidVocabularyId; i < Constants.VOCABULARY_ValidVocabularyId + Constants.VOCABULARY_ValidCount; i++)
                {
                    Vocabulary vocabulary = new Vocabulary();
                    vocabulary.VocabularyId = i;
                    vocabulary.Name = ContentTestHelper.GetVocabularyName(i);
                    vocabulary.Type = (i == Constants.VOCABULARY_HierarchyVocabularyId) ? VocabularyType.Hierarchy : VocabularyType.Simple;
                    vocabulary.Description = ContentTestHelper.GetVocabularyName(i);
                    vocabulary.ScopeTypeId = Constants.SCOPETYPE_ValidScopeTypeId;
                    vocabulary.Weight = Constants.VOCABULARY_ValidWeight;

                    vocabularies.Add(vocabulary);
                }

                return vocabularies.AsQueryable();
            }
        }
    }
}