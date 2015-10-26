// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent;
using Dnn.Tests.DynamicContent.UnitTests.Builders;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Dnn.Tests.DynamicContent.UnitTests
{
    [TestFixture]
    public class FieldDefinitionCheckerTests
    {

        private MockRepository _mockRepository;
        private Mock<IFieldDefinitionManager> _mockFieldDefinitionManager;

        [SetUp]
        public void Setup()
        {
            //Mock Repository
            _mockRepository = new MockRepository(MockBehavior.Default);

            // Setup Mock
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
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        public void Cannot_CreateFieldDefinition_WithCircularReference(int depthLevel)
        {
            //Arrange
            var listFieldDefinitions = new List<FieldDefinition>();
            var baseFieldName = "BaseField0";
            var baseFieldDefinition = new FieldDefinitionBuilder().WithName(baseFieldName).WithIsReferenceType(true).Build();

            var nestedFieldDefinition = baseFieldDefinition;
            for (var i = 0; i < depthLevel; i++)
            {
                var fieldName = "BaseField" + (i + 1);

                nestedFieldDefinition = new FieldDefinitionBuilder().WithName(fieldName).WithIsReferenceType(true).
                    WithFieldTypeId(nestedFieldDefinition.ContentTypeId).
                    WithFieldDefinitionId(nestedFieldDefinition.FieldDefinitionId + 1).
                    WithContentTypeId(nestedFieldDefinition.ContentTypeId + 1).Build();
                
                listFieldDefinitions.Add(nestedFieldDefinition);
            }
            baseFieldDefinition.FieldTypeId = nestedFieldDefinition.ContentTypeId;
            listFieldDefinitions.Add(baseFieldDefinition);

            var fieldDefinitionCounter = listFieldDefinitions.Count - 1;
            if (depthLevel > 0)
            {
                _mockFieldDefinitionManager.Setup(m => m.GetFieldDefinitions(It.IsAny<int>()))
                    .Returns(() => new List<FieldDefinition> {listFieldDefinitions[fieldDefinitionCounter]}.AsQueryable())
                    .Callback(() => fieldDefinitionCounter--);
            }
            //Act 
            string errorMessage;
            var isValid = FieldDefinitionChecker.Instance.IsValid(baseFieldDefinition, out errorMessage);

            //Assert
            Assert.IsFalse(isValid, "The field definition containing a dead loop is valid");
            Assert.AreEqual(string.Format("It is not posible to create the field with name {0} because it would create a dead loop definition.",
                baseFieldName), errorMessage, "Error message is not the expected one");
            _mockRepository.VerifyAll();
        }
    }
}
