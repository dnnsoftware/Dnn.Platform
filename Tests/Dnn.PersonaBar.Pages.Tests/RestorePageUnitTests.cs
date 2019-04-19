using NUnit.Framework;
using Moq;
using Dnn.PersonaBar.Library.Prompt;
using Dnn.PersonaBar.Recyclebin.Components;
using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using Dnn.PersonaBar.Library.Helper;
using Dnn.PersonaBar.Library.Prompt.Models;

namespace Dnn.PersonaBar.Pages.Tests
{
    [TestFixture]
    public class RestorePageUnitTests
    {
        TabInfo _tab;
        string _message;
        PortalSettings _portalSettings;
        IConsoleCommand _restorePage;

        Mock<ITabController> _tabControllerMock;
        Mock<IRecyclebinController> _recyclebinControllerMock;
        Mock<IContentVerifier> _contentVerifierMock;

        int _tabId = 91;
        int _testPortalId = 1;

        [SetUp]
        public void RunBeforeAnyTest()
        {
            _message = string.Empty;

            _tab = new TabInfo();
            _tab.TabID = _tabId;
            _tab.PortalID = _testPortalId;

            _portalSettings = new PortalSettings();
            _portalSettings.PortalId = _testPortalId;

            _tabControllerMock = new Mock<ITabController>();
            _recyclebinControllerMock = new Mock<IRecyclebinController>();
            _contentVerifierMock = new Mock<IContentVerifier>();

            _tabControllerMock.SetReturnsDefault(_tab);
            _contentVerifierMock.SetReturnsDefault(true);
            _recyclebinControllerMock.Setup(r => r.RestoreTab(_tab, out _message));
        }

        [Test]
        public void Run_RestorePage_WithValidCommand_ShouldReturnSuccessResponse()
        {
            // Arrange                      
            SetupCommand();

            // Act
            var result = _restorePage.Run();

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(1, result.Records);
            Assert.IsFalse(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNonExistingTab_ShouldReturnErrorResponse()
        {
            // Arrange
            _tab = null;

            _tabControllerMock.Setup(t => t.GetTab(_tabId, _testPortalId)).Returns(_tab);

            SetupCommand();

            // Act
            var result = _restorePage.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForRequestedPortalNotAllowed_ShouldReturnErrorResponse()
        {
            // Arrange
            _contentVerifierMock.Setup(c => c.IsContentExistsForRequestedPortal(_testPortalId, _portalSettings, false)).Returns(false);

            SetupCommand();

            // Act
            var result = _restorePage.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        [Test]
        public void Run_RestorePage_WithValidCommandForNotRestoreTab_ShouldReturnErrorResponse()
        {
            // Arrange
            _message = "Tab not found";

            _recyclebinControllerMock.Setup(r => r.RestoreTab(_tab, out _message));

            SetupCommand();

            // Act
            var result = _restorePage.Run();

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.IsTrue(result is ConsoleErrorResultModel);
        }

        private void SetupCommand()
        {
            _restorePage = new RestorePage(_tabControllerMock.Object, _recyclebinControllerMock.Object, _contentVerifierMock.Object);

            var args = new[] { "restore-page", _tabId.ToString() };
            _restorePage.Initialize(args, _portalSettings, null, 0);
        }
    }
}