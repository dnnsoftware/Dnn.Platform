using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Tokens;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Services.Tokens
{
    [TestFixture]
    public class TokenReplaceTests
    {
        private Mock<CachingProvider> _mockCache;
        private Mock<IPortalController> _portalController;
        private Mock<IModuleController> _moduleController;
        private Mock<IUserController> _userController;
        private Mock<IHostController> _mockHostController;

        [SetUp]
        public void SetUp()
        {
            _mockCache = MockComponentProvider.CreateDataCacheProvider();
            _mockHostController = new Mock<IHostController>();
            _portalController = new Mock<IPortalController>();
            _moduleController = new Mock<IModuleController>();
            _userController = new Mock<IUserController>();
            PortalController.SetTestableInstance(_portalController.Object);
            ModuleController.SetTestableInstance(_moduleController.Object);
            UserController.SetTestableInstance(_userController.Object);
            HostController.RegisterInstance(_mockHostController.Object);
            SetupPortalSettings();
            SetupModuleInfo();
            SetupUserInfo();
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
            ModuleController.ClearInstance();
            UserController.ClearInstance();
        }

        [Test]
        [TestCase("var myArray = [{ foo: 'bar' }]")]
        [TestCase("This is just plain text")]
        public void TextInputIsReturnedUnModified(string sourceText)
        {
            //Arrange
            var tokenReplace = new TokenReplace(Scope.DefaultSettings, PortalSettings.Current.DefaultLanguage,
                PortalSettings.Current, UserController.Instance.GetUser(1, 1), 1);

            //Act
            var outputText = tokenReplace.ReplaceEnvironmentTokens(sourceText);

            //Assert
            Assert.AreEqual(outputText, sourceText);
        }

        [Test]
        [TestCase("[Resx:{key:\"Email\"}]")]
        [TestCase("[Css: { path: \"~/DesktopModules/Dnn/ContactList/stylesheets/bootstrap.min.css\"}]")]
        [TestCase("[JavaScript:{ jsname: \"Knockout\" }] [JavaScript:{ path: \"~/DesktopModules/Dnn/ContactList/ClientScripts/contacts.js\"}]")]
        public void ObjectInputIsReturnedBlank(string sourceText)
        {
            //Arrange
            var tokenReplace = new TokenReplace(Scope.DefaultSettings, PortalSettings.Current.DefaultLanguage,
                PortalSettings.Current, UserController.Instance.GetUser(1, 1), 1);

            //Act
            var outputText = tokenReplace.ReplaceEnvironmentTokens(sourceText);

            //Assert
            Assert.AreEqual(outputText.Trim(), string.Empty);
        }

        private void SetupPortalSettings()
        {
            var portalSettings = new PortalSettings
            {
                AdministratorRoleName = Utilities.Constants.RoleName_Administrators,
                ActiveTab = new TabInfo { ModuleID = 1, TabID = 1 }
            };

            _portalController.Setup(pc => pc.GetCurrentPortalSettings()).Returns(portalSettings);
        }

        private void SetupModuleInfo()
        {
            var moduleInfo = new ModuleInfo
            {
                ModuleID = 1,
                PortalID = _portalController.Object.GetCurrentPortalSettings().PortalId
            };

            _moduleController.Setup(mc => mc.GetModule(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(moduleInfo);
        }

        private void SetupUserInfo()
        {
            var userInfo = new UserInfo
            {
                UserID = 1,
                Username = "admin",
                PortalID = _portalController.Object.GetCurrentPortalSettings().PortalId
            };
            _userController.Setup(uc => uc.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns(userInfo);
        }
    }
}
