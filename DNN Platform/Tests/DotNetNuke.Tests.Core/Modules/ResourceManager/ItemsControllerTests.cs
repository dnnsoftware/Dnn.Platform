// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.ResourceManager.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;
    using System.Web.Http.Hosting;

    using Dnn.Modules.ResourceManager.Services;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.UI.Modules;

    using Moq;
    using NUnit.Framework;

    /// <summary>Unit tests for the Resource Manager <see cref="ItemsController"/>.</summary>
    [TestFixture]
    public class ItemsControllerTests
    {
        /// <summary>The test portal id.</summary>
        private readonly int testPortalId = 1;

        private readonly Mock<IPortalController> portalControllerMock = new();
        private readonly Mock<IRoleController> roleControllerMock = new();
        private readonly Mock<ILocalizationProvider> localizationProviderMock = new();

        /// <summary>
        /// Simulates the in-process roles cache: <see cref="IRoleController.GetRoles"/> returns
        /// this same list of shared <see cref="RoleInfo"/> instances on every call.
        /// </summary>
        private List<RoleInfo> cachedRoles;

        [SetUp]
        public void SetUp()
        {
            // The controller resolves singletons (e.g. FolderMappingController) via the
            // component factory at construction time, so register test doubles up front.
            MockComponentProvider.ResetContainer();
            ComponentFactory.RegisterComponentInstance<IFolderMappingController>(new Mock<IFolderMappingController>().Object);

            this.cachedRoles = new List<RoleInfo>
            {
                new RoleInfo
                {
                    RoleID = 100,
                    PortalID = this.testPortalId,
                    RoleName = "Content Editors",
                    Status = RoleStatus.Approved,
                },
                new RoleInfo
                {
                    RoleID = 200,
                    PortalID = this.testPortalId,
                    RoleName = "Content Managers",
                    Status = RoleStatus.Approved,
                },
            };

            this.portalControllerMock
                .Setup(p => p.GetCurrentPortalSettings())
                .Returns(new PortalSettings { PortalId = this.testPortalId });

            this.roleControllerMock
                .Setup(r => r.GetRoles(It.IsAny<int>()))
                .Returns(this.cachedRoles);

            // Simulates a culture whose GlobalResources resx localizes the standard roles.
            this.localizationProviderMock
                .Setup(l => l.GetString("Content Editors.Role", It.IsAny<string>()))
                .Returns("Éditeurs de contenu");
            this.localizationProviderMock
                .Setup(l => l.GetString("Content Managers.Role", It.IsAny<string>()))
                .Returns("Gestionnaires de contenu");

            PortalController.SetTestableInstance(this.portalControllerMock.Object);
            RoleController.SetTestableInstance(this.roleControllerMock.Object);
            LocalizationProvider.SetTestableInstance(this.localizationProviderMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
            RoleController.ClearInstance();
            LocalizationProvider.ClearInstance();
        }

        [Test]
        public void GetRoles_ReturnsLocalizedRoleNames()
        {
            // Arrange
            var controller = this.CreateController();

            // Act
            var content = ReadResponseContent(controller.GetRoles());

            // Assert
            Assert.That(content, Does.Contain("Éditeurs de contenu"));
            Assert.That(content, Does.Contain("Gestionnaires de contenu"));
        }

        /// <summary>
        /// Regression test for https://github.com/dnnsoftware/Dnn.Platform/issues/7604:
        /// the shared, cached <see cref="RoleInfo"/> instances must not be mutated with
        /// localized names, otherwise every role lookup by the real name fails and the
        /// whole site returns 500 until the app pool is recycled.
        /// </summary>
        [Test]
        public void GetRoles_DoesNotMutateSharedRoleInfoCache()
        {
            // Arrange
            var controller = this.CreateController();

            // Act
            var content = ReadResponseContent(controller.GetRoles());

            // Assert
            Assert.That(content, Does.Contain("Éditeurs de contenu"));
            Assert.That(this.cachedRoles.Single(r => r.RoleID == 100).RoleName, Is.EqualTo("Content Editors"));
            Assert.That(this.cachedRoles.Single(r => r.RoleID == 200).RoleName, Is.EqualTo("Content Managers"));
        }

        private static string ReadResponseContent(IHttpActionResult result)
        {
            var response = result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult();
            return response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        private ItemsController CreateController()
        {
            var controller = new ItemsController(
                new Mock<IModuleControlPipeline>().Object,
                new Mock<IApplicationStatusInfo>().Object,
                new Mock<IPermissionDefinitionService>().Object,
                new Mock<IHostSettings>().Object,
                new Mock<RoleProvider>().Object,
                new Mock<ICryptographyProvider>().Object,
                new Mock<IModuleController>().Object);

            var request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
            controller.Request = request;
            return controller;
        }
    }
}
