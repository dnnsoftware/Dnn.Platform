// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Search
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Search.Internals;
    using NUnit.Framework;

    [TestFixture]
    public class SearchQueryStringParserTests
    {
        public void GetLastModifiedDate_ShouldReturnOneDayAgoDate_WhenKeywordsHaveAfterDay()
        {
            // Arrange
            const string keywords = "mysearch after:day";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(-1).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneWeekAgoDate_WhenKeywordsHaveAfterWeek()
        {
            // Arrange
            const string keywords = "mysearch after:week";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(DateTime.UtcNow.AddDays(-7).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneMonthAgoDate_WhenKeywordsHaveAfterMonth()
        {
            // Arrange
            const string keywords = "mysearch after:month";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(DateTime.UtcNow.AddMonths(-1).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneQuarterAgoDate_WhenKeywordsHaveAfterQuarter()
        {
            // Arrange
            const string keywords = "mysearch after:quarter";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(DateTime.UtcNow.AddMonths(-3).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnOneYearAgoDate_WhenKeywordsHaveAfterYear()
        {
            // Arrange
            const string keywords = "mysearch after:year";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(DateTime.UtcNow.AddYears(-1).Date, date.Date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldReturnMinDate_WhenKeywordsDoNotHaveAfterParameter()
        {
            // Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            // Act
            var date = SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(DateTime.MinValue, date);
        }

        [Test]
        public void GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutAfterParameter_WhenKeywordsHaveAfterParameter()
        {
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] after:week ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetLastModifiedDate_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveAfterParameter()
        {
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetLastModifiedDate(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetSearchTypeList_ShouldReturnSearchTypeList_WhenKeywordsHaveTypeParameter()
        {
            // Arrange
            const string keywords = "mysearch type:Documents,Pages";
            var expectedSearchTypeList = new List<string> { "Documents", "Pages" };
            var outputKeywords = string.Empty;

            // Act
            var searchTypeList = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedSearchTypeList, searchTypeList);
        }

        [Test]
        public void GetSearchTypeList_ShouldReturnEmptySearchTypeList_WhenKeywordsDoNotHaveTypeParameter()
        {
            // Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            // Act
            var searchTypeList = SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(0, searchTypeList.Count);
        }

        [Test]
        public void GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTypeParameter_WhenKeywordsHaveTypeParameter()
        {
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] type:Documents ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetSearchTypeList_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTypeParameter()
        {
            // Arrange
            const string keywords = " mysearch [tag1][tag2][tag3] ";
            const string expectedOutputKeywords = "mysearch [tag1][tag2][tag3]";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetSearchTypeList(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetTags_ShouldReturnTagsList_WhenKeywordsHaveTags()
        {
            // Arrange
            const string keywords = "mysearch [tag][tag2]";
            var expectedTagsList = new List<string> { "tag", "tag2" };
            var outputKeywords = string.Empty;

            // Act
            var tagsList = SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedTagsList, tagsList);
        }

        [Test]
        public void GetTags_ShouldReturnEmptyTagList_WhenKeywordsDoNotHaveTags()
        {
            // Arrange
            const string keywords = "mysearch ";
            var outputKeywords = string.Empty;

            // Act
            var tagsList = SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(0, tagsList.Count);
        }

        [Test]
        public void GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywordsWithoutTags_WhenKeywordsHaveTags()
        {
            // Arrange
            const string keywords = "mysearch [tag][tag2][tag3]";
            const string expectedOutputKeywords = "mysearch";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }

        [Test]
        public void GetTags_ShouldSetOutputKeywordsEqualsToTrimmedKeywords_WhenKeywordsDoNotHaveTags()
        {
            // Arrange
            const string keywords = " mysearch after:week ";
            const string expectedOutputKeywords = "mysearch after:week";
            var outputKeywords = string.Empty;

            // Act
            SearchQueryStringParser.Instance.GetTags(keywords, out outputKeywords);

            // Assert
            Assert.AreEqual(expectedOutputKeywords, outputKeywords);
        }
    }
}
