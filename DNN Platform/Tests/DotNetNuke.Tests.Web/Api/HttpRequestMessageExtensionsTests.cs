using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Hosting;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Web.Api;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class HttpRequestMessageExtensionsTests
    {
        [Test]
        public void FindTabIdTriesAllProviders()
        {
            //Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
            provider.Setup(x => x.TryFindTabId(request, out i)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            //Act
            request.FindTabId();

            //Assert
            provider.Verify(x => x.TryFindTabId(request, out i), Times.Exactly(2));
        }

        [Test]
        public void FindTabIdStopCallingProvidersAfterOneSuccess()
        {
            //Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
            provider.Setup(x => x.TryFindTabId(request, out i)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            //Act
            request.FindTabId();

            //Assert
            provider.Verify(x => x.TryFindTabId(request, out i), Times.Once());
        }

        [Test]
        public void FindModuleIdTriesAllProviders()
        {
            //Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
            provider.Setup(x => x.TryFindModuleId(request, out i)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            //Act
            request.FindModuleId();

            //Assert
            provider.Verify(x => x.TryFindModuleId(request, out i), Times.Exactly(2));
        }

        [Test]
        public void FindModuleIdStopCallingProvidersAfterOneSuccess()
        {
            //Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var provider = new Mock<ITabAndModuleInfoProvider>();
            int i;
            provider.Setup(x => x.TryFindModuleId(request, out i)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            //Act
            request.FindModuleId();

            //Assert
            provider.Verify(x => x.TryFindModuleId(request, out i), Times.Once());
        }

        [Test]
        public void FindModuleInfoTriesAllProviders()
        {
            //Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var provider = new Mock<ITabAndModuleInfoProvider>();
            ModuleInfo moduleInfo;
            provider.Setup(x => x.TryFindModuleInfo(request, out moduleInfo)).Returns(false);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            //Act
            request.FindModuleInfo();

            //Assert
            provider.Verify(x => x.TryFindModuleInfo(request, out moduleInfo), Times.Exactly(2));
        }

        [Test]
        public void FindModuleInfoStopCallingProvidersAfterOneSuccess()
        {
            //Arrange
            var request = new HttpRequestMessage();
            var configuration = new HttpConfiguration();
            var provider = new Mock<ITabAndModuleInfoProvider>();
            ModuleInfo moduleInfo;
            provider.Setup(x => x.TryFindModuleInfo(request, out moduleInfo)).Returns(true);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            configuration.AddTabAndModuleInfoProvider(provider.Object);
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;

            //Act
            request.FindModuleInfo();

            //Assert
            provider.Verify(x => x.TryFindModuleInfo(request, out moduleInfo), Times.Once());
        }
    }
}