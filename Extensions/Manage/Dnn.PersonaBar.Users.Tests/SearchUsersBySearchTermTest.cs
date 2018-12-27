using Dnn.PersonaBar.Users.Components;
using Dnn.PersonaBar.Users.Components.Contracts;
using Dnn.PersonaBar.Users.Components.Dto;
using Dnn.PersonaBar.Users.Components.Helpers;
using Moq;
using NUnit.Framework;
using System.Data;

namespace Dnn.PersonaBar.Users.Tests
{
    [TestFixture]
    public class SearchUsersBySearchTermTest
    {

        private GetUsersContract usersContract;
        private UsersControllerTestable usersCtrl;

        [SetUp]
        public void Init()
        {
            usersContract = new GetUsersContract
            {
                SearchText = null,
                PageIndex = 0,
                PageSize = 10,
                SortColumn = "displayname",
                SortAscending = true,
                PortalId = 0,
                Filter = UserFilters.All
            };

            usersCtrl = new UsersControllerTestable();
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
            usersContract.SearchText = searchText;
            usersCtrl.GetUsers(usersContract, true, out totalRecords);

            Assert.AreEqual(expectedFilteredText, usersCtrl.LastSearch);

        }

        private class UsersControllerTestable : UsersController
        {
            private Mock<IDataReader> dataReader;
            
            public string LastSearch { get; set; }

            protected override IDataReader CallGetUsersBySearchTerm(
                GetUsersContract usersContract,
                bool? includeAuthorized,
                bool? includeDeleted,
                bool? includeSuperUsers)
            {
                LastSearch = SearchTextFilter.CleanWildcards(usersContract.SearchText);
                dataReader = new Mock<IDataReader>();
                return dataReader.Object;
            }
        }
    }
}