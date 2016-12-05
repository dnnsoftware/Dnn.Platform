﻿#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Collections;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;

#endregion

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class PagedListTests
    {
        [Test]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        [TestCase(10, 1)]
        [TestCase(12, 2)]
        public void PagedList_Constructor_Succeeds_When_Given_Valid_Index(int totalCount, int pageIndex)
        {
            //Arrange
            IQueryable<int> list = Util.CreateIntegerList(totalCount).AsQueryable();

            //Act
            new PagedList<int>(list, pageIndex, Constants.PAGE_RecordCount);

            //Assert
            //asserted by no exception :)
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(10, 0)]
        [TestCase(10, 1)]
        [TestCase(12, 2)]
        public void PagedList_Constructor_Overload_Succeeds_When_Given_Valid_Index(int totalCount, int pageIndex)
        {
            //Arrange
            IQueryable<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount).AsQueryable();

            //Act
            new PagedList<int>(list, totalCount, pageIndex, Constants.PAGE_RecordCount);

            //Assert
            //asserted by no exception :)
        }


        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Throws_When_Given_InValid_Index(int totalCount, int pageIndex)
        {
            //Arrange
            IQueryable<int> list = Util.CreateIntegerList(totalCount).AsQueryable();

            Assert.Throws<IndexOutOfRangeException>(() => new PagedList<int>(list, pageIndex, Constants.PAGE_RecordCount));
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Overload_Throws_When_Given_InValid_Index(int totalCount, int pageIndex)
        {
            //Arrange
            IQueryable<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount).AsQueryable();

            Assert.Throws<IndexOutOfRangeException>(() => new PagedList<int>(list, totalCount, pageIndex, Constants.PAGE_RecordCount));
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Throws_When_Given_Invalid_PageSize(int totalCount, int pageIndex)
        {
            //Arrange
            IQueryable<int> list = Util.CreateIntegerList(totalCount).AsQueryable();

            Assert.Throws<IndexOutOfRangeException>(() => new PagedList<int>(list, pageIndex, -1));
        }

        [Test]
        [TestCase(0, 1)]
        [TestCase(5, 1)]
        [TestCase(7, 2)]
        public void PagedList_Constructor_Overload_Throws_When_Given_Invalid_PageSize(int totalCount, int pageIndex)
        {
            //Arrange
            IQueryable<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount).AsQueryable();

            Assert.Throws<IndexOutOfRangeException>(() => new PagedList<int>(list, totalCount, pageIndex, -1));
        }
        
        [Test]
        public void PagedList_Constructor_Throws_When_Given_Negative_Index()
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act, Assert
            Assert.Throws<IndexOutOfRangeException>(() => new PagedList<int>(list, Constants.PAGE_NegativeIndex, Constants.PAGE_RecordCount));
        }

        [Test]
        public void PagedList_Constructor_Overload_Throws_When_Given_Negative_Index()
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_RecordCount);

            //Act, Assert
            Assert.Throws<IndexOutOfRangeException>(() => new PagedList<int>(list, Constants.PAGE_TotalCount, Constants.PAGE_NegativeIndex, Constants.PAGE_RecordCount));
        }

        [Test]
        [TestCase(0, true)]
        [TestCase(1, true)]
        [TestCase(Constants.PAGE_Last, false)]
        public void PagedList_HasNextPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool hasNext)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

            //Assert
            Assert.AreEqual(hasNext, pagedList.HasNextPage);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(Constants.PAGE_Last, true)]
        public void PagedList_HasPreviousPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool hasPrevious)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

            //Assert
            Assert.AreEqual(hasPrevious, pagedList.HasPreviousPage);
        }

        [Test]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(Constants.PAGE_Last, false)]
        public void PagedList_IsFirstPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool isFirst)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

            //Assert
            Assert.AreEqual(isFirst, pagedList.IsFirstPage);
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(Constants.PAGE_Last, true)]
        public void PagedList_IsLastPage_Has_Correct_Value_When_Given_Valid_Index(int index, bool isLast)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

            //Assert
            Assert.AreEqual(isLast, pagedList.IsLastPage);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(Constants.PAGE_Last)]
        public void PagedList_Returns_Correct_Page_When_Given_Valid_Index(int index)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, Constants.PAGE_RecordCount);

            //Assert
            Assert.AreEqual(index, pagedList.PageIndex);
        }

        [Test]
        [TestCase(5)]
        [TestCase(8)]
        public void PagedList_Returns_Correct_RecordCount_When_Given_Valid_Index(int pageSize)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, Constants.PAGE_First, pageSize);

            //Assert
            Assert.AreEqual(pageSize, pagedList.PageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagedList_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(int index, int pageSize)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, pageSize);

            //Assert
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(index*pageSize + i, pagedList[i]);
            }
        }

        [Test]
        [TestCase(0, 5, 5)]
        [TestCase(0, 6, 4)]
        [TestCase(2, 4, 6)]
        [TestCase(1, 10, 3)]
        public void PagedList_Sets_Correct_PageCount(int index, int pageSize, int pageCount)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, pageSize);

            //Assert
            Assert.AreEqual(pageCount, pagedList.PageCount);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagedList_Sets_TotalCount_To_Total_Number_Of_Items(int index, int pageSize)
        {
            //Arrange
            List<int> list = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            var pagedList = new PagedList<int>(list, index, pageSize);

            //Assert
            Assert.AreEqual(Constants.PAGE_TotalCount, pagedList.TotalCount);
        }
    }
}