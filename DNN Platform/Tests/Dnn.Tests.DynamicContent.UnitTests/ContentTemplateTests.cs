// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Dnn.DynamicContent;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class ContentTemplateTests
    {
        [Test]
        public void Constructor_Sets_Default_Properties()
        {
            //Arrange

            //Act
            var template = new ContentTemplate();

            //Assert
            Assert.AreEqual(-1, template.PortalId);
            Assert.AreEqual(-1, template.TemplateId);
            Assert.AreEqual(-1, template.ContentTypeId);
            Assert.AreEqual(-1, template.TemplateFileId);
            Assert.AreEqual(String.Empty, template.Name);
        }

        [Test]
        public void Constructor_Sets_PortalId_Property()
        {
            //Arrange

            //Act
            var dataType = new ContentTemplate(Constants.CONTENT_ValidPortalId);

            //Assert
            Assert.AreEqual(Constants.CONTENT_ValidPortalId, dataType.PortalId);
        }

        [TestCase(-1, true)]
        [TestCase(0, false)]
        public void IsSystem_Returns_Correct_Value(int portalId, bool isSystem)
        {
            //Arrange

            //Act
            var dataType = new ContentTemplate(portalId);

            //Assert
            Assert.AreEqual(isSystem, dataType.IsSystem);
        }

    }
}
