// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Services.Tokens;

using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Tests.Utilities.Mocks;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NUnit.Framework;

[TestFixture]
public class TokenReplaceTests
{
    private Mock<CachingProvider> mockCache;
    private Mock<IPortalController> portalController;
    private Mock<IModuleController> moduleController;
    private Mock<IUserController> userController;
    private Mock<IHostController> mockHostController;

    [SetUp]

    public void SetUp()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IApplicationStatusInfo>(container => Mock.Of<IApplicationStatusInfo>());
        serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
        serviceCollection.AddTransient<IHostSettingsService, HostController>();
        Globals.DependencyProvider = serviceCollection.BuildServiceProvider();

        ComponentFactory.RegisterComponentInstance<TokenProvider>(new CoreTokenProvider());
        this.mockCache = MockComponentProvider.CreateDataCacheProvider();
        this.mockHostController = new Mock<IHostController>();
        this.portalController = new Mock<IPortalController>();
        this.moduleController = new Mock<IModuleController>();
        this.userController = new Mock<IUserController>();
        PortalController.SetTestableInstance(this.portalController.Object);
        ModuleController.SetTestableInstance(this.moduleController.Object);
        UserController.SetTestableInstance(this.userController.Object);
        HostController.RegisterInstance(this.mockHostController.Object);
        this.SetupPortalSettings();
        this.SetupModuleInfo();
        this.SetupUserInfo();
    }

    [TearDown]
    public void TearDown()
    {
        Globals.DependencyProvider = null;
        PortalController.ClearInstance();
        ModuleController.ClearInstance();
        UserController.ClearInstance();
    }

    [Test]
    [TestCase("var myArray = [{ foo: 'bar' }]")]
    [TestCase("This is just plain text")]

    public void TextInputIsReturnedUnModified(string sourceText)
    {
        // Arrange
        var tokenReplace = new TokenReplace(Scope.DefaultSettings, PortalSettings.Current.DefaultLanguage,
            PortalSettings.Current, UserController.Instance.GetUser(1, 1), 1);

        // Act
        var outputText = tokenReplace.ReplaceEnvironmentTokens(sourceText);

        // Assert
        Assert.That(sourceText, Is.EqualTo(outputText));
    }

    [Test]
    [TestCase("[Resx:{key:\"Email\"}]")]
    [TestCase("[Css: { path: \"~/DesktopModules/Dnn/ContactList/stylesheets/bootstrap.min.css\"}]")]
    [TestCase("[JavaScript:{ jsname: \"Knockout\" }] [JavaScript:{ path: \"~/DesktopModules/Dnn/ContactList/ClientScripts/contacts.js\"}]")]

    public void ObjectInputIsReturnedBlank(string sourceText)
    {
        // Arrange
        var tokenReplace = new TokenReplace(Scope.DefaultSettings, PortalSettings.Current.DefaultLanguage,
            PortalSettings.Current, UserController.Instance.GetUser(1, 1), 1);

        // Act
        var outputText = tokenReplace.ReplaceEnvironmentTokens(sourceText);

        // Assert
        Assert.That(outputText.Trim(), Is.EqualTo(string.Empty));
    }

    private void SetupPortalSettings()
    {
        var portalSettings = new PortalSettings
        {
            AdministratorRoleName = Utilities.Constants.RoleName_Administrators,
            ActiveTab = new TabInfo { ModuleID = 1, TabID = 1 },
        };

        this.portalController.Setup(pc => pc.GetCurrentPortalSettings()).Returns(portalSettings);
    }

    private void SetupModuleInfo()
    {
        var moduleInfo = new ModuleInfo
        {
            ModuleID = 1,
            PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
        };

        this.moduleController.Setup(mc => mc.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .Returns(moduleInfo);
    }

    private void SetupUserInfo()
    {
        var userInfo = new UserInfo
        {
            UserID = 1,
            Username = "admin",
            PortalID = this.portalController.Object.GetCurrentPortalSettings().PortalId,
        };
        this.userController.Setup(uc => uc.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns(userInfo);
    }
}
