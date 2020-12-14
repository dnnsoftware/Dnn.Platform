namespace DotNetNuke.Tests.Core.Services.Installer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.XPath;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Installer.Installers;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    public class AssemblyInstallerTests : DnnUnitTest
    {
        [SetUp]
        public void SetUp()
        {
            var serviceCollection = new ServiceCollection();
            var appInfo = new Mock<IApplicationStatusInfo>();
            appInfo.SetupGet(app => app.Status).Returns(UpgradeStatus.None);
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => appInfo.Object);
            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

            var dp = MockComponentProvider.CreateDataProvider();
            dp.Setup(p => p.UnRegisterAssembly(It.IsAny<int>(), It.IsAny<string>())).Returns(true);

            LocalizationProvider.SetTestableInstance(new Mock<ILocalizationProvider>().Object);
        }

        [Test]
        public void Install_UnRegisterAssembly_ShouldSucceed_WhenFileIsMissing()
        {
            var nav = GetAssembliesXmlNavigator();

            var sut = new AssemblyInstaller()
            {
                DeleteFiles = true,
                Package = new PackageInfo(new InstallerInfo(Directory.GetCurrentDirectory(), InstallMode.Install)),
            };
            sut.ReadManifest(nav);

            sut.Install();
        }

        private XPathNavigator GetAssembliesXmlNavigator()
        {
            var doc = new XPathDocument(new StringReader(@"
<assemblies>
    <assembly action=""UnRegister"">
        <path>bin\Providers</path>
        <name>FakeAssembly.dll</name>
        <sourceFileName>FakeAssembly.dll</sourceFileName>
        <version>01.02.03</version>
    </assembly>
</assemblies>
"));
            return doc.CreateNavigator();
        }
    }
}
