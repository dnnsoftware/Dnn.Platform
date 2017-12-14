#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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

namespace DotNetNuke.Tests.Content
{
    /// <summary>
    ///   Summary description for VocabularyTests
    /// </summary>
    [TestFixture]
    public class VocabularyControllerTests
    {
        private Mock<CachingProvider> mockCache;

        #region Test Initialize

        [SetUp]
        public void SetUp()
        {
            //Register MockCachingProvider
            mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(String.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        #endregion

        #region AddVocabulary

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Null_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.AddVocabulary(null));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Invalid_Name()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.Name = Constants.VOCABULARY_InValidName;

            //Act, Arrange
            Assert.Throws<ArgumentException>(() => vocabularyController.AddVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Throws_On_Negative_ScopeTypeID()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.ScopeTypeId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.AddVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Calls_DataService_On_Valid_Arguments()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            //Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);

            //Assert
            mockDataService.Verify(ds => ds.AddVocabulary(vocabulary, It.IsAny<int>()));
        }

        [Test]
        public void VocabularyController_AddVocabulary_Returns_ValidId_On_Valid_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            //Act
            int vocabularyId = vocabularyController.AddVocabulary(vocabulary);

            //Assert
            Assert.AreEqual(Constants.VOCABULARY_AddVocabularyId, vocabularyId);
        }

        [Test]
        public void VocabularyController_AddVocabulary_Sets_ValidId_On_Valid_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddVocabulary(It.IsAny<Vocabulary>(), It.IsAny<int>())).Returns(Constants.VOCABULARY_AddVocabularyId);
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            //Act
            vocabularyController.AddVocabulary(vocabulary);

            //Assert
            Assert.AreEqual(Constants.VOCABULARY_AddVocabularyId, vocabulary.VocabularyId);
        }

        [Test]
        public void VocabularyController_AddVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();

            //Act
            vocabularyController.AddVocabulary(vocabulary);

            //Assert
            mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
        }

        #endregion

        #region DeleteVocabulary

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Null_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.DeleteVocabulary(null));
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Throws_On_Negative_VocabularyId()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = new Vocabulary();
            vocabulary.VocabularyId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.DeleteVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Calls_DataService_On_Valid_Arguments()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;

            //Act
            vocabularyController.DeleteVocabulary(vocabulary);

            //Assert
            mockDataService.Verify(ds => ds.DeleteVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_DeleteVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_ValidVocabularyId;

            //Act
            vocabularyController.DeleteVocabulary(vocabulary);

            //Assert
            mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
        }

        #endregion

        #region GetVocabularies

        [Test]
        public void VocabularyController_GetVocabularies_Calls_DataService()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
            var vocabularyController = new VocabularyController(mockDataService.Object);

            //Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();

            //Assert
            mockDataService.Verify(ds => ds.GetVocabularies());
        }

        [Test]
        public void VocabularyController_GetVocabularies_Returns_List_Of_Vocabularies()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetVocabularies()).Returns(MockHelper.CreateValidVocabulariesReader(Constants.VOCABULARY_ValidCount));
            var vocabularyController = new VocabularyController(mockDataService.Object);

            //Act
            IQueryable<Vocabulary> vocabularys = vocabularyController.GetVocabularies();

            //Assert
            Assert.AreEqual(Constants.VOCABULARY_ValidCount, vocabularys.Count());
        }

        #endregion

        #region UpdateVocabulary

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Null_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            //Act, Arrange
            Assert.Throws<ArgumentNullException>(() => vocabularyController.UpdateVocabulary(null));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_VocabularyId()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Invalid_Name()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.Name = Constants.VOCABULARY_InValidName;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Throws_On_Negative_ScopeTypeID()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.ScopeTypeId = Null.NullInteger;

            //Act, Arrange
            Assert.Throws<ArgumentOutOfRangeException>(() => vocabularyController.UpdateVocabulary(vocabulary));
        }

        [Test]
        public void VocabularyController_UpdateVocabulary_Calls_DataService_On_Valid_Arguments()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;

            //Act
            vocabularyController.UpdateVocabulary(vocabulary);

            //Assert
            mockDataService.Verify(ds => ds.UpdateVocabulary(vocabulary, It.IsAny<int>()));
        }

        [Test]
        public void VocabularyController__UpdateVocabulary_Clears_Vocabulary_Cache_On_Valid_Vocabulary()
        {
            //Arrange
            var mockDataService = new Mock<IDataService>();
            var vocabularyController = new VocabularyController(mockDataService.Object);

            Vocabulary vocabulary = ContentTestHelper.CreateValidVocabulary();
            vocabulary.VocabularyId = Constants.VOCABULARY_UpdateVocabularyId;

            //Act
            vocabularyController.UpdateVocabulary(vocabulary);

            //Assert
            mockCache.Verify(cache => cache.Remove(Constants.VOCABULARY_CacheKey));
        }

        #endregion
    }
}