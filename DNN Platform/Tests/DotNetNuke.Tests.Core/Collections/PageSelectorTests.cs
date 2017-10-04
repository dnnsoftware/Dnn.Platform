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

#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Collections;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;

#endregion

namespace DotNetNuke.Tests.Core.Collections
{
    [TestFixture]
    public class PageSelectorTests
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            list = Util.CreateIntegerList(Constants.PAGE_TotalCount);
        }

        #endregion

        private IEnumerable<int> list;

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(Constants.PAGE_Last)]
        public void PageSelector_Returns_CorrectPage_When_Given_Valid_Index(int index)
        {
            //Arrange
            var selector = new PageSelector<int>(list, Constants.PAGE_RecordCount);

            //Act
            IPagedList<int> pagedList = selector.GetPage(index);

            //Assert
            Assert.AreEqual(index, pagedList.PageIndex);
        }

        [Test]
        [TestCase(5)]
        [TestCase(8)]
        public void PageSelector_Returns_Correct_RecordCount_When_Given_Valid_Index(int pageSize)
        {
            //Arrange
            var selector = new PageSelector<int>(list, pageSize);

            //Act
            IPagedList<int> pagedList = selector.GetPage(Constants.PAGE_First);

            //Assert
            Assert.AreEqual(pageSize, pagedList.PageSize);
        }

        [Test]
        [TestCase(0, 5)]
        [TestCase(0, 6)]
        [TestCase(2, 4)]
        [TestCase(4, 4)]
        public void PageSelector_Returns_Correct_Values_When_Given_Valid_Index_And_PageSize(int index, int pageSize)
        {
            //Arrange
            var selector = new PageSelector<int>(list, pageSize);

            //Act
            IPagedList<int> pagedList = selector.GetPage(index);

            //Assert
            for (int i = 0; i < pageSize; i++)
            {
                Assert.AreEqual(index*pageSize + i, pagedList[i]);
            }
        }

        [Test]
        public void PageSelector_Throws_When_Given_InValid_Index()
        {
            //Arrange
            var selector = new PageSelector<int>(list, Constants.PAGE_RecordCount);

            //Assert
            Assert.Throws<IndexOutOfRangeException>(() => selector.GetPage(Constants.PAGE_OutOfRange));
        }

        [Test]
        public void PageSelector_Throws_When_Given_Negative_Index()
        {
            //Arrange
            var selector = new PageSelector<int>(list, Constants.PAGE_RecordCount);

            //Assert
            Assert.Throws<IndexOutOfRangeException>(() => selector.GetPage(Constants.PAGE_NegativeIndex));
        }
    }
}