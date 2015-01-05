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
using System.Web;
using DotNetNuke.Web.Mvc.Framework;
using DotNetNuke.Web.Mvc.Helpers;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Helpers
{
    [TestFixture]
    public class HttpContextBaseExtensionsTests
    {
        [Test]
        public void HasSiteContext_Returns_False_If_No_SiteContext_Key_Exists()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            // Act and Assert
            Assert.IsFalse(context.HasSiteContext());
        }

        [Test]
        public void HasSiteContext_Returns_False_If_SiteContext_Key_Is_Null()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            context.Items.Add(HttpContextBaseExtensions.GetKeyFor<SiteContext>(), null);

            // Act and Assert
            Assert.IsFalse(context.HasSiteContext());
        }


        [Test]
        public void HasSiteContext_Returns_True_If_SiteContext_Key_Exists()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var expected = new SiteContext(context);
            context.Items.Add(HttpContextBaseExtensions.GetKeyFor<SiteContext>(), expected);

            // Act and Assert
            Assert.IsTrue(context.HasSiteContext());
        }

        [Test]
        public void GetSiteContext_Returns_Null_SiteRequestContext_If_None_Present()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();

            // Act and Assert
            Assert.IsNull(context.GetSiteContext());
        }

        [Test]
        public void GetSiteContext_Returns_Stored_SiteRequestContext_If_Present()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var expected = new SiteContext(context);
            context.Items.Add(HttpContextBaseExtensions.GetKeyFor<SiteContext>(), expected);

            // Act
            var actual = context.GetSiteContext();

            // Assert
            Assert.AreSame(expected, actual);
        }

        [Test]
        public void SetSiteContext_Sets_SiteRequestContext()
        {
            // Arrange
            HttpContextBase context = MockHelper.CreateMockHttpContext();
            var siteContext = new SiteContext(context);

            // Act
            context.SetSiteContext(siteContext);

            // Assert
            var actual = context.Items[HttpContextBaseExtensions.GetKeyFor<SiteContext>()];
            Assert.IsNotNull(actual);
            Assert.IsInstanceOf<SiteContext>(actual);
        }

        [Test]
        public void GetKeyFor_Prefixes_Full_TypeName_With_DnnMvc_Prefix()
        {
            // Assert
            Assert.AreEqual(String.Format("DnnMvc:{0}", typeof(Version).FullName), HttpContextBaseExtensions.GetKeyFor<Version>());
        }

    }
}
