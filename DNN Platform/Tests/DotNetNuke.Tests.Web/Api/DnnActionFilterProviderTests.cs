// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

using Moq;

using NUnit.Framework;

[TestFixture]
public class DnnActionFilterProviderTests
{
    [SetUp]
    public void SetUp()
    {
        CBO.SetTestableInstance(new MockCBO());
    }

    [TearDown]
    public void TearDown()
    {
        CBO.ClearInstance();
    }

    [Test]
    public void RequiresHostAttributeAddedWhenNoOtherActionFiltersPresent()
    {
        // Arrange
        var adMock = new Mock<HttpActionDescriptor>();
        adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>());

        var cdMock = new Mock<HttpControllerDescriptor>();
        cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());

        HttpActionDescriptor actionDescriptor = adMock.Object;
        actionDescriptor.ControllerDescriptor = cdMock.Object;

        var configuration = new HttpConfiguration();

        // Act
        var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
        var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();

        // Assert
        Assert.That(filters, Has.Count.EqualTo(1));
        Assert.That(filters.First().Instance, Is.InstanceOf<RequireHostAttribute>());
    }

    [Test]
    public void RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()
    {
        // Arrange
        var adMock = new Mock<HttpActionDescriptor>();
        adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new[] { new DnnAuthorizeAttribute() }));

        var cdMock = new Mock<HttpControllerDescriptor>();
        cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());

        HttpActionDescriptor actionDescriptor = adMock.Object;
        actionDescriptor.ControllerDescriptor = cdMock.Object;

        var configuration = new HttpConfiguration();

        // Act
        var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
        var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();

        // Assert
        Assert.That(filters, Has.Count.EqualTo(1));
        Assert.That(filters.First().Instance, Is.InstanceOf<DnnAuthorizeAttribute>());
    }

    [Test]
    public void RequiresHostAttributeAddedWhenNoOverrideAuthFilterPresent()
    {
        // Arrange
        var adMock = new Mock<HttpActionDescriptor>();
        adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new[] { new AuthorizeAttribute() }));

        var cdMock = new Mock<HttpControllerDescriptor>();
        cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());

        HttpActionDescriptor actionDescriptor = adMock.Object;
        actionDescriptor.ControllerDescriptor = cdMock.Object;

        var configuration = new HttpConfiguration();

        // Act
        var filterProvider = new DnnActionFilterProvider(Mock.Of<IServiceProvider>());
        var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();

        // Assert
        Assert.That(filters, Has.Count.EqualTo(2));
        Assert.That(filters.Last().Instance, Is.InstanceOf<RequireHostAttribute>());
    }
}
