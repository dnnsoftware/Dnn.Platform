// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Content;

using System;
using System.Linq;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Tests.Content.Mocks;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

/// <summary>  Summary description for ScopeTypeTests.</summary>
[TestFixture]
public class ScopeTypeControllerTests
{
    private Mock<CachingProvider> mockCache;

    [SetUp]

    public void SetUp()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
        serviceCollection.AddTransient<IHostSettingsService, HostController>();
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

        // Register MockCachingProvider
        this.mockCache = MockComponentProvider.CreateNew<CachingProvider>();
        MockComponentProvider.CreateDataProvider().Setup(c => c.GetProviderPath()).Returns(string.Empty);
    }

    [TearDown]
    public void TearDown()
    {
        Globals.DependencyProvider = null;
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
        Assert.That(scopeTypeId, Is.EqualTo(Constants.SCOPETYPE_AddScopeTypeId));
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
        Assert.That(scopeType.ScopeTypeId, Is.EqualTo(Constants.SCOPETYPE_AddScopeTypeId));
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
        Assert.That(scopeTypes, Is.Not.Null);
        Assert.That(scopeTypes.Count(), Is.EqualTo(0));
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
        Assert.That(scopeTypes.Count(), Is.EqualTo(Constants.SCOPETYPE_ValidScopeTypeCount));
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
