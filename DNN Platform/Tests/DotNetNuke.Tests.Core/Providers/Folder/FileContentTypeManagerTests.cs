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
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using DotNetNuke.Common.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.FileSystem.Internal;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.Security.Permissions;
using Moq;

using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Folder
{
    [TestFixture]
    public class FileContentTypeManagerTests
    {
        #region Private Variables


        #endregion

        #region Setup & TearDown

        [SetUp]
        public void Setup()
        {
			var _mockData = MockComponentProvider.CreateDataProvider();
			var _mockCache = MockComponentProvider.CreateDataCacheProvider();
			var _globals = new Mock<IGlobals>();
			var _cbo = new Mock<ICBO>();

			_mockData.Setup(m => m.GetProviderPath()).Returns(String.Empty);

			TestableGlobals.SetTestableInstance(_globals.Object);
			CBO.SetTestableInstance(_cbo.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TestableGlobals.ClearInstance();
            CBO.ClearInstance();
        }

        #endregion

        #region GetContentType

        [Test]
        public void GetContentType_Returns_Known_Value_When_Extension_Is_Not_Managed()
        {
            const string notManagedExtension = "asdf609vas21AS:F,l/&%/(%$";

            var contentType = FileContentTypeManager.Instance.GetContentType(notManagedExtension);

            Assert.AreEqual("application/octet-stream", contentType);
        }

		[Test]
		public void GetContentType_Returns_Correct_Value_For_Extension()
		{
			const string notManagedExtension = "htm";

			var contentType = FileContentTypeManager.Instance.GetContentType(notManagedExtension);

			Assert.AreEqual("text/html", contentType);
		}

        #endregion
    }
}
