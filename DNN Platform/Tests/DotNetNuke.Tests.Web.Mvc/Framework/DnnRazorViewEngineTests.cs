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
using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Mvc.Framework
{
    [TestFixture]
    public class DnnRazorViewEngineTests
    {
        [Test]
        public void FindPartialView_Throws_Exception_If_Provided_ControlContext_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = "ViewName";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentNullException>(() => viewEngine.FindPartialView(null, partialViewName, useCache));
        }

        [Test]
        public void FindPartialView_Throws_Exception_If_Provided_ViewName_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = null;
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindPartialView(new ControllerContext(), partialViewName, useCache));
        }

        [Test]
        public void FindPartialView_Throws_Exception_If_Provided_ViewName_Is_EmptyString()
        {
            const bool useCache = true;
            string partialViewName = String.Empty;
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindPartialView(new ControllerContext(), partialViewName, useCache));
        }

        [Test]
        public void FindView_Throws_Exception_If_Provided_ControlContext_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = "ViewName";
            const string masterViewName = "MasterView";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentNullException>(() => viewEngine.FindView(null, partialViewName, masterViewName, useCache));
        }

        [Test]
        public void FindView_Throws_Exception_If_Provided_ViewName_IsNull()
        {
            const bool useCache = true;
            const string partialViewName = null;
            const string masterViewName = "MasterView";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindView(new ControllerContext(), partialViewName, masterViewName, useCache));
        }

        [Test]
        public void FindView_Throws_Exception_If_Provided_ViewName_Is_EmptyString()
        {
            const bool useCache = true;
            string partialViewName = String.Empty;
            const string masterViewName = "MasterView";
            var viewEngine = new DnnRazorViewEngine();

            Assert.Throws<ArgumentException>(() => viewEngine.FindView(new ControllerContext(), partialViewName, masterViewName, useCache));
        }
    }
}
