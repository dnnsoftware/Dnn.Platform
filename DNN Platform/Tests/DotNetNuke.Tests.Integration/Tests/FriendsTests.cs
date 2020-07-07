// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests
{
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

    public class FriendsTests : IntegrationTestBase
    {
        private const int PortalId = 0;
        private const string FirstLanguage = "en-US";
        private const string SecondLanguage = "fr-FR";

        [SetUp]
        public void SetUp()
        {
            Globals.SetStatus(Globals.UpgradeStatus.None);
            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.RegisterComponentInstance<DataProvider>(new SqlDataProvider());
            ComponentFactory.RegisterComponentSettings<SqlDataProvider>(new Dictionary<string, string>()
            {
                { "name", "SqlDataProvider" },
                { "type", "DotNetNuke.Data.SqlDataProvider, DotNetNuke" },
                { "connectionStringName", "SiteSqlServer" },
                { "objectQualifier", ConfigurationManager.AppSettings["objectQualifier"] },
                { "databaseOwner", "dbo." },
            });
            ComponentFactory.InstallComponents(new ProviderInstaller("caching", typeof(CachingProvider), typeof(FBCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("members", typeof(MembershipProvider), typeof(AspNetMembershipProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("profiles", typeof(ProfileProvider), typeof(DNNProfileProvider)));
        }

        [Test]
        public void Friend_Request_Should_Match_Target_User_Culture()
        {
            this.PrepareSecondLanguage();
            int userId1, userId2, fileId1, fileId2;
            string userName1, userName2;
            this.CreateNewUser(out userId1, out userName1, out fileId1);
            this.CreateNewUser(out userId2, out userName2, out fileId2);

            this.UpdateUserProfile(userId1, UserProfile.USERPROFILE_PreferredLocale, FirstLanguage);
            this.UpdateUserProfile(userId2, UserProfile.USERPROFILE_PreferredLocale, SecondLanguage);
            WebApiTestHelper.ClearHostCache();

            var connector = WebApiTestHelper.LoginUser(userName1);
            connector.PostJson("API/MemberDirectory/MemberDirectory/AddFriend", new
            {
                friendId = userId2,
            }, this.GetRequestHeaders());

            var notificationTitle = this.GetNotificationTitle(userId1);

            // the notification should use french language: testuser8836 veut être amis avec vous
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

        private void PrepareSecondLanguage()
        {
            if (!this.LanguageEnabled(PortalId, SecondLanguage))
            {
            }
        }

        private bool LanguageEnabled(int portalId, string secondLanguage)
        {
            var portalLanguages = CBO.FillDictionary<string, Locale>("CultureCode", DataProvider.Instance().GetLanguagesByPortal(portalId));
            if (!portalLanguages.ContainsKey(secondLanguage))
            {
                var connector = WebApiTestHelper.LoginHost();
                connector.PostJson($"API/PersonaBar/Extensions/ParseLanguagePackage?cultureCode={secondLanguage}", new { });
                connector.PostJson($"API/PersonaBar/Extensions/InstallAvailablePackage", new
                {
                    PackageType = "CoreLanguagePack",
                    FileName = "installlanguage.resources",
                });

                var language = CBO.FillDictionary<string, Locale>("CultureCode", DataProvider.Instance().GetLanguages())[secondLanguage];
                connector.PostJson("API/PersonaBar/SiteSettings/UpdateLanguage", new
                {
                    PortalId = PortalId,
                    LanguageId = language.LanguageId,
                    Code = language.Code,
                    Enabled = true,
                    IsDefault = false,
                    Roles = "Administrators",
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
            return WebApiTestHelper.GetRequestHeaders("//ActivityFeed", "Member Directory", PortalId);
        }
    }
}
