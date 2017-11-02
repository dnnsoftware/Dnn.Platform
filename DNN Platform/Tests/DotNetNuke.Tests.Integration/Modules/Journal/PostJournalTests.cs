using System;
using System.Collections.Generic;
using System.Configuration;
using DotNetNuke.Common.Utilities;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Helpers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Modules.Journal
{
    [TestFixture]
    public class PostJournalTests : IntegrationTestBase
    {
        #region Fields

        private readonly string _hostName;
        private readonly string _hostPass;

        private readonly int PortalId = 0;

        #endregion

        #region SetUp

        public PostJournalTests()
        {
            var url = ConfigurationManager.AppSettings["siteUrl"];
            _hostName = ConfigurationManager.AppSettings["hostUsername"];
            _hostPass = ConfigurationManager.AppSettings["hostPassword"];
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            try
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                    DatabaseHelper.ExecuteNonQuery("TRUNCATE TABLE {objectQualifier}JsonWebTokens");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region Tests

        [Test]
        public void Journal_Should_Able_To_Attach_Files_Upload_By_Himself()
        {
            int userId, fileId;
            string username;
            var connector = PrepareNewUser(out userId, out username, out fileId);

            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet  = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}"
            };
            
            
            connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());
        }

        [Test]
        public void Journal_Should_Not_Able_To_Attach_Files_Upload_By_Other_User()
        {
            int userId, fileId;
            string username;
            var connector = PrepareNewUser(out userId, out username, out fileId);
            fileId = DatabaseHelper.ExecuteScalar<int>($"SELECT MIN(FileId) FROM {{objectQualifier}}Files WHERE PortalId = {PortalId}");
            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}"
            };

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());
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
            var connector = PrepareNewUser(out userId, out username, out fileId);

            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = 1,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}"
            };

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());
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
            var connector1 = PrepareNewUser(out userId1, out username1, out fileId1);
            var connector2 = PrepareNewUser(out userId2, out username2, out fileId2);

            //ADD FRIENDS
            connector1.PostJson("/API/MemberDirectory/MemberDirectory/AddFriend", new {friendId = userId2}, GetRequestHeaders("Member Directory"));

            var notificationId = DatabaseHelper.ExecuteScalar<int>($"SELECT TOP 1 MessageID FROM {{objectQualifier}}CoreMessaging_Messages WHERE SenderUserID = {userId1}");
            connector2.PostJson("/API/InternalServices/RelationshipService/AcceptFriend", new { NotificationId  = notificationId }, GetRequestHeaders());

            //POST JOURNAL
            var postData = new
            {
                text = $"{username1} Post",
                profileId = userId2,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}"
            };

            connector1.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());
        }

        [Test]
        public void Journal_Should_Able_To_Post_On_Group_Already_Join()
        {
            int userId, fileId;
            string username;
            var connector = PrepareNewUser(out userId, out username, out fileId);
            var groupId = CreateNewGroup(username.Replace("testuser", "testrole"));
            AddUserToGroup(groupId, userId);

            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = groupId,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}"
            };

            connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());
        }

        [Test]
        public void Journal_Should_Not_Able_To_Post_On_Group_Not_Join()
        {
            int userId, fileId;
            string username;
            var connector = PrepareNewUser(out userId, out username, out fileId);
            var groupId = CreateNewGroup(username.Replace("testuser", "testrole"));

            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = groupId,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}"
            };

            var exceptionThrown = false;
            var exceptionMessage = string.Empty;
            try
            {
                connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());
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
            var connector = PrepareNewUser(out userId, out username, out fileId);

            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"javascript:alert(1);\\\" onerror=\\\"alert(2);.png\",\"Url\":\"fileid={fileId}\"}}"
            };

            connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());

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
            var connector = PrepareNewUser(out userId, out username, out fileId);

            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"javascript:alert(1);\", \"Title\": \"Test.png\"}}"
            };

            connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var url = Json.Deserialize<dynamic>(itemData).Url.ToString();

            Assert.AreEqual(-1, url.IndexOf("javascript"));
        }

        [Test]
        public void Journal_Should_Not_Able_To_Post_Extenal_Link_In_Url()
        {
            int userId, fileId;
            string username;
            var connector = PrepareNewUser(out userId, out username, out fileId);

            //POST JOURNAL
            var postData = new
            {
                text = $"{username} Post",
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"http://www.dnnsoftware.com\", \"Title\": \"Test.png\"}}"
            };

            connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());

            var itemData = DatabaseHelper.ExecuteScalar<string>($"SELECT ItemData FROM {{objectQualifier}}Journal WHERE UserId = {userId}");
            var url = Json.Deserialize<dynamic>(itemData).Url.ToString();

            Assert.AreEqual(-1, url.IndexOf("www.dnnsoftware.com"));
        }

        [Test]
        public void Journal_Should_Able_See_By_All_When_Set_Security_To_Everyone()
        {
            int userId, fileId;
            string username;
            var connector = PrepareNewUser(out userId, out username, out fileId);

            //POST JOURNAL
            var journalText = $"{username} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "E",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}"
            };

            connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());

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
            var connector1 = PrepareNewUser(out userId1, out username1, out fileId1);
            var connector2 = PrepareNewUser(out userId2, out username2, out fileId2);

            //POST JOURNAL
            var journalText = $"{username1} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId1,
                groupId = -1,
                journalType = "file",
                securitySet = "C",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}"
            };

            connector1.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());

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
            var connector1 = PrepareNewUser(out userId1, out username1, out fileId1);
            var connector2 = PrepareNewUser(out userId2, out username2, out fileId2);
            var connector3 = PrepareNewUser(out userId3, out username3, out fileId3);

            //ADD FRIENDS
            connector1.PostJson("/API/MemberDirectory/MemberDirectory/AddFriend", new { friendId = userId2 }, GetRequestHeaders("Member Directory"));

            var notificationId = DatabaseHelper.ExecuteScalar<int>($"SELECT TOP 1 MessageID FROM {{objectQualifier}}CoreMessaging_Messages WHERE SenderUserID = {userId1}");
            connector2.PostJson("/API/InternalServices/RelationshipService/AcceptFriend", new { NotificationId = notificationId }, GetRequestHeaders());


            //POST JOURNAL
            var journalText = $"{username1} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId1,
                groupId = -1,
                journalType = "file",
                securitySet = "F",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId1}\"}}"
            };

            connector1.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());

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
            var connector = PrepareNewUser(out userId, out username, out fileId);

            //POST JOURNAL
            var journalText = $"{username} Post";
            var postData = new
            {
                text = journalText,
                profileId = userId,
                groupId = -1,
                journalType = "file",
                securitySet = "P",
                itemData = $"{{\"ImageUrl\":\"\",\"Url\":\"fileid={fileId}\"}}"
            };

            connector.PostJson("/API/Journal/Services/Create", postData, GetRequestHeaders());

            var response = connector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.Greater(response.IndexOf(journalText), 0);

            var hostConnector = WebApiTestHelper.LoginHost();
            response = hostConnector.GetContent($"/Activity-Feed/userId/{userId}").Content.ReadAsStringAsync().Result;
            Assert.AreEqual(response.IndexOf(journalText), -1);
        }

        #endregion

        #region Private Methods

        private IWebApiConnector PrepareNewUser(out int userId, out string username, out int fileId)
        {
            username = $"testuser{DateTime.Now.Ticks}";
            var email = $"{username}@dnn.com";

            WebApiTestHelper.Register(username, _hostPass, username, email);

            userId = DatabaseHelper.ExecuteScalar<int>($"SELECT UserId FROM {{objectQualifier}}Users WHERE Username = '{username}'");

            var connector = WebApiTestHelper.LoginHost();

            var url = $"/API/PersonaBar/Users/UpdateAuthorizeStatus?userId={userId}&authorized=true";
            connector.PostJson(url, new {});
            connector.Logout();

            var userConnector = WebApiTestHelper.LoginUser(username);
            userConnector.UploadUserFile("Files\\Test.png", true, userId);

            fileId = DatabaseHelper.ExecuteScalar<int>($"SELECT MAX(FileId) FROM {{objectQualifier}}Files WHERE FileName = 'Test.png' AND CreatedByUserID = {userId} AND PortalId = {PortalId}");

            return userConnector;
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
                isSystem = false
            });

            return DatabaseHelper.ExecuteScalar<int>($"SELECT RoleId FROM {{objectQualifier}}Roles WHERE RoleName = '{roleName}' AND PortalId = {PortalId}");
        }

        private void AddUserToGroup(int groupId, int userId)
        {
            var connector = WebApiTestHelper.LoginHost();

            var url = "/API/PersonaBar/Roles/AddUserToRole?notifyUser=true&isOwner=false";
            connector.PostJson(url, new
            {
                userId = userId,
                roleId = groupId,
                isAdd = true
            });
        }

        private IDictionary<string, string> GetRequestHeaders(string moduleName = "Journal")
        {
            var tabId = DatabaseHelper.ExecuteScalar<int>($"SELECT * FROM {{objectQualifier}}Tabs WHERE TabPath = '//ActivityFeed' AND PortalId = {PortalId}");
            var moduleId =
                DatabaseHelper.ExecuteScalar<int>(
                    $@"
SELECT TOP 1 m.ModuleID FROM {{objectQualifier}}TabModules tm
	INNER JOIN {{objectQualifier}}modules m ON m.ModuleID = tm.ModuleID
	INNER JOIN {{objectQualifier}}ModuleDefinitions md ON md.ModuleDefID = m.ModuleDefID
WHERE tm.TabID = {tabId} AND md.FriendlyName = '{moduleName}'
");
            return new Dictionary<string, string>
            {
                {"TabId", tabId.ToString()},
                {"ModuleId", moduleId.ToString()}
            };
        }

        #endregion
    }
}
