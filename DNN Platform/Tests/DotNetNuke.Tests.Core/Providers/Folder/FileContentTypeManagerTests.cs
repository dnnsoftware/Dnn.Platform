// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Folder
{
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
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.FileSystem.Internal;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class FileContentTypeManagerTests
    {
        [SetUp]
        public void Setup()
        {
            var _mockData = MockComponentProvider.CreateDataProvider();
            var _mockCache = MockComponentProvider.CreateDataCacheProvider();
            var _globals = new Mock<IGlobals>();
            var _cbo = new Mock<ICBO>();

            _mockData.Setup(m => m.GetProviderPath()).Returns(string.Empty);

            TestableGlobals.SetTestableInstance(_globals.Object);
            CBO.SetTestableInstance(_cbo.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TestableGlobals.ClearInstance();
            CBO.ClearInstance();
        }

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
    }
}
