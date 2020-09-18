// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using System.Data;

    using Dnn.PersonaBar.Users.Components;
    using Dnn.PersonaBar.Users.Components.Contracts;
    using Dnn.PersonaBar.Users.Components.Dto;
    using Dnn.PersonaBar.Users.Components.Helpers;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class SearchUsersBySearchTermTest
    {
        private GetUsersContract usersContract;
        private UsersControllerTestable usersCtrl;

        [SetUp]
        public void Init()
        {
            this.usersContract = new GetUsersContract
            {
                SearchText = null,
                PageIndex = 0,
                PageSize = 10,
                SortColumn = "displayname",
                SortAscending = true,
                PortalId = 0,
                Filter = UserFilters.All,
            };

            this.usersCtrl = new UsersControllerTestable();
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("", null)]
        [TestCase("search_text", "search_text%")]
        [TestCase("*search_text", "%search_text%")]
        [TestCase("%search_text", "%search_text%")]
        [TestCase("search_text%", "search_text%")]
        [TestCase("search_text*", "search_text%")]
        [TestCase("*search_text*", "%search_text%")]
        [TestCase("%search_text%", "%search_text%")]
        [TestCase("*search_text%", "%search_text%")]
        [TestCase("%search_text*", "%search_text%")]
        [TestCase("*search*_text*", "%search_text%")]
        [TestCase("*search**_text*", "%search_text%")]
        [TestCase("*search*%_text*", "%search_text%")]
        [TestCase("*search%_text*", "%search_text%")]
        [TestCase("search text", "search text%")]
        public void FilteredSearchTest(string searchText, string expectedFilteredText)
        {
            int totalRecords;
            this.usersContract.SearchText = searchText;
            this.usersCtrl.GetUsers(this.usersContract, true, out totalRecords);

            Assert.AreEqual(expectedFilteredText, this.usersCtrl.LastSearch);
        }

        private class UsersControllerTestable : UsersController
        {
            private Mock<IDataReader> dataReader;

            public string LastSearch { get; set; }

            protected override IDataReader CallGetUsersBySearchTerm(
                GetUsersContract usersContract,
                bool? includeAuthorized,
                bool? includeDeleted,
                bool? includeSuperUsers,
                bool? hasAgreedToTerms,
                bool? requestsRemoval)
            {
                this.LastSearch = SearchTextFilter.CleanWildcards(usersContract.SearchText);
                this.dataReader = new Mock<IDataReader>();
                return this.dataReader.Object;
            }
        }
    }
}
