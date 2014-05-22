#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections.Generic;

using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Controllers.Search
{
	/// <summary>
    ///  Testing various aspects of SearchController
	/// </summary>
	[TestFixture]
	public class InternalSearchControllerTests
    {

        #region constants

	    private const int PortalId0 = 0;
        private const int PortalId1 = 1;
        private const int OtherSearchTypeId = 2;
        private const string TermDNN = "DNN";
        private const string TermDotNetNuke = "DotnetNuke";
        private const string TermLaptop = "Laptop";
        private const string TermNotebook = "Notebook";
        private const string TermJump = "Jump";
        private const string TermLeap = "Leap";
        private const string TermHop = "Hop";

        #endregion 

        #region Private Properties

        private Mock<DataProvider> _dataProvider;
        private Mock<ISearchHelper> _mockSearchHelper;
        private Mock<CachingProvider> _cachingProvider;
        private Mock<IHostController> _mockHostController;

        private IInternalSearchController _internalSearchController;

		#endregion

		#region Set Up

		[SetUp]
		public void SetUp()
		{
            ComponentFactory.Container = new SimpleContainer();
            _cachingProvider = MockComponentProvider.CreateDataCacheProvider();
            _dataProvider = MockComponentProvider.CreateDataProvider();

            _mockSearchHelper = new Mock<ISearchHelper>();
            _mockSearchHelper.Setup(c => c.GetSynonymsGroups(It.IsAny<int>(),It.IsAny<string>())).Returns(GetSynonymsGroupsCallBack);
            _mockSearchHelper.Setup(x => x.GetSearchTypeByName(It.IsAny<string>()))
                              .Returns((string name) => new SearchType { SearchTypeId = 0, SearchTypeName = name });
            SearchHelper.SetTestableInstance(_mockSearchHelper.Object);

            _mockHostController = new Mock<IHostController>();
            _mockHostController.Setup(c => c.GetInteger(Constants.SearchTitleBoostSetting,
                Constants.DefaultSearchTitleBoost)).Returns(Constants.StandardLuceneBoost);
            HostController.RegisterInstance(_mockHostController.Object);

            _internalSearchController = new InternalSearchControllerImpl();

            SetupDataProvider();
        }



	    [TearDown]
        public void TearDown()
        {
            SearchHelper.ClearInstance();
        }

		#endregion

        #region Private Methods

        private void SetupDataProvider()
        {
            //Standard DataProvider Path for Logging
            _dataProvider.Setup(d => d.GetProviderPath()).Returns("");
        }

        private IList<SynonymsGroup> GetSynonymsGroupsCallBack()
        {
            var groups = new List<SynonymsGroup>
                {
                    new SynonymsGroup {PortalId = 0, SynonymsGroupId = 1, SynonymsTags = string.Join(",", TermDNN, TermDotNetNuke)},
                    new SynonymsGroup {PortalId = 0, SynonymsGroupId = 2, SynonymsTags = string.Join(",", TermLaptop, TermNotebook)},
                    new SynonymsGroup {PortalId = 0, SynonymsGroupId = 3, SynonymsTags = string.Join(",", TermJump, TermLeap, TermHop)}
                };

            return groups;
        }

      

        #endregion

        #region Add Tests

        [Test]
        public void SearchController_Add_Throws_On_Null_SearchDocument()
        {
            //Arrange            

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => _internalSearchController.AddSearchDocument(null));
        }

        [Test]
        public void SearchController_Add_Throws_On_Null_Or_Empty_UniqueuKey()
        {
            //Arrange            

            //Act, Assert
            Assert.Throws<ArgumentException>(() => _internalSearchController.AddSearchDocument(new SearchDocument()));
        }

        [Test]
        public void SearchController_Add_Throws_On_Null_OrEmpty_Title()
        {
            //Arrange            

            //Act, Assert
            Assert.Throws<ArgumentException>(() => _internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString() }));
        }


        [Test]
        public void SearchController_AddSearchDcoumets_Does_Not_Throw_On_Null_OrEmpty_Title()
        {
            //Arrange            
            var documents = new List<SearchDocument> {new SearchDocument {UniqueKey = Guid.NewGuid().ToString()}};

            //Act, Assert
            _internalSearchController.AddSearchDocuments(documents);
        }

        [Test]
        public void SearchController_AddSearchDcoumets_Does_Not_Throw_On_Empty_Search_Document()
        {
            //Arrange            
            var documents = new List<SearchDocument> { new SearchDocument () };

            //Act, Assert
            _internalSearchController.AddSearchDocuments(documents);
        }

        [Test]
        public void SearchController_Add_Throws_On_Zero_SearchTypeId()
        {
            //Arrange            

            //Act, Assert
            Assert.Throws<ArgumentException>(() => _internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString() }));
        }

        [Test]
        public void SearchController_Add_Throws_On_Negative_SearchTypeId()
        {
            //Arrange            

            //Act, Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "title", SearchTypeId = -1 }));
        }

        [Test]
        public void SearchController_Add_Throws_On_DateTimeMin_ModifiedTimeUtc()
        {
            //Arrange            

            //Act, Assert
            Assert.Throws<ArgumentException>(() => _internalSearchController.AddSearchDocument(new SearchDocument { UniqueKey = Guid.NewGuid().ToString(), Title = "title", SearchTypeId = 1 }));
        }

        #endregion

        #region Delete Tests

        [Test]
        public void SearchController_Delete_Throws_On_Null_Or_Empty_UniqueuKey()
        {
            //Arrange            

            //Act, Assert
            var searchDoc = new SearchDocument() { UniqueKey = null, PortalId = 0, SearchTypeId = 1 };
            Assert.Throws<ArgumentException>(() => _internalSearchController.DeleteSearchDocument(searchDoc));
        }

        [Test]
        public void SearchController_Delete_Throws_On_Zero_SearchTypeId()
        {
            //Arrange            

            //Act, Assert
            var searchDoc = new SearchDocument() { UniqueKey = "key", PortalId = 0, SearchTypeId = 0 };
            Assert.Throws<ArgumentException>(() => _internalSearchController.DeleteSearchDocument(searchDoc));
        }

        [Test]
        public void SearchController_Delete_Throws_On_Negative_SearchTypeId()
        {
            //Arrange            

            //Act, Assert
            var searchDoc = new SearchDocument() { UniqueKey = "key", PortalId = 0, SearchTypeId = -1 };
            Assert.Throws<ArgumentOutOfRangeException>(() => _internalSearchController.DeleteSearchDocument(searchDoc));
        }

        #endregion

    }
}

