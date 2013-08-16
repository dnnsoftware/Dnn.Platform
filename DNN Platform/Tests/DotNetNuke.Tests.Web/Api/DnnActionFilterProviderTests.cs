using System.Collections.ObjectModel;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class DnnActionFilterProviderTests
    {
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

            //Act
            var filterProvider = new DnnActionFilterProvider();
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();

            //Assert
            Assert.AreEqual(1, filters.Count);
            Assert.IsInstanceOf<RequireHostAttribute>(filters.First().Instance);
        }

        [Test]
        public void RequiresHostAttributeNotAddedWhenAnOverrideAuthFilterPresent()
        {
            // Arrange
            var adMock = new Mock<HttpActionDescriptor>();
            adMock.Setup(ad => ad.GetFilters()).Returns(new Collection<IFilter>(new []{ new DnnAuthorizeAttribute() }));

            var cdMock = new Mock<HttpControllerDescriptor>();
            cdMock.Setup(cd => cd.GetFilters()).Returns(new Collection<IFilter>());

            HttpActionDescriptor actionDescriptor = adMock.Object;
            actionDescriptor.ControllerDescriptor = cdMock.Object;

            var configuration = new HttpConfiguration();

            //Act
            var filterProvider = new DnnActionFilterProvider();
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();

            //Assert
            Assert.AreEqual(1, filters.Count);
            Assert.IsInstanceOf<DnnAuthorizeAttribute>(filters.First().Instance);
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

            //Act
            var filterProvider = new DnnActionFilterProvider();
            var filters = filterProvider.GetFilters(configuration, actionDescriptor).ToList();

            //Assert
            Assert.AreEqual(2, filters.Count);
            Assert.IsInstanceOf<RequireHostAttribute>(filters.Last().Instance);
        }
    }
}