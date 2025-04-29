namespace Dnn.PersonaBar.Security.Tests.Checks;

using System;
using System.Collections.Generic;

using Dnn.PersonaBar.Library.DTO;
using Dnn.PersonaBar.Pages.Components;
using Dnn.PersonaBar.Pages.Services.Dto;
using Dnn.PersonaBar.Security.Components;
using Dnn.PersonaBar.Security.Components.Checks;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CheckUserProfilePageTests
{
    private const string CustomUserProfilePageTabPath = "//MyCustomUserProfilePage";
    private const int PortalId = 1;

    [Test]
    [TestCase("portalController")]
    [TestCase("tabController")]
    [TestCase("pagesController")]
    public void Constructor_WhenArgumentIsNull_ThrowsArgumentNullException(string nullParamName)
    {
        // arrange
        var argumentNullException = default(ArgumentNullException);
        var unexpectedException = default(Exception);

        // act
        try
        {
            new CheckUserProfilePage(
                nullParamName == "portalController" ? null : new Mock<IPortalController>().Object,
                nullParamName == "tabController" ? null : new Mock<ITabController>().Object,
                nullParamName == "pagesController" ? null : new Mock<IPagesController>().Object);
        }
        catch (ArgumentNullException ex)
        {
            argumentNullException = ex;
        }
        catch (Exception ex)
        {
            unexpectedException = ex;
        }

        Assert.Multiple(() =>
        {
            // assert
            Assert.That(unexpectedException, Is.Null);
            Assert.That(argumentNullException, Is.Not.Null);
        });
        Assert.That(argumentNullException.ParamName, Is.EqualTo(nullParamName));
    }

    [Test]
    public void Execute_WhenNotFound_ReturnsPass()
    {
        // arrange
        var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
        var tabControllerMock = SetupTabControllerMock();
        var pagesControllerMock = new Mock<IPagesController>();

        var sut = new CheckUserProfilePage(
            portalControllerMock.Object,
            tabControllerMock.Object,
            pagesControllerMock.Object);

        // act
        var result = sut.Execute();

        // assert
        Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
    }

    [Test]
    public void Execute_WhenDeleted_ReturnsPass()
    {
        // arrange
        var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);

        var tabControllerMock = SetupTabControllerMock(profilePage: new TabInfo { IsDeleted = true });

        var pagesControllerMock = new Mock<IPagesController>();

        var sut = new CheckUserProfilePage(
            portalControllerMock.Object,
            tabControllerMock.Object,
            pagesControllerMock.Object);

        // act
        var result = sut.Execute();

        // assert
        Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
    }

    [Test]

    public void Execute_WhenPublic_ReturnsWarning()
    {
        // arrange
        RegisterTestablePermissionProvider();

        var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
        var activityFeedPage = BuildActivityFeedTabInfo();
        var tabControllerMock = SetupTabControllerMock(activityFeedPage);
        var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: true);

        var sut = new CheckUserProfilePage(
            portalControllerMock.Object,
            tabControllerMock.Object,
            pagesControllerMock.Object);

        // act
        var result = sut.Execute();

        // assert
        Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Warning));
    }

    [Test]

    public void Execute_WhenNeitherPublicNorActivityFeed_ReturnsPass()
    {
        // arrange
        RegisterTestablePermissionProvider();

        var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
        var customProfilePage = BuildCustomProfileTabInfo();
        var tabControllerMock = SetupTabControllerMock(customProfilePage);
        var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: false);

        var sut = new CheckUserProfilePage(
            portalControllerMock.Object,
            tabControllerMock.Object,
            pagesControllerMock.Object);

        // act
        var result = sut.Execute();

        // assert
        Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
    }

    [Test]

    public void Execute_WhenNotPublicAndActivityFeedAndMyProfileNotFound_ReturnsPass()
    {
        // arrange
        RegisterTestablePermissionProvider();

        var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
        var activityFeedPage = BuildActivityFeedTabInfo();
        var tabControllerMock = SetupTabControllerMock(activityFeedPage, myProfilePage: null);
        var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: false);

        var sut = new CheckUserProfilePage(
            portalControllerMock.Object,
            tabControllerMock.Object,
            pagesControllerMock.Object);

        // act
        var result = sut.Execute();

        // assert
        Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
    }

    [Test]

    public void Execute_WhenNotPublicAndActivityFeedAndMyProfileDeleted_ReturnsPass()
    {
        // arrange
        RegisterTestablePermissionProvider();

        var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
        var activityFeedPage = BuildActivityFeedTabInfo();
        var myProfilePage = BuildMyProfileTabInfo(deleted: true);
        var tabControllerMock = SetupTabControllerMock(activityFeedPage, myProfilePage);
        var pagesControllerMock = SetupPagesControllerMock(userProfilePageIsPublic: false);

        var sut = new CheckUserProfilePage(
            portalControllerMock.Object,
            tabControllerMock.Object,
            pagesControllerMock.Object);

        // act
        var result = sut.Execute();

        // assert
        Assert.That(result.Severity, Is.EqualTo(SeverityEnum.Pass));
    }

    [Test]
    [TestCase(true, SeverityEnum.Warning)]
    [TestCase(false, SeverityEnum.Pass)]

    public void Execute_WhenNotPublicAndActivityFeedAndMyProfileValid_ReturnsCorrectSeverity(
        bool myProfilePageIsPublic, SeverityEnum expectedSeverity)
    {
        // arrange
        RegisterTestablePermissionProvider();

        var portalControllerMock = SetupPortalControllerMock(PortalId, userTabId: 1);
        var activityFeedPage = BuildActivityFeedTabInfo();
        var myProfilePage = BuildMyProfileTabInfo();
        var tabControllerMock = SetupTabControllerMock(activityFeedPage, myProfilePage);
        var pagesControllerMock = SetupPagesControllerMock(false, myProfilePageIsPublic);

        var sut = new CheckUserProfilePage(
            portalControllerMock.Object,
            tabControllerMock.Object,
            pagesControllerMock.Object);

        // act
        var result = sut.Execute();

        // assert
        Assert.That(result.Severity, Is.EqualTo(expectedSeverity));
    }

    private static TabInfo BuildActivityFeedTabInfo(bool deleted = false) => new TabInfo
    {
        TabID = 1,
        IsDeleted = deleted,
        TabPath = CheckUserProfilePage.ActivityFeedTabPath,
        PortalID = PortalId,
    };

    private static TabInfo BuildCustomProfileTabInfo(bool deleted = false) => new TabInfo
    {
        TabID = 1,
        IsDeleted = deleted,
        TabPath = CustomUserProfilePageTabPath,
        PortalID = PortalId,
    };

    private static TabInfo BuildMyProfileTabInfo(bool deleted = false) => new TabInfo
    {
        TabID = 2,
        IsDeleted = deleted,
        TabPath = CheckUserProfilePage.MyProfileTabPath,
        PortalID = PortalId,
    };

    private static void RegisterTestablePermissionProvider()
    {
        var mock = new Mock<PermissionProvider>();

        mock.Setup(x => x.ImplicitRolesForPages(It.IsAny<int>()))
            .Returns(new List<RoleInfo>());

        ComponentFactory.RegisterComponentInstance<PermissionProvider>(mock.Object);
    }

    private static Mock<IPagesController> SetupPagesControllerMock(
        bool userProfilePageIsPublic, bool myProfilePageIsPublic = false)
    {
        var mock = new Mock<IPagesController>();

        mock.Setup(x => x.GetPermissionsData(It.IsAny<int>()))
            .Returns((int pageId) =>
            {
                switch (pageId)
                {
                    case 1: return BuildPermissionsData(userProfilePageIsPublic);
                    case 2: return BuildPermissionsData(myProfilePageIsPublic);
                    default: throw new ArgumentOutOfRangeException(nameof(pageId));
                }
            });

        return mock;
    }

    private static PagePermissions BuildPermissionsData(bool allUsersCanView)
    {
        var permissionsData = new PagePermissions(false);
        if (allUsersCanView)
        {
            permissionsData.RolePermissions = new List<RolePermission>
            {
                new RolePermission
                {
                    RoleId = int.Parse(Globals.glbRoleAllUsers),
                    Permissions = new List<Permission>
                    {
                        new Permission
                        {
                            PermissionName = CheckUserProfilePage.ViewTab,
                            AllowAccess = true,
                        },
                    },
                },
            };
        }

        return permissionsData;
    }

    private static Mock<IPortalSettings> SetupPortalSettingsMock(int portalId, int userTabId)
    {
        var mock = new Mock<IPortalSettings>();
        mock.SetupGet(x => x.PortalId).Returns(portalId);
        mock.SetupGet(x => x.UserTabId).Returns(userTabId);
        return mock;
    }

    private static Mock<IPortalController> SetupPortalControllerMock(int portalId, int userTabId)
    {
        var settingsMock = SetupPortalSettingsMock(portalId, userTabId);

        var settingsDictionary = new Dictionary<string, string>
        {
            {
                nameof(settingsMock.Object.ContentLocalizationEnabled),
                settingsMock.Object.ContentLocalizationEnabled.ToString()
            },
        };

        var portalSettings = new PortalSettings
        {
            PortalId = settingsMock.Object.PortalId,
        };

        var mock = new Mock<IPortalController>();

        mock.Setup(x => x.GetCurrentSettings()).Returns(settingsMock.Object);
        mock.Setup(x => x.GetPortalSettings(It.IsAny<int>())).Returns(settingsDictionary);
        mock.Setup(x => x.GetCurrentPortalSettings()).Returns(portalSettings);

        PortalController.SetTestableInstance(mock.Object);

        return mock;
    }

    private static Mock<ITabController> SetupTabControllerMock(
        TabInfo profilePage = null, TabInfo myProfilePage = null)
    {
        var mock = new Mock<ITabController>();

        mock.Setup(x => x.GetTab(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(profilePage);

        mock.Setup(x => x.GetTabsByPortal(It.IsAny<int>()))
            .Returns((int portalId) =>
            {
                var tabCollection = new TabCollection();

                if (myProfilePage != null)
                {
                    tabCollection.Add(myProfilePage);
                }

                return tabCollection;
            });

        return mock;
    }
}
