#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.PersonaBar.Manage.Users
{
    [TestFixture]
    public class UsersFiltersTests : IntegrationTestBase
    {
        private const string UnauthorizeApi = "/API/PersonaBar/Users/UpdateAuthorizeStatus?userId={0}&authorized=false";
        private const string DeleteApi = "/API/PersonaBar/Users/SoftDeleteUser?userId={0}";
        private const string MakeAdminApi = "/API/PersonaBar/Users/SaveUserRole?notifyUser=true&isOwner=false";

        private const int MaxUsers = 8;
        private int[] _userIds = new int[MaxUsers];
        private string[] _userNames = new string[MaxUsers];

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            // clear all existing users except the HOST superuser
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}UserRelationships");
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}UserPortals");
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}Users WHERE UserID > 1");

            // create MaxUsers new users
            for (var i = 0; i < MaxUsers; i++)
            {
                int userId, fileId;
                string userName;
                WebApiTestHelper.PrepareNewUser(out userId, out userName, out fileId);
                _userIds[i] = userId;
                _userNames[i] = userName;
                Console.WriteLine(@"Created test users => id: {0}, username: {1}", userId, userName);
            }

            var hostConnector = WebApiTestHelper.LoginHost();
            var userIdx = 0;

            // make first user as admin
            var makeAdminItem = new { RoleId = 0, UserId = _userIds[userIdx] };
            var response = hostConnector.PostJson(MakeAdminApi, makeAdminItem).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(response);
            Assert.AreEqual(_userNames[userIdx], result.displayName.ToString());

            // Unauthorize the next 2 new users
            for (userIdx = 1; userIdx <= 2; userIdx++)
            {
                var unauthorizeLink = string.Format(UnauthorizeApi, _userIds[userIdx]);
                response = hostConnector.PostJson(unauthorizeLink, "").Content.ReadAsStringAsync().Result;
                result = JsonConvert.DeserializeObject<dynamic>(response);
                Assert.IsTrue(bool.Parse(result.Success.ToString()));
            }

            // soft delete the next new user
            var deleteLink = string.Format(DeleteApi, _userIds[userIdx]);
            response = hostConnector.PostJson(deleteLink, "").Content.ReadAsStringAsync().Result;
            result = JsonConvert.DeserializeObject<dynamic>(response);
            Assert.IsTrue(bool.Parse(result.Success.ToString()));
        }

        [TestCase("All", MaxUsers, "/API/PersonaBar/Users/GetUsers?searchText=&filter=5&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Authorized", MaxUsers - 3, "/API/PersonaBar/Users/GetUsers?searchText=&filter=0&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Unauthorized", 2, "/API/PersonaBar/Users/GetUsers?searchText=&filter=1&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Deleted", 1, "/API/PersonaBar/Users/GetUsers?searchText=&filter=2&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Superusers", 1, "/API/PersonaBar/Users/GetUsers?searchText=&filter=3&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        public void GetUsersAsHostWithVariousFiltersShoudlReturnExpectedResult(string actionName, int expectedTotal, string apiMethod)
        {
            // Arrange: all is done in TestFixtureSetUp()

            // Act
            var hostConnector = WebApiTestHelper.LoginHost();
            var response = hostConnector.GetContent(apiMethod, null).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            // Assert
            var totalResults = int.Parse(result.TotalResults.ToString());
            Assert.AreEqual(expectedTotal, totalResults, $"Total results {totalResults} is incorrect for action [{actionName}]");
        }

        [TestCase("All", MaxUsers, "/API/PersonaBar/Users/GetUsers?searchText=&filter=5&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Authorized", MaxUsers - 3, "/API/PersonaBar/Users/GetUsers?searchText=&filter=0&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Unauthorized", 2, "/API/PersonaBar/Users/GetUsers?searchText=&filter=1&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Deleted", 1, "/API/PersonaBar/Users/GetUsers?searchText=&filter=2&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        [TestCase("Superusers", 0, "/API/PersonaBar/Users/GetUsers?searchText=&filter=3&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true")]
        public void GetUsersAsAdminWithVariousFiltersShoudlReturnExpectedResult(string actionName, int expectedTotal, string apiMethod)
        {
            // Arrange: all is done in TestFixtureSetUp()

            // Act
            var adminConnector = WebApiTestHelper.LoginUser(_userNames[0]);
            var response = adminConnector.GetContent(apiMethod, null).Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<dynamic>(response);

            // Assert
            var totalResults = int.Parse(result.TotalResults.ToString());
            Assert.AreEqual(expectedTotal, totalResults, $"Total results {totalResults} is incorrect for action [{actionName}]");
        }
    }
}