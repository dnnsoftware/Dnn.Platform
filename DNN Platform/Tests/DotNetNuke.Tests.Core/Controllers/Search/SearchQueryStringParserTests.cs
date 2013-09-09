#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Services.Search.Internals;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    [TestFixture]
    public class SearchQueryStringParserTests
    {
        #region GetLastModifiedDate Method Tests
        public void GetLastModifiedDate_ShouldReturnOneDayAgoDate_WhenKeywordsHaveAfterDay()
        {
            //Arrange
            const string keywords = "mysearch after:day";
            var outputKeywords = string.Empty;

            //Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(-1).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneWeekAgoDate_WhenKeywordsHaveAfterWeek()
        {
            //Arrange
            const string keywords = "mysearch after:week";
            var outputKeywords = string.Empty;

            //Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(-7).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneMonthAgoDate_WhenKeywordsHaveAfterMonth()
        {
            //Arrange
            const string keywords = "mysearch after:month";
            var outputKeywords = string.Empty;

            //Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddMonths(-1).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneQuarterAgoDate_WhenKeywordsHaveAfterQuarter()
        {
            //Arrange
            const string keywords = "mysearch after:quarter";
            var outputKeywords = string.Empty;

            //Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddMonths(-3).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneYearAgoDate_WhenKeywordsHaveAfterYear()
        {
            //Arrange
            const string keywords = "mysearch after:year";
            var outputKeywords = string.Empty;

            //Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(DateTime.UtcNow.AddYears(-1).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnMinDate_WhenKeywordsDoNotHaveAfterParameter()
        {
            //Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            //Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(DateTime.MinValue, date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutAfterParameter_WhenKeywordsHaveAfterParameter()
        {
            //Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] after:week ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            //Act
            SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveAfterParameter()
        {
            //Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            //Act
            SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }
        #endregion

        #region GetSearchTypeList Method Tests
        [Test]
        public void GetSearchTypeList_ShouldReturnSearchTypeList_WhenKeywordsHaveTypeParameter()
        {
            //Arrange
            const string keywords = "mysearch type:Documents,Pages";
            var expectedSearchTypeList = new List<string> { "Documents", "Pages" };
            var outputKeywords = string.Empty;

            //Act
            var searchTypeList = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedSearchTypeList, searchTypeList);
        }

        [Test]
        public void GetSearchTypeList_ShouldReturnEmptySearchTypeList_WhenKeywordsDoNotHaveTypeParameter()
        {
            //Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            //Act
            var searchTypeList = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(0, searchTypeList.Count);
        }

        [Test]
        public void GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTypeParameter_WhenKeywordsHaveTypeParameter()
        {
            //Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] type:Documents ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            //Act
            SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTypeParameter()
        {
            //Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            //Act
            SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }
        #endregion

        #region GetTags Method Tests
        [Test]
        public void GetTags_ShouldReturnTagsList_WhenKeywordsHaveTags()
        {
            //Arrange
            const string keywords = "mysearch [tag][tag2]";
            var expectedTagsList = new List<string> { "tag", "tag2" };
            var outputKeywords = string.Empty;

            //Act
            var tagsList = SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedTagsList, tagsList);
        }

        [Test]
        public void GetTags_ShouldReturnEmptyTagList_WhenKeywordsDoNotHaveTags()
        {
            //Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            //Act
            var tagsList = SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(0, tagsList.Count);
        }

        [Test]
        public void GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTags_WhenKeywordsHaveTags()
        {
            //Arrange
            const string keywords = "mysearch [tag][tag2][tag3]";
            const string expectedOutputKeywords = "mysearch";
            var outputKeywords = string.Empty;

            //Act
            SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTags()
        {
            //Arrange
            const string keywords = " mysearch after:week ";
            const string expectedOutputKeywords = "mysearch after:week";
            var outputKeywords = string.Empty;

            //Act
            SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            //Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }
        #endregion
    }
}
