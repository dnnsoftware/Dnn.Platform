// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content
{
    using System;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Content.Mocks;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///   Summary description for VocabularyTests.
    /// </summary>
    [TestFixture]
    public class VocabularyControllerTests
    {
        private Mock<CachingProvider> mockCache;

        [SetUp]
        public void SetUp()
        {
            // Register MockCachingProvider
            this.mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.AddVocabulary(null));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Invalid_Name()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.Name = Constants.VOCABULARY_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentException>(() => vocabularyController.AddVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.ScopeTypeId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.AddVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            // Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);

            // Assert
            mockDataService.Verify(ds => ds.AddVocabulary(vocabulary, It.IsAny<int>()));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            // Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);

            // Assert
            Assert.AreEqual(Constants.VOCABULARY_AddVocabularyId, vocabularyId);
        }

        [Test]
        public void VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            // Act
            vocabularyController.AddVocabulary(vocabulary);

            // Assert
            Assert.AreEqual(Constants.VOCABULARY_AddVocabularyId, vocabulary.VocabularyId);
        }

        [Test]
        public void VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            // Act
            vocabularyController.AddVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.DeleteVocabulary(null));
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = new Vocabulary();
            vocabulary.VocabularyId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.DeleteVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;

            // Act
            vocabularyController.DeleteVocabulary(vocabulary);

            // Assert
            mockDataService.Verify(ds => ds.DeleteVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;

            // Act
            vocabularyController.DeleteVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
        }

        [Test]
        public void VocabularyController_GetVocabularies_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
            var vocabularyController = new VocabularyController(mockDataService.Object);

            // Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();

            // Assert
            mockDataService.Verify(ds => ds.GetVocabularies());
        }

        [Test]
        public void VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
            var vocabularyController = new VocabularyController(mockDataService.Object);

            // Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();

            // Assert
            Assert.AreEqual(Constants.VOCABULARY_ValidCount, vocabularys.Count());
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.UpdateVocabulary(null));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.Name = Constants.VOCABULARY_InValidName;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.ScopeTypeId = Null.NullInteger;

            // Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;

            // Act
            vocabularyController.UpdateVocabulary(vocabulary);

            // Assert
            mockDataService.Verify(ds => ds.UpdateVocabulary(vocabulary, It.IsAny<int>()));
        }

        [Test]
        public void VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;

            // Act
            vocabularyController.UpdateVocabulary(vocabulary);

            // Assert
            this.mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
        }
    }
}
