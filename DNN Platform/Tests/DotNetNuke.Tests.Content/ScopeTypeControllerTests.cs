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
    ///   Summary description for ScopeTypeTests.
    /// </summary>
    [TestFixture]
    public class ScopeTypeControllerTests
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
        public void ScopeTypeController_AddScopeType_Throws_On_Null_ScopeType()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.AddScopeType(null));
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Calls_DataService_On_Valid_Arguments()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            var scopeType = ContentTestHelper.CreateValidScopeType();

            // Act
            int scopeTypeId = scopeTypeController.AddScopeType(scopeType);

            // Assert
            mockDataService.Verify(ds => ds.AddScopeType(scopeType));
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Returns_ValidId_On_Valid_ScopeType()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddScopeType(It.IsAny<ScopeType>())).Returns(Constants.SCOPETYPE_AddScopeTypeId);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();

            // Act
            int scopeTypeId = scopeTypeController.AddScopeType(scopeType);

            // Assert
            Assert.AreEqual(Constants.SCOPETYPE_AddScopeTypeId, scopeTypeId);
        }

        [Test]
        public void ScopeTypeController_AddScopeType_Sets_ValidId_On_Valid_ScopeType()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddScopeType(It.IsAny<ScopeType>())).Returns(Constants.SCOPETYPE_AddScopeTypeId);
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);
            var scopeType = ContentTestHelper.CreateValidScopeType();

            // Act
            scopeTypeController.AddScopeType(scopeType);

            // Assert
            Assert.AreEqual(Constants.SCOPETYPE_AddScopeTypeId, scopeType.ScopeTypeId);
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Throws_On_Null_ScopeType()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.DeleteScopeType(null));
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Throws_On_Negative_ScopeTypeId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeTypeId = Null.NullInteger;

            Assert.Throws<ArgumentOutOfRangeException>(() => scopeTypeController.DeleteScopeType(scopeType));
        }

        [Test]
        public void ScopeTypeController_DeleteScopeType_Calls_DataService_On_Valid_ContentTypeId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            var scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeTypeId = Constants.SCOPETYPE_ValidScopeTypeId;

            // Act
            scopeTypeController.DeleteScopeType(scopeType);

            // Assert
            mockDataService.Verify(ds => ds.DeleteScopeType(scopeType));
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateValidScopeTypesReader(Constants.SCOPETYPE_ValidScopeTypeCount));
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            // Act
            IQueryable<ScopeType> scopeTypes = scopeTypeController.GetScopeTypes();

            // Assert
            mockDataService.Verify(ds => ds.GetScopeTypes());
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Returns_Empty_List_Of_ScopeTypes_If_No_ScopeTypes()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateEmptyScopeTypeReader());
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            // Act
            var scopeTypes = scopeTypeController.GetScopeTypes();

            // Assert
            Assert.IsNotNull(scopeTypes);
            Assert.AreEqual(0, scopeTypes.Count());
        }

        [Test]
        public void ScopeTypeController_GetScopeTypes_Returns_List_Of_ScopeTypes()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetScopeTypes()).Returns(MockHelper.CreateValidScopeTypesReader(Constants.SCOPETYPE_ValidScopeTypeCount));
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            // Act
            var scopeTypes = scopeTypeController.GetScopeTypes();

            // Assert
            Assert.AreEqual(Constants.SCOPETYPE_ValidScopeTypeCount, scopeTypes.Count());
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Throws_On_Null_ScopeType()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            // Act, Arrange
            Assert.Throws<ArgumentNullException>(() => scopeTypeController.UpdateScopeType(null));
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Throws_On_Negative_ScopeTypeId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeType = Constants.SCOPETYPE_InValidScopeType;

            Assert.Throws<ArgumentOutOfRangeException>(() => scopeTypeController.UpdateScopeType(scopeType));
        }

        [Test]
        public void ScopeTypeController_UpdateScopeType_Calls_DataService_On_Valid_ContentType()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var scopeTypeController = new ScopeTypeController(mockDataService.Object);

            ScopeType scopeType = ContentTestHelper.CreateValidScopeType();
            scopeType.ScopeTypeId = Constants.SCOPETYPE_UpdateScopeTypeId;
            scopeType.ScopeType = Constants.SCOPETYPE_UpdateScopeType;

            // Act
            scopeTypeController.UpdateScopeType(scopeType);

            // Assert
            mockDataService.Verify(ds => ds.UpdateScopeType(scopeType));
        }
    }
}
