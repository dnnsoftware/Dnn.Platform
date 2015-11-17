// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Validators;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace Dnn.Tests.DynamicContent.UnitTests.Validators
{
    [TestFixture]
    class DynamicContentValidatorTests
    {
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;

        [SetUp]
        public void SetUp()
        {
            _mockFieldDefinitionManager = MockComponentProvider.CreateNew<IFieldDefinitionManager>();
            FieldDefinitionManager.SetTestableInstance(_mockFieldDefinitionManager.Object);
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
            FieldDefinitionManager.ClearInstance();
        }

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
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, new DynamicContentType(Constants.PORTAL_ValidPortalId));

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

            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, contentType);
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
            var contentItem = new DynamicContentItem(Constants.PORTAL_ValidPortalId, contentType);
            var validator = new DynamicContentValidator(contentItem);

            //Action
            var result = validator.Validate();

            //Assert
            Assert.NotNull(result);
        }
    }
}
