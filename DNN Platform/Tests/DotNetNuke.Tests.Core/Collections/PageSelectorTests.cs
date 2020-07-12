// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Collections;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PageSelectorTests
    {
        private IEnumerable<int> list;

        [SetUp]
        public void SetUp()
        {
            this.list = Util.CreateIntegerList(Constants.PAGE_TotalCount);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(Constants.PAGE_Last)]
        public void PageSelector_Returns_CorrectPage_When_Given_Valid_Index(int index)
        {
            // Arrange
            var selector = new PageSelector<int>(this.list, Constants.PAGE_RecordCount);

            // Act
            IPagedList<int> pagedList = selector.GetPage(index);

            // Assert
            Assert.AreEqual(index, pagedList.PageIndex);
        }

        [Test]
        [TestCase(5)]
        [TestCase(8)]
        public void PageSelector_Returns_Correct_RecordCount_When_Given_Valid_Index(int pageSize)
        {
            // Arrange
            var selector = new PageSelector<int>(this.list, pageSize);

            // Act
            IPagedList<int> pagedList = selector.GetPage(Constants.PAGE_First);

            // Assert
            Assert.AreEqual(pageSize, pagedList.PageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PageSelector_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(int index, int pageSize)
        {
            // Arrange
            var selector = new PageSelector<int>(this.list, pageSize);

            // Act
            IPagedList<int> pagedList = selector.GetPage(index);

            // Assert
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual((index * pageSize) + i, pagedList[i]);
            }
        }

        [Test]
        public void PageSelector_Throws_When_Given_InValid_Index()
        {
            // Arrange
            var selector = new PageSelector<int>(this.list, Constants.PAGE_RecordCount);

            // Assert
            Assert.Throws<IndexOutOfRangeException>(() => selector.GetPage(Constants.PAGE_OutOfRange));
        }

        [Test]
        public void PageSelector_Throws_When_Given_Negative_Index()
        {
            // Arrange
            var selector = new PageSelector<int>(this.list, Constants.PAGE_RecordCount);

            // Assert
            Assert.Throws<IndexOutOfRangeException>(() => selector.GetPage(Constants.PAGE_NegativeIndex));
        }
    }
}
