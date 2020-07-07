// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Content.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///   Summary description for TermTests.
    /// </summary>
    [TestFixture]
    public class TermControllerTests
    {
        private Mock<CachingProvider> mockCache;

        [SetUp]
        public void SetUp()
        {
            Mock<IVocabularyController> vocabularyController = MockHelper.CreateMockVocabularyController();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);

            // Register MockCachingProvider
            this.mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void TermController_AddTerm_Throws_On_Null_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.AddTerm(null));
        }

        [Test]
        public void TermController_AddTerm_Throws_On_Invalid_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
            term.Name = Constants.TERM_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => termController.AddTerm(term));
        }

        [Test]
        public void TermController_AddTerm_Throws_On_Negative_VocabularyId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Null.NullInteger);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.AddTerm(term));
        }

        [Test]
        public void TermController_AddTerm_Should_Call_DataService_AddSimpleTerm_If_Term_Is_Simple_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);

            // Act
            int termId = termController.AddTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.AddSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
        }

        [Test]
        public void TermController_AddTerm_Should_Call_DataService_AddHeirarchicalTerm_If_Term_Is_Heirarchical_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);

            // Act
            int termId = termController.AddTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.AddHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
        }

        [Test]
        public void TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);
            mockDataService.Setup(ds => ds.AddSimpleTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);

            // Act
            int termId = termController.AddTerm(term);

            // Assert
            Assert.AreEqual(Constants.TERM_AddTermId, termId);
        }

        [Test]
        public void TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Simple_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);
            mockDataService.Setup(ds => ds.AddSimpleTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);

            // Act
            termController.AddTerm(term);

            // Assert
            Assert.AreEqual(Constants.TERM_AddTermId, term.TermId);
        }

        [Test]
        public void TermController_AddTerm_Returns_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);
            mockDataService.Setup(ds => ds.AddHeirarchicalTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);

            // Act
            int termId = termController.AddTerm(term);

            // Assert
            Assert.AreEqual(Constants.TERM_AddTermId, termId);
        }

        [Test]
        public void TermController_AddTerm_Sets_Valid_Id_On_Valid_Term_If_Term_Is_Heirarchical_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);
            mockDataService.Setup(ds => ds.AddHeirarchicalTerm(It.IsAny<Term>(), It.IsAny<int>())).Returns(Constants.TERM_AddTermId);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);

            // Act
            termController.AddTerm(term);

            // Assert
            Assert.AreEqual(Constants.TERM_AddTermId, term.TermId);
        }

        [Test]
        public void TermController_AddTerm_Clears_Term_Cache_On_Valid_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);

            // Act
            termController.AddTerm(term);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(string.Format(Constants.TERM_CacheKey, Constants.VOCABULARY_ValidVocabularyId)));
        }

        [Test]
        public void TermController_AddTermToContent_Throws_On_Null_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            ContentItem content = ContentTestHelper.CreateValidContentItem();

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.AddTermToContent(null, content));
        }

        [Test]
        public void TermController_AddTermToContent_Throws_On_Null_ContentItem()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.AddTermToContent(term, null));
        }

        [Test]
        public void TermController_AddTermToContent_Should_Call_DataService_If_Valid_Params()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            ContentItem content = ContentTestHelper.CreateValidContentItem();
            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);

            // Act
            termController.AddTermToContent(term, content);

            // Assert
            mockDataService.Verify(ds => ds.AddTermToContent(term, content));
        }

        [Test]
        public void TermController_DeleteTerm_Throws_On_Null_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.DeleteTerm(null));
        }

        [Test]
        public void TermController_DeleteTerm_Throws_On_Negative_TermId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            var term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
            term.TermId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.DeleteTerm(term));
        }

        [Test]
        public void TermController_DeleteTerm_Should_Call_DataService_DeleteSimpleTerm_If_Term_Is_Simple_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            var term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
            term.TermId = Constants.TERM_DeleteTermId;

            // Act
            termController.DeleteTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.DeleteSimpleTerm(term));
        }

        [Test]
        public void TermController_DeleteTerm_Should_Call_DataService_DeleteHeirarchicalTerm_If_Term_Is_Heirarchical_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            var term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);
            term.TermId = Constants.TERM_DeleteTermId;

            // Act
            termController.DeleteTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.DeleteHeirarchicalTerm(term));
        }

        [Test]
        public void TermController_DeleteTerm_Clears_Term_Cache_On_Valid_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            var term = new Term(Constants.VOCABULARY_ValidVocabularyId) { TermId = Constants.TERM_DeleteTermId };

            // Act
            termController.DeleteTerm(term);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(string.Format(Constants.TERM_CacheKey, Constants.VOCABULARY_ValidVocabularyId)));
        }

        [Test]
        public void TermController_GetTerm_Throws_On_Negative_TermId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.GetTerm(Null.NullInteger));
        }

        [Test]
        public void TermController_GetTerm_Returns_Null_On_InValidTermId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetTerm(Constants.TERM_InValidTermId)).Returns(MockHelper.CreateEmptyTermReader());

            var termController = new TermController(mockDataService.Object);

            // Act
            Term term = termController.GetTerm(Constants.TERM_InValidTermId);

            // Assert
            Assert.IsNull(term);
        }

        [Test]
        public void TermController_GetTerm_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetTerm(Constants.TERM_ValidTermId)).Returns(MockHelper.CreateValidTermReader());
            var termController = new TermController(mockDataService.Object);

            // Act
            Term term = termController.GetTerm(Constants.TERM_ValidTermId);

            // Assert
            mockDataService.Verify(ds => ds.GetTerm(Constants.TERM_ValidTermId));
        }

        [Test]
        public void TermController_GetTerm_Returns_Term_On_Valid_TermId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetTerm(Constants.TERM_ValidTermId)).Returns(MockHelper.CreateValidTermReader());

            var termController = new TermController(mockDataService.Object);

            // Act
            var term = termController.GetTerm(Constants.TERM_ValidTermId);

            // Assert
            Assert.AreEqual(Constants.TERM_ValidTermId, term.TermId);
            Assert.AreEqual(Constants.TERM_ValidName, term.Name);
        }

        [Test]
        public void TermController_GetTermsByContent_Throws_On_Invalid_ContentItemId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.GetTermsByContent(Null.NullInteger));
        }

        [Test]
        public void TermController_GetTermsByContent_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetTermsByContent(Constants.TERM_ValidContent1)).Returns(MockHelper.CreateValidTermsReader(
                Constants.TERM_ValidCountForContent1,
                v => Constants.TERM_ValidVocabularyId,
                c => Constants.TERM_ValidContent1));
            var termController = new TermController(mockDataService.Object);

            // Act
            IQueryable<Term> terms = termController.GetTermsByContent(Constants.TERM_ValidContent1);

            // Assert
            mockDataService.Verify(ds => ds.GetTermsByContent(Constants.TERM_ValidContent1));
        }

        [Test]
        public void TermController_GetTermsByContent_Returns_Terms_On_Valid_ContentItemId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetTermsByContent(Constants.TERM_ValidContent1)).Returns(MockHelper.CreateValidTermsReader(
                Constants.TERM_ValidCountForContent1,
                v => Constants.TERM_ValidVocabularyId,
                c => Constants.TERM_ValidContent1));

            var termController = new TermController(mockDataService.Object);

            // Act
            var terms = termController.GetTermsByContent(Constants.TERM_ValidContent1).ToList();

            // Assert
            Assert.AreEqual(Constants.TERM_ValidCountForContent1, terms.Count);

            for (int i = 0; i < Constants.TERM_ValidCountForContent1; i++)
            {
                Assert.AreEqual(i + Constants.TERM_ValidTermId, terms[i].TermId);
                Assert.AreEqual(ContentTestHelper.GetTermName(i + Constants.TERM_ValidTermId), terms[i].Name);
            }
        }

        [Test]
        public void TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.GetTermsByVocabulary(Null.NullInteger));
        }

        [Test]
        public void TermController_GetTermsByVocabulary_Returns_Terms_On_Valid_VocabularyId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetTermsByVocabulary(Constants.TERM_ValidVocabulary1)).Returns(MockHelper.CreateValidTermsReader(
                Constants.TERM_ValidCountForVocabulary1,
                v => Constants.TERM_ValidVocabulary1,
                c => Constants.TERM_ValidContent1));
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);

            var termController = new TermController(mockDataService.Object);

            // Act
            var terms = termController.GetTermsByVocabulary(Constants.TERM_ValidVocabulary1).ToList();

            // Assert
            Assert.AreEqual(Constants.TERM_ValidCountForVocabulary1, terms.Count);

            for (int i = 0; i < Constants.TERM_ValidCountForVocabulary1; i++)
            {
                Assert.AreEqual(i + Constants.TERM_ValidTermId, terms[i].TermId);
                Assert.AreEqual(ContentTestHelper.GetTermName(i + Constants.TERM_ValidTermId), terms[i].Name);
            }
        }

        [Test]
        public void TermController_GetTermsByVocabulary_Throws_On_Invalid_VocabularyName()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => termController.GetTermsByVocabulary(Null.NullString));
        }

        [Test]
        public void TermController_RemoveTermsFromContent_Throws_On_Null_ContentItem()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.RemoveTermsFromContent(null));
        }

        [Test]
        public void TermController_RemoveTermsFromContent_Should_Call_DataService_If_Valid_Params()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            ContentItem content = ContentTestHelper.CreateValidContentItem();

            // Act
            termController.RemoveTermsFromContent(content);

            // Assert
            mockDataService.Verify(ds => ds.RemoveTermsFromContent(content));
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Null_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => termController.UpdateTerm(null));
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Negative_TermId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Null.NullInteger);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.UpdateTerm(term));
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Invalid_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
            term.Name = Constants.TERM_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.UpdateTerm(term));
        }

        [Test]
        public void TermController_UpdateTerm_Throws_On_Negative_VocabularyId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Null.NullInteger);

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => termController.UpdateTerm(term));
        }

        [Test]
        public void TermController_UpdateTerm_Should_Call_DataService_UpdateSimpleTerm_If_Term_Is_Simple_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
            term.TermId = Constants.TERM_UpdateTermId;
            term.Name = Constants.TERM_UpdateName;
            term.Weight = Constants.TERM_UpdateWeight;

            // Act
            termController.UpdateTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.UpdateSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
        }

        [Test]
        public void TermController_UpdateTerm_Should_Call_DataService_UpdateHeirarchicalTerm_If_Term_Is_Heirarchical_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidHeirarchicalTerm(Constants.VOCABULARY_HierarchyVocabularyId, Constants.TERM_ValidParentTermId);
            term.TermId = Constants.TERM_UpdateTermId;
            term.Name = Constants.TERM_UpdateName;
            term.Weight = Constants.TERM_UpdateWeight;

            // Act
            termController.UpdateTerm(term);

            // Assert
            mockDataService.Verify(ds => ds.UpdateHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID));
        }

        [Test]
        public void TermController_UpdateTerm_Clears_Term_Cache_On_Valid_Term()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var termController = new TermController(mockDataService.Object);

            Term term = ContentTestHelper.CreateValidSimpleTerm(Constants.VOCABULARY_ValidVocabularyId);
            term.TermId = Constants.TERM_UpdateTermId;
            term.Name = Constants.TERM_UpdateName;
            term.Weight = Constants.TERM_UpdateWeight;

            // Act
            termController.UpdateTerm(term);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(string.Format(Constants.TERM_CacheKey, Constants.VOCABULARY_ValidVocabularyId)));
        }
    }
}
