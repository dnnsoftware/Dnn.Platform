// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Collections
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Collections;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class PagingExtensionsTests
    {
        [Test]
        public void PagingExtensions_InPagesOf_Returns_PageSelector()
        {
            // Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();

            // Act
            PageSelector<int> pageSelector = queryable.InPagesOf(Constants.PAGE_RecordCount);

            // Assert
            Assert.IsInstanceOf<PageSelector<int>>(pageSelector);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable(int index, int pageSize)
        {
            // Arrange
            List<int> enumerable = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            // Act
            IPagedList<int> pagedList = enumerable.ToPagedList(index, pageSize);

            // Assert
            Assert.IsInstanceOf<IPagedList<int>>(pagedList);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable_With_Correct_Index_AndPageSize(
            int index, int pageSize)
        {
            // Arrange
            List<int> enumerable = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            // Act
            IPagedList<int> pagedList = enumerable.ToPagedList(index, pageSize);

            // Assert
            Assert.AreEqual(index, pagedList.PageIndex);
            Assert.AreEqual(pageSize, pagedList.PageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable(int index, int pageSize)
        {
            // Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();

            // Act
            IPagedList<int> pagedList = queryable.ToPagedList(index, pageSize);

            // Assert
            Assert.IsInstanceOf<IPagedList<int>>(pagedList);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Queryable_With_Correct_Index_AndPageSize(
            int index, int pageSize)
        {
            // Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();

            // Act
            IPagedList<int> pagedList = queryable.ToPagedList(index, pageSize);

            // Assert
            Assert.AreEqual(index, pagedList.PageIndex);
            Assert.AreEqual(pageSize, pagedList.PageSize);
        }
    }
}
