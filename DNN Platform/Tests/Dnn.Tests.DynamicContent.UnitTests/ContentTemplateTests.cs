// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Dnn.DynamicContent;
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
            Assert.AreEqual(-1, template.TemplateId);
            Assert.AreEqual(-1, template.ContentTypeId);
            Assert.AreEqual(-1, template.TemplateFileId);
            Assert.AreEqual(String.Empty, template.Name);
        }
    }
}
