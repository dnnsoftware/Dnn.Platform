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

#region Usings

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Collections;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;

#endregion

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class PagingExtensionsTests
    {
        [Test]
        public void PagingExtensions_InPagesOf_Returns_PageSelector()
        {
            //Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();

            //Act
            PageSelector<int> pageSelector = queryable.InPagesOf(Constants.PAGE_RecordCount);

            //Assert
            Assert.IsInstanceOf<PageSelector<int>>(pageSelector);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PagingExtensions_ToPagedList_Returns_PagedList_From_Enumerable(int index, int pageSize)
        {
            //Arrange
            List<int> enumerable = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            IPagedList<int> pagedList = enumerable.ToPagedList(index, pageSize);

            //Assert
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
            //Arrange
            List<int> enumerable = Util.CreateIntegerList(Constants.PAGE_TotalCount);

            //Act
            IPagedList<int> pagedList = enumerable.ToPagedList(index, pageSize);

            //Assert
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
            //Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();

            //Act
            IPagedList<int> pagedList = queryable.ToPagedList(index, pageSize);

            //Assert
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
            //Arrange
            IQueryable<int> queryable = Util.CreateIntegerList(Constants.PAGE_TotalCount).AsQueryable();

            //Act
            IPagedList<int> pagedList = queryable.ToPagedList(index, pageSize);

            //Assert
            Assert.AreEqual(index, pagedList.PageIndex);
            Assert.AreEqual(pageSize, pagedList.PageSize);
        }
    }
}