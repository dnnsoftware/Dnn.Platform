// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Modules.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Common.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PostJournalTests : IntegrationTestBase
    {
        private static readonly Random rnd = new Random();

        private readonly string _hostName;
        private readonly string _hostPass;

        private readonly int PortalId = 0;

        public PostJournalTests()
        {
            var url = ConfigurationManager.AppSettings["siteUrl"];
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            try
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                {
                    DatabaseHelper.ExecuteNonQuery("TRUNCATE TABLE {objectQualifier}JsonWebTokens");
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [Test]
        public void Journal_Should_Able_To_Attach_Files_Upload_By_Himself()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
        }

        [Test]
        public void Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);
            fileId = DatabaseHelper.ExecuteScalar<int>($"SELECT MIN(FileId) FROM {{objectQualifier}}Files WHERE PortalId = {this.PortalId}");

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
            }
            catch (WebApiException ex)
            {
                exceptionThrown = true;
                exceptionMessage = Json.Deserialize<dynamic>(ex.Body).ExceptionMessage;
            }

            Assert.IsTrue(exceptionThrown, "Should throw out exception");
            Assert.AreEqual("you have no permission to attach files not belongs to you.", exceptionMessage);
        }

        [Test]
        public void Journal_Should_Not_Able_To_Post_On_Other_User_Profile_Page()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = 1,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
            }
            catch (WebApiException ex)
            {
                exceptionThrown = true;
                exceptionMessage = Json.Deserialize<dynamic>(ex.Body).ExceptionMessage;
            }

            Assert.IsTrue(exceptionThrown, "Should throw out exception");
            Assert.AreEqual("you have no permission to post journal on current profile page.", exceptionMessage);
        }

        [Test]
        public void Journal_Should_Able_To_Post_On_Friends_Profile_Page()
        {
            int userId1, fileId1, userId2, fileId2;
            string username1, username2;
            var connector1 = this.PrepareNewUser(out userId1, out username1, out fileId1);
            var connector2 = this.PrepareNewUser(out userId2, out username2, out fileId2);

            // ADD FRIENDS
            connector1.PostJson("/API/MemberDirectory/MemberDirectory/AddFriend", new { friendId = userId2 }, this.GetRequestHeaders("Member Directory"));

            var notificationId = DatabaseHelper.ExecuteScalar<int>($"SELECT TOP 1 MessageID FROM {{objectQualifier}}CoreMessaging_Messages WHERE SenderUserID = {userId1}");
            connector2.PostJson("/API/InternalServices/RelationshipService/AcceptFriend", new { NotificationId = notificationId }, this.GetRequestHeaders());

            // POST JOURNAL
            var postData = new
            {
                text = $"{username1} Post",
                profileId = userId2,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}",
            };

            connector1.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
        }

        [Test]
        public void Journal_Should_Able_To_Post_On_Group_Already_Join()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);
            var groupId = this.CreateNewGroup(username.Replace("testuser", "testrole"));
            this.AddUserToGroup(groupId, userId);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = groupId,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
        }

        [Test]
        public void Journal_Should_Not_Able_To_Post_On_Group_Not_Join()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);
            var groupId = this.CreateNewGroup(username.Replace("testuser", "testrole"));

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = groupId,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());
            }
            catch (WebApiException ex)
            {
                exceptionThrown = true;
                exceptionMessage = Json.Deserialize<dynamic>(ex.Body).ExceptionMessage;
            }

            Assert.IsTrue(exceptionThrown, "Should throw out exception");
            Assert.AreEqual("you have no permission to post journal on current group.", exceptionMessage);
        }

        [Test]
        public void Journal_Should_Not_Able_To_Post_Xss_Code_In_ImageUrl()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"javascript:alert(1);\\\" onerror=\\\"alert(2);.png\",\"Url\":\"fileid={fileId}\"}}",
            };

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var imageUrl = Json.Deserialize<dynamic>(itemData).ImageUrl.ToString();

            Assert.AreEqual(-1, imageUrl.IndexOf("javascript"));
            Assert.AreEqual(-1, imageUrl.IndexOf("onerror"));
            Assert.AreEqual(-1, imageUrl.IndexOf("alert"));
        }

        [Test]
        public void Journal_Should_Not_Able_To_Post_Xss_Code_In_Url()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"javascript:alert(1);\", \"Title\": \"Test.png\"}}",
            };

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var url = Json.Deserialize<dynamic>(itemData).Url.ToString();

            Assert.AreEqual(-1, url.IndexOf("javascript"));
        }

        [Test]
        public void Journal_Should_Not_Able_To_Post_Extenal_Link_In_Url()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);

            // POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"http://www.dnnsoftware.com\", \"Title\": \"Test.png\"}}",
            };

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var url = Json.Deserialize<dynamic>(itemData).Url.ToString();

            Assert.AreEqual(-1, url.IndexOf("www.dnnsoftware.com"));
        }

        [Test]
        public void Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);

            // POST JOURNAL
            var journalText = $"{username} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.Greater(response.IndexOf(journalText), 0);

            connector.Logout();
            response = connector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.Greater(response.IndexOf(journalText), 0);
        }

        [Test]
        public void Journal_Should_Only_Able_See_By_Members_When_Set_Security_To_CommunityMembers()
        {
            int userId1, fileId1, userId2, fileId2;
            string username1, username2;
            var connector1 = this.PrepareNewUser(out userId1, out username1, out fileId1);
            var connector2 = this.PrepareNewUser(out userId2, out username2, out fileId2);

            // POST JOURNAL
            var journalText = $"{username1} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId1,
                groupId = -1,
                journalType = "file",
                securitySet = "C",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}",
            };

            connector1.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector2.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.Greater(response.IndexOf(journalText), 0);

            connector2.Logout();
            response = connector2.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.AreEqual(response.IndexOf(journalText), -1);
        }

        [Test]
        public void Journal_Should_Only_Able_See_By_Friends_When_Set_Security_To_Friends()
        {
            int userId1, fileId1, userId2, fileId2, userId3, fileId3;
            string username1, username2, username3;
            var connector1 = this.PrepareNewUser(out userId1, out username1, out fileId1);
            var connector2 = this.PrepareNewUser(out userId2, out username2, out fileId2);
            var connector3 = this.PrepareNewUser(out userId3, out username3, out fileId3);

            // ADD FRIENDS
            connector1.PostJson("/API/MemberDirectory/MemberDirectory/AddFriend", new { friendId = userId2 }, this.GetRequestHeaders("Member Directory"));

            var notificationId = DatabaseHelper.ExecuteScalar<int>($"SELECT TOP 1 MessageID FROM {{objectQualifier}}CoreMessaging_Messages WHERE SenderUserID = {userId1}");
            connector2.PostJson("/API/InternalServices/RelationshipService/AcceptFriend", new { NotificationId = notificationId }, this.GetRequestHeaders());

            // POST JOURNAL
            var journalText = $"{username1} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId1,
                groupId = -1,
                journalType = "file",
                securitySet = "F",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}",
            };

            connector1.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector2.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.Greater(response.IndexOf(journalText), 0);

            response = connector3.GetContent($"/Activity-Feed/userId/{userId1}").Content.ReadAsStringAsync().Result;
            Assert.AreEqual(response.IndexOf(journalText), -1);
        }

        [Test]
        public void Journal_Should_Only_Able_See_By_Himself_When_Set_Security_To_Private()
        {
            int userId, fileId;
            string username;
            var connector = this.PrepareNewUser(out userId, out username, out fileId);

            // POST JOURNAL
            var journalText = $"{username} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "P",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}",
            };

            connector.PostJson("/API/Journal/Services/Create", postData, this.GetRequestHeaders());

            var response = connector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.Greater(response.IndexOf(journalText), 0);

            var hostConnector = WebApiTestHelper.LoginHost();
            response = hostConnector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.AreEqual(response.IndexOf(journalText), -1);
        }

        private IWebApiConnector PrepareNewUser(out int userId, out string username, out int fileId)
        {
            return WebApiTestHelper.PrepareNewUser(out userId, out username, out fileId, this.PortalId);
        }

        private int CreateNewGroup(string roleName)
        {
            var connector = WebApiTestHelper.LoginHost();

            var url = "/API/PersonaBar/Roles/SaveRole?assignExistUsers=false";
            connector.PostJson(url, new
            {
                id = -1,
                name = roleName,
                groupId = -1,
                description = roleName,
                securityMode = 1,
                status = 1,
                isPublic = true,
                autoAssign = false,
                isSystem = false,
            });

            return DatabaseHelper.ExecuteScalar<int>($"SELECT RoleId FROM {{objectQualifier}}Roles WHERE RoleName = '{roleName}' AND PortalId = {this.PortalId}");
        }

        private void AddUserToGroup(int groupId, int userId)
        {
            var connector = WebApiTestHelper.LoginHost();

            var url = "/API/PersonaBar/Roles/AddUserToRole?notifyUser=true&isOwner=false";
            connector.PostJson(url, new
            {
                userId = userId,
                roleId = groupId,
                isAdd = true,
            });
        }

        private IDictionary<string, string> GetRequestHeaders(string moduleName = "Journal")
        {
            return WebApiTestHelper.GetRequestHeaders("//ActivityFeed", moduleName, this.PortalId);
        }
    }
}
