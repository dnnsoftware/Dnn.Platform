using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Controllers;
using DNN.Integration.Test.Framework.Helpers;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Profile;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Localization;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Tests
{
    public class FriendsTests : IntegrationTestBase
    {
        #region Fields

        private const int PortalId = 0;
        private const string FirstLanguage = "en-US";
        private const string SecondLanguage = "fr-FR";

        #endregion

        #region SetUp

        [SetUp]
        public void SetUp()
        {
            Globals.SetStatus(Globals.UpgradeStatus.None);
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider());
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>()
            {
                {"name", "SqlDataProvider"},
                {"type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke"},
                {"connectionStringName", "SiteSqlServer"},
                {"objectQualifier", ConfigurationManager.AppSettings["objectQualifier"]},
                {"databaseOwner", "dbo."}
            });
            ComponentFactory.InstallComponents(new ProviderInstaller("caching", typeof(CachingProvider), typeof(FBCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("members", typeof(MembershipProvider), typeof(AspNetMembershipProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("profiles", typeof(ProfileProvider), typeof(DNNProfileProvider)));

        }

        #endregion

        #region Tests

        [Test]
        public void Friend_Request_Should_Match_Target_User_Culture()
        {
            PrepareSecondLanguage();
            CreateNewUser(out var userId1, out var userName1, out var fileId1);
            CreateNewUser(out var userId2, out var userName2, out var fileId2);

            UpdateUserProfile(userId1, UserProfile.USERPROFILE_PreferredLocale, FirstLanguage);
            UpdateUserProfile(userId2, UserProfile.USERPROFILE_PreferredLocale, SecondLanguage);
            WebApiTestHelper.ClearHostCache();

            var connector = WebApiTestHelper.LoginUser(userName1);
            connector.PostJson("API/MemberDirectory/MemberDirectory/AddFriend", new
            {
                friendId = userId2
            }, GetRequestHeaders());

            var notificationTitle = GetNotificationTitle(userId1);

            //the notification should use french language: testuser8836 veut être amis avec vous
            Assert.AreEqual($"{userName1} veut être amis", notificationTitle);
        }

        private void UpdateUserProfile(int userId, string propertyName, string propertyValue)
        {
            var user = DotNetNuke.Entities.Users.UserController.Instance.GetUserById(PortalId, userId);
            if (user != null)
            {
                var userProfile = user.Profile;
                userProfile.SetProfileProperty(propertyName, propertyValue);
                ProfileController.UpdateUserProfile(user);
            }
        }

        #endregion

        #region Private Methods

        private void PrepareSecondLanguage()
        {
            if (!LanguageEnabled(PortalId, SecondLanguage))
            {

            }
        }

        private bool LanguageEnabled(int portalId, string secondLanguage)
        {
            var portalLanguages = CBO.FillDictionary<string, Locale>("CultureCode",  DataProvider.Instance().GetLanguagesByPortal(portalId));
            if (!portalLanguages.ContainsKey(secondLanguage))
            {
                var connector = WebApiTestHelper.LoginHost();
                connector.PostJson($"API/PersonaBar/Extensions/ParseLanguagePackage?cultureCode={secondLanguage}", new {});
                connector.PostJson($"API/PersonaBar/Extensions/InstallAvailablePackage", new
                {
                    PackageType = "CoreLanguagePack",
                    FileName = "installlanguage.resources"
                });

                var language = CBO.FillDictionary<string, Locale>("CultureCode", DataProvider.Instance().GetLanguages())[secondLanguage];
                connector.PostJson("API/PersonaBar/SiteSettings/UpdateLanguage", new
                {
                    PortalId = PortalId,
                    LanguageId = language.LanguageId,
                    Code = language.Code,
                    Enabled = true,
                    IsDefault = false,
                    Roles = "Administrators"
                });
            }
            return false;
        }

        private IWebApiConnector CreateNewUser(out int userId, out string username, out int fileId)
        {
            return WebApiTestHelper.PrepareNewUser(out userId, out username, out fileId, PortalId);
        }

        private string GetNotificationTitle(int userId)
        {
            return DatabaseHelper.ExecuteScalar<string>(
                $"select TOP 1 Subject from {{objectQualifier}}[CoreMessaging_Messages] WHERE SenderUserID = {userId}");
        }

        private IDictionary<string, string> GetRequestHeaders()
        {
            return WebApiTestHelper.GetRequestHeaders("Member Directory", PortalId);
        }

        #endregion
    }
}
