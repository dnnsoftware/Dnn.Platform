#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using NUnit.Framework;

namespace PbIntegrationTests.Manage.Dnn.PersonaBar.Users
{
    [TestFixture]
    public class UsersFiltersTests : IntegrationTestBase
    {

        private int[] _users = new int[10];
        private string[] _names = new string[10];

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            var hostUserName = AppConfigHelper.GetAppSetting("hostUserName");

            // clean up users
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}UserRelationships");
            DatabaseHelper.ExecuteNonQuery("DELETE FROM {objectQualifier}UserPortals");
            DatabaseHelper.ExecuteNonQuery($"DELETE FROM {{objectQualifier}}Users WHERE Username <> '{hostUserName}'");

            // create new users
            for (var i = 0; i < _names.Length; i++)
            {
                int user, file;
                string name;
                var connector = WebApiTestHelper.PrepareNewUser(out user, out name, out file);
                _users[i] = connector.UserId;
                _names[i] = connector.UserName;
                Console.WriteLine(@"Test users => {0}: {1}", connector.UserId, connector.UserName);
            }
        }

        [Test]
        public void GetUsersWithVariousFiltersShoudlReturnExpectedResult()
        {
            // Arrange
            // clear all existing users except superusers
            // create 10 users and unauthorize 2 and delete 1 then use this API
            // /API/PersonaBar/Users/GetUsers?searchText=&filter=0&pageIndex=0&pageSize=10&sortColumn=&sortAscending=false&resetIndex=true
            // with various parameters to validate the returned results.

            // After deleting all users the PrepareNewUser fails due to DNN-10547;
            // therefore, I couldn't complete writing the intended integration tests.

            Assert.Fail();
        }
    }
}