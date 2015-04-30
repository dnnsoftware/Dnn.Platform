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
using Dnn.DynamicContent;
using Dnn.DynamicContent.Validators;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests.Validators
{
    [TestFixture]
    class DynamicContentValidatorTests
    {
        [Test]
        public void Constructor_Throws_on_Null_ContentItem()
        {
            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => new DynamicContentValidator(null));
        }

        [Test]
        public void Constructor_Sets_ContentItem_Property()
        {
            //Arrange
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId);

            //Act
            var validator = new DynamicContentValidator(contentItem);

            //Assert
            Assert.AreSame(contentItem, validator.ContentItem);
        }

        [Test]
        public void Validate_Returns_ValidationResult()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var contentType = new DynamicContentType
                                        {
                                            ContentTypeId = contentTypeId,
                                            PortalId = Constants.PORTAL_ValidPortalId
                                        };

            var contentItem = new DynamicContentItem(contentType);
            var validator = new DynamicContentValidator(contentItem);

            //Action
            var result = validator.Validate();

            //Assert
            Assert.NotNull(result);
        }

        [Test]
        public void Validate_Returns_ValidationResult_With_IsValid_True_If_No_Validation_Rules()
        {
            //Arrange
            var contentTypeId = Constants.CONTENTTYPE_ValidContentTypeId;

            var fieldDefinition = new FieldDefinition()
                                        {
                                            ContentTypeId = contentTypeId,
                                            Name = "Name"
                                        };

            var contentType = new DynamicContentType {PortalId = Constants.PORTAL_ValidPortalId};
            contentType.FieldDefinitions.Add(fieldDefinition);
            var contentItem = new DynamicContentItem(contentType);
            var validator = new DynamicContentValidator(contentItem);

            //Action
            var result = validator.Validate();

            //Assert
            Assert.NotNull(result);
        }
    }
}
